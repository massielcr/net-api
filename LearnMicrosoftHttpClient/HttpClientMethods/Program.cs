using HttpClientMethods.Dtos;
using HttpClientMethods.Endpoints;
using HttpClientMethods.Helpers;
using HttpClientMethods.Services;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.AddHttpClient("GitHub", client =>
{
    client.BaseAddress = new Uri("https://api.github.com");
    client.DefaultRequestHeaders.Add("Accept", "application/vnd.github+json");
    client.DefaultRequestHeaders.Add("User-Agent", "MyHttpClientMethodsTestService");

    // Dynamically inject the token if it exists at startup
    string? token = Environment.GetEnvironmentVariable("GITHUB_TOKEN");
    if (!string.IsNullOrEmpty(token))
    {
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
    }
});

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
    options.SerializerOptions.WriteIndented = true;
});

builder.Services.AddHttpClient();

builder.Services.AddSingleton<CancellationManager>();
builder.Services.AddSingleton<IFileService, FileService>();

builder.Services.AddScoped<IGetAsyncEndpointsService,  GetAsyncEndpointsService>();
builder.Services.AddScoped<IGetStreamAsyncEndpointsService, GetStreamAsyncEndpointsService>();
builder.Services.AddScoped<IGetByteArrayAsyncEndpointsService, GetByteArrayAsyncEndpointsService>();
builder.Services.AddScoped<IGetStringAsyncEndpointsService, GetStringAsyncEndpointsService>();
builder.Services.AddScoped<IPostAsyncEndpointsService, PostAsyncEndpointsService>();
builder.Services.AddScoped<ISendAsyncEndpointsService, SendAsyncEndpointsService>();




// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseStaticFiles();

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

app.MapGetAsyncEndpoints();
app.MapGetStreamAsyncEndpoints();
app.MapGetByteArrayAsyncEndpoints();
app.MapGetStringAsyncEndpoints();
app.MapPostAsyncEndpoints();
app.MapSendAsyncEndpoints();
app.MapCancellationEndpoints();



app.Run();



