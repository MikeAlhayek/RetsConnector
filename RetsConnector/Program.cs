using CrestApps.RetsSdk.Models;
using CrestApps.RetsSdk.Models.Enums;
using CrestApps.RetsSdk.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RetsConnector
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            using var builder = new HostBuilder()
                .ConfigureAppConfiguration((hostContext, config) =>
                {
                    string basePath = GetApplicationRoot();

                    if (string.IsNullOrWhiteSpace(basePath))
                    {
                        basePath = Directory.GetCurrentDirectory();
                    }

                    Directory.SetCurrentDirectory(basePath);
                    config.SetBasePath(basePath);
                    config.AddJsonFile("appsettings.json", true);
                    if (args != null)
                    {
                        config.AddCommandLine(args);
                    }
                })
                .ConfigureServices((hostContext, services) =>
                {
                    // 1) Register HttpClientFactory
                    services.AddHttpClient();

                    // 2) Register the IRetsRequester
                    services.AddTransient<IRetsRequester, RetsWebRequester>();

                    // 3) Register the IRetsSession
                    services.AddTransient<IRetsSession, RetsSession>();

                    // 4) Register the IRetsClient
                    services.AddTransient<IRetsClient, RetsClient>();

                    // 5) Register the ILogger
                    services.AddLogging();

                    // 6) Register the connection options
                    services.AddOptions<ConnectionOptions>()
                            .Configure((opts) =>
                            {
                                opts.UserAgent = hostContext.Configuration["Rets:UserAgent"];
                                opts.RetsServerVersion = RetsVersion.Make(hostContext.Configuration["Rets:ServerVersion"]);
                                opts.LoginUrl = hostContext.Configuration["Rets:LoginUrl"];
                                opts.Username = hostContext.Configuration["Rets:Username"];
                                opts.Password = hostContext.Configuration["Rets:Password"];
                                opts.UserAgentPassward = hostContext.Configuration["Rets:UserAgentPassward"];
                                opts.Type = Enum.Parse<AuthenticationType>(hostContext.Configuration["Rets:Type"], true);
                                opts.Timeout = TimeSpan.FromHours(1);
                            });

                    RegisterExamples(services);
                })
                .ConfigureLogging((hostContext, logging) =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                    logging.SetMinimumLevel(LogLevel.Information);
                }).Build();

            // by default we'll invoke the main Example
            string exampleNameToInvoke = args.Length > 0 ? args[0] : nameof(Example);

            await InvokeExample(exampleNameToInvoke, builder.Services);

            await builder.StopAsync();
        }

        private static void RegisterExamples(IServiceCollection services)
        {
            IEnumerable<Type> types = AppDomain.CurrentDomain.GetAssemblies()
                                 .SelectMany(assembly => assembly.GetTypes())
                                 .Where(x => !x.IsAbstract && x.IsClass && !x.IsInterface && typeof(IExample).IsAssignableFrom(x));

            foreach (Type type in types)
            {
                services.AddTransient(typeof(IExample), type);
            }
        }

        private static async Task InvokeExample(string exampleNameToInvoke , IServiceProvider serviceProvider)
        {
            IEnumerable<IExample> examples = serviceProvider.GetServices<IExample>();
            
            IExample exampleToExecute = examples.FirstOrDefault(x => x.GetType().Name.Equals(exampleNameToInvoke, StringComparison.OrdinalIgnoreCase));

            if(exampleToExecute == null)
            {
                ILogger<Program> logger = serviceProvider.GetService<ILogger<Program>>();

                logger.LogWarning($"Unable to find example with the name '{exampleNameToInvoke}'.");

                return;
            }

            await exampleToExecute.Execute();
        }

        private static string GetApplicationRoot()
        {
            var exePath = Path.GetDirectoryName(System.Reflection
                              .Assembly.GetExecutingAssembly().CodeBase);
            Regex appPathMatcher = new Regex(@"(?<!fil)[A-Za-z]:\\+[\S\s]*?(?=\\+bin)");
            var appRoot = appPathMatcher.Match(exePath).Value;
            return appRoot;
        }
    }


}
