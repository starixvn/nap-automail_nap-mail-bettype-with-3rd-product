using Fanex.Bettype3rdProduct.Service;
using Fanex.Data.SqlServer;
using Fanex.Logging.Sentry.RavenClient;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var configuration = builder.Configuration;

services.AddControllers();
services.AddEndpointsApiExplorer();
services.AddSwaggerGen();
services.AddConfig();
services.AddFanexLogSentry(configuration);
services.AddFanexSql(configuration);
services.AddInternalServices();

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseDeveloperExceptionPage();
app.UseSwagger();
app.UseSwaggerUI();

app.UseRouting();

app.MapControllers();

await app.RunAsync();