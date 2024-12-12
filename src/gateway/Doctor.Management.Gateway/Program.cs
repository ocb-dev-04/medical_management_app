using Doctor.Management.Gateway.Extensions;
using Doctor.Management.Gateway.Middleware;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.AddServices();

WebApplication app = builder.Build();

app.UseRateLimiter();

await app.AddDynamicRoutes();

app.UseMiddleware<AuthenticationMiddleware>();

app.MapReverseProxy();

app.Run();
