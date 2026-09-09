#!/usr/bin/env bash
# Build + deploy one (or all) StoryVerse services to Cloud Run via Cloud Build.
#
#   ./deploy/deploy.sh content
#   ./deploy/deploy.sh all
#
# Env (override as needed):
#   REGION   default asia-southeast1
#   REPO     default storyverse   (Artifact Registry repo)
set -euo pipefail

REGION="${REGION:-asia-southeast1}"
REPO="${REPO:-storyverse}"
ALL=(authentication content community library moderation notification)

cd "$(dirname "$0")/.."

target="${1:-}"
[ -z "$target" ] && { echo "usage: $0 <service|all>"; echo "services: ${ALL[*]}"; exit 1; }

tag="$(git rev-parse --short HEAD 2>/dev/null || date +%Y%m%d%H%M%S)"

deploy_one() {
  local svc="$1"
  [ -f "deploy/$svc/Dockerfile" ] || { echo "!! no deploy/$svc/Dockerfile"; return 1; }
  echo "==> $svc  (tag $tag)"
  gcloud builds submit \
    --config deploy/cloudbuild.yaml \
    --substitutions "_SERVICE=$svc,_REGION=$REGION,_REPO=$REPO,_TAG=$tag" \
    .
}

if [ "$target" = "all" ]; then
  for s in "${ALL[@]}"; do deploy_one "$s"; done
else
  deploy_one "$target"
fi
