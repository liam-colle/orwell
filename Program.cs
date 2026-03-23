using orwell;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHttpClient<orwell.EndpointMonitorService>();
builder.Services.AddHostedService<orwell.EndpointMonitorService>();

var host = builder.Build();
host.Run();
