using Fanex.Bettype3rdProduct.Business.Models;
using Fanex.Bettype3rdProduct.Business.Services;
using Fanex.Data;
using Fanex.Data.Repository;
using Fanex.Logging;
using Fanex.Logging.Sentry.RavenClient;
using Fanex.MailDelivery.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using OfficeOpenXml;
using SharpRaven;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LicenseContext = OfficeOpenXml.LicenseContext;

namespace Fanex.Bettype3rdProduct.Service;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection Services { get; set; }

    public static void LoadConfiguration(this IServiceCollection services)
    {
        Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
        var text = File.ReadAllText("appsettings.json");
        AppConfigService.Config = JsonConvert.DeserializeObject<ConfigSettingModel>(text);
    }

    public static void SentryRegister(this IServiceCollection services)
    {
        var sentry = new SentryLogging(new SentryEngineOptions
        {
            Dsn = new Dsn(AppConfigService.Config.SentryClientKey)
        });

        LogManager
            .SetDefaultLogCategory(AppConfigService.Config.CategorySentryLog)
            .Use(new List<ILogEngine>
            {
                sentry
            });
    }

    public static void FanexDataRegister(this IServiceCollection services, IConfiguration config)
    {
        var connections = config.GetSection("ConnectionStrings")
            .GetChildren()
            .AsEnumerable()
            .ToDictionary
            (
                x => x.Key,
                y => y.Value
            );

        DbSettingProviderManager
            .StartNewSession()
            .UseConnectionStrings(connections)
            .UseDefaultDbSettingProvider(config.GetSection("StoredProcedurePath").Value)
            .Run();
    }

    public static void InternalServiceRegister(this IServiceCollection services)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        services.AddSingleton<Log, LogImp>();
        services.AddSingleton<IDynamicRepository, DynamicRepository>();
        services.AddSingleton<DataService, DataServiceImp>();
        services.AddSingleton<DateProviderService, DateProviderServiceImp>();
        services.AddSingleton<SportSplitterService, SportSplitterServiceImp>();
        services.AddSingleton<ExcelService, ExcelServiceImp>();
        services.AddSingleton<ExcelHeaderService, ExcelHeaderServiceImp>();
        services.AddSingleton<MailService, MailServiceImp>();
        services.AddSingleton<MainService, MainServiceImp>();
        services.AddSingleton<RawDataSourceService, RawDataSourceServiceImp>();
        services.AddSingleton<RepositoryBettypeProductService, RepositoryServiceImp>();

        Services = services;
        MailClientHandler.Start();
    }
}