var builder = WebApplication.CreateBuilder(args);

// Layer registration order follows codebase-architecture-flow.md section 8:
// Application -> Infrastructure -> Api-only concerns (auth, swagger, controllers).
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApiServices(builder.Configuration);

var app = builder.Build();

// Global exception handling must run first so every downstream failure is
// converted into the standard ResponseDto<T> error envelope.
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
