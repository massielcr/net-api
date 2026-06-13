using HttpClientMethods.Dtos;
using HttpClientMethods.Endpoints;
using PluralsightKDStreams.Endpoints;
using PluralsightKDStreams.Interfaces;
using PluralsightKDStreams.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddRequestDecompression();

builder.Services.AddHttpClient("Local", client =>
{
    client.BaseAddress = new Uri("http://localhost:5217");
})
.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    AutomaticDecompression = System.Net.DecompressionMethods.GZip
});

builder.Services.AddHttpClient();


builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
    options.SerializerOptions.WriteIndented = true;
});

builder.Services.AddSingleton<ICancellationService, CancellationService>();

builder.Services.AddScoped<IStreamerService, StreamerService>();




var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseRequestDecompression();

app.UseExceptionHandler(exceptionApp =>
{
    exceptionApp.Run(async context =>
    {
        var exceptionHandlerPathFeature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerPathFeature>();
        var exception = exceptionHandlerPathFeature?.Error;

        // Resolve the logger from the app container
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogError(exception, "An unhandled exception occurred during the request.");

        // Return a clean, safe JSON error payload to the client
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsJsonAsync(new ErrorResponse(500, "An unexpected server error occurred. Please try again later."));
    });
});

app.MapStreamerEndpoints();
app.MapCancellationEndpoints();

app.Run();
