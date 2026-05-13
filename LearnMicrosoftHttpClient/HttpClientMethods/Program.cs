using HttpClientMethods.Endpoints;
using HttpClientMethods.Services;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
    options.SerializerOptions.WriteIndented = true;
});

builder.Services.AddHttpClient();

builder.Services.AddSingleton<CancellationManager>();

builder.Services.AddScoped<IGetAsyncEndpointsService, GetAsyncEndpointsService>();
builder.Services.AddScoped<ISendAsyncEndpointsService, SendAsyncEndpointsService>();


// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGetAsyncEndpoints();
app.MapSendAsyncEndpoints();


app.Run();


[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(IEnumerable<(string commitMessage, DateTime commitDate)>))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{

}
