using HttpClientMethods.Methods;
using HttpClientMethods.Services;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});

builder.Services.AddHttpClient();

builder.Services.AddSingleton<CancellationManager>();

builder.Services.AddScoped<IGetEndpointsService, GetEndpointsService>();


// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGetEndpoints();



app.Run();


[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(IEnumerable<JsonElement>))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{

}
