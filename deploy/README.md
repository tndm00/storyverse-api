# Deploy — StoryVerse backend on Google Cloud Run

Six services, one container each. Build context is the **repo root** (`story-be-prj/`).

```
deploy/
├── <service>/Dockerfile     x6  — runtime image (non-root, listens on $PORT)
├── migrate/Dockerfile           — one-shot `dotnet ef database update` runner
├── cloudbuild.yaml              — build + push + deploy one service
└── deploy.sh                    — wrapper: ./deploy.sh <service|all>
```

| service | folder | Cloud Run name | connection string key | migrate env var | migration |
| --- | --- | --- | --- | --- | --- |
| authentication | `Authentications` | `storyverse-authentication` | `AuthenticationDatabase` | `AUTH_DB_CONNECTION` | `AddUserRoles` (+ initial) |
| content | `Contents` | `storyverse-content` | `ContentDatabase` | `CONTENT_DB_CONNECTION` | `InitialContentSchema` |
| community | `Communities` | `storyverse-community` | `CommunityDatabase` | `COMMUNITY_DB_CONNECTION` | `InitialCommunitySchema` |
| library | `Libraries` | `storyverse-library` | `LibraryDatabase` | `LIBRARY_DB_CONNECTION` | `InitialLibrarySchema` |
| moderation | `Moderations` | `storyverse-moderation` | `ModerationDatabase` | `MODERATION_DB_CONNECTION` | `InitialModerationSchema` |
| notification | `Notifications` | `storyverse-notification` | `NotificationDatabase` | `NOTIFICATION_DB_CONNECTION` | `InitialNotificationSchema` |

All six services now have a `DbContext` + design-time factory + an initial migration and
build clean. The `INFRA_PROJECT` build arg for `deploy/migrate/Dockerfile` is
`src/Services/<folder>/<Prefix>.Infrastructure` (e.g. `src/Services/Communities/Community.Infrastructure`).

---

## 0. One-time GCP setup

```bash
PROJECT=your-project ; REGION=asia-southeast1 ; REPO=storyverse
gcloud config set project "$PROJECT"

gcloud services enable run.googleapis.com cloudbuild.googleapis.com \
  artifactregistry.googleapis.com secretmanager.googleapis.com

gcloud artifacts repositories create "$REPO" \
  --repository-format=docker --location="$REGION"

# let Cloud Build deploy to Cloud Run
PROJECT_NUM=$(gcloud projects describe "$PROJECT" --format='value(projectNumber)')
gcloud projects add-iam-policy-binding "$PROJECT" \
  --member="serviceAccount:${PROJECT_NUM}@cloudbuild.gserviceaccount.com" \
  --role=roles/run.admin
gcloud projects add-iam-policy-binding "$PROJECT" \
  --member="serviceAccount:${PROJECT_NUM}@cloudbuild.gserviceaccount.com" \
  --role=roles/iam.serviceAccountUser
```

## 1. Database

Cloud Run has no database — use **Cloud SQL for PostgreSQL**, **Neon**, or **Supabase**.
Create one database per service (or one server with 6 databases / schemas).

Connection string format (Npgsql):
```
Host=<host>;Port=5432;Database=storyverse_content;Username=<u>;Password=<p>;SSL Mode=Require;Trust Server Certificate=true
```
For Cloud SQL via the built-in connector use the unix-socket host
`Host=/cloudsql/<PROJECT>:<REGION>:<INSTANCE>` and add
`--add-cloudsql-instances=<PROJECT>:<REGION>:<INSTANCE>` to the deploy.

## 2. Secrets

```bash
# JWT signing key — the SAME value for every service that validates tokens
printf '%s' "$(openssl rand -base64 48)" | gcloud secrets create jwt-signing-key --data-file=-

# one DB connection string per service
printf '%s' "Host=...;Database=storyverse_content;..." | gcloud secrets create content-db --data-file=-
printf '%s' "Host=...;Database=storyverse_authentication;..." | gcloud secrets create auth-db --data-file=-
```

## 3. Migrate (before deploying a service with schema changes)

Build the migrator once per service and run it as a Cloud Run **Job**:

```bash
REGION=asia-southeast1 ; REPO=storyverse
IMG="$REGION-docker.pkg.dev/$PROJECT/$REPO/migrate-content:latest"

gcloud builds submit --tag "$IMG" \
  --build-arg INFRA_PROJECT=src/Services/Contents/Content.Infrastructure .
# (or: docker build -f deploy/migrate/Dockerfile --build-arg INFRA_PROJECT=... -t "$IMG" . && docker push "$IMG")

gcloud run jobs deploy migrate-content \
  --image "$IMG" --region "$REGION" \
  --set-env-vars DB_ENV_VAR=CONTENT_DB_CONNECTION \
  --set-secrets DB_CONNECTION=content-db:latest \
  --max-retries 1 --task-timeout 10m
gcloud run jobs execute migrate-content --region "$REGION" --wait
```

## 4. Deploy a service

```bash
./deploy/deploy.sh content        # or: authentication | all
```

Then set env + secrets on the service (once; preserved on later deploys):

```bash
gcloud run services update storyverse-content --region "$REGION" \
  --set-secrets \
     "ConnectionStrings__ContentDatabase=content-db:latest,GcpSettings__AuthSettings__SecretKey=jwt-signing-key:latest" \
  --set-env-vars \
     "GcpSettings__AuthSettings__Issuer=Authentication.Api,GcpSettings__AuthSettings__Audiences__0=StoryVerse.Web,GcpSettings__AuthSettings__Audiences__1=StoryVerse.Mobile"
```

Authentication also needs `GcpSettings__GoogleAuthSettings__ClientId=<oauth-client-id>` (public, plain env var).

Verify:
```bash
URL=$(gcloud run services describe storyverse-content --region "$REGION" --format='value(status.url)')
curl "$URL/health"                 # -> 200
curl "$URL/v1/stories"             # -> 200 (published-only)
```

## 5. Wire the gateway

Put each service URL into `storyverse/gateway/wrangler.toml` `[env.production.vars]`
(`CONTENT_URL`, `AUTHENTICATION_URL`, …) and `npm run deploy` the Worker.
Deploy services one at a time — a URL left `""` in the gateway just returns 503.

---

## Notes

- `--allow-unauthenticated` opens the service to the internet. Lock it down by
  removing that flag and fronting everything with the Cloudflare Worker + an
  identity token, or Cloud Run's own IAM, once the gateway is stable.
- `--min-instances=0` = scale to zero (cheap, ~1–2s cold start). Bump to `1` for
  the auth service if cold starts hurt login latency.
- The images run as UID 1654 (non-root) and honour `SIGTERM` for graceful drain.
- `ASPNETCORE_FORWARDEDHEADERS_ENABLED=true` is baked in so the app sees the real
  scheme/host behind Cloud Run + Cloudflare.
