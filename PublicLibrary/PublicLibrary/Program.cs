using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PublicLibrary;
using PublicLibrary.Repositories;
using PublicLibrary.Services;

var builder = Host.CreateApplicationBuilder(args);


builder.Services.AddTransient<IHoursRepository, HoursRepository>();
builder.Services.AddTransient<IOfficeHoursService, OfficeHoursService>();


builder.Services.AddTransient<AppRunner>();

using IHost host = builder.Build();

var app = host.Services.GetRequiredService<AppRunner>();
app!.Run();
