var builder = WebApplication.CreateBuilder(args);

// Structured JSON console logging (Serilog) so log fields — including the
// CorrelationId set by CorrelationIdMiddleware below — are real, filterable
// Elasticsearch fields once Filebeat ships them, not just free text.
builder.UseStoryVerseSerilog();

// Layer registration order follows codebase-architecture-flow.md section 8:
// Application -> Infrastructure -> Api-only concerns (auth, swagger, controllers).
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApiServices(builder.Configuration);

var app = builder.Build();

// Correlation ID must wrap exception handling (registered first, so its
// BeginScope stays active until the request truly finishes) or the logger
// scope would already be popped by the time an unhandled exception is
// logged further down the pipeline.
app.UseStoryVerseCorrelationId();

// Global exception handling so every downstream failure is converted into
// the standard ResponseDto<T> error envelope.
app.UseStoryVerseExceptionHandling();

if (app.Environment.IsDevelopment())
{
    app.UseStoryVerseSwagger();
}

app.UseHttpsRedirection();

app.UseStoryVerseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
