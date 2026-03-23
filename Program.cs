using orwell.src;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHttpClient<EndpointMonitorService>();
builder.Services.AddHostedService<EndpointMonitorService>();

var host = builder.Build();
host.Run();
