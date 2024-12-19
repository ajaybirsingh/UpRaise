using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureKeyVault;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.ApplicationInsights;
using NLog.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpRaise
{
    public class Program
    {
        public static void Main(string[] args)
        {
            System.Console.WriteLine($"--- Starting UpRaise ---");



            var logger = NLog.Web.NLogBuilder.ConfigureNLog("NLog.config").GetCurrentClassLogger();
            try
            {
                logger.Info("Starting UpRaise...");

                var host = CreateHostBuilder(logger, args).Build();
                host.Run();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Stopped program because of exception");
                System.Console.WriteLine($"Exception: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"DBGException: {ex.Message}");
                throw;
            }
            finally
            {
                // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
                NLog.LogManager.Shutdown();
            }

            logger.Info("Shutting down UpRaise...");

        }

        public static IHostBuilder CreateHostBuilder(NLog.Logger logger, string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
#if DEBUG
                    webBuilder.UseSetting(WebHostDefaults.DetailedErrorsKey, "true");
                    webBuilder.CaptureStartupErrors(true);
#endif
                    webBuilder.UseStartup<Startup>();
                })
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
                logger.Info("HostBuilder::ConfigureAppConfiguration - Start");

                //var env = hostingContext.HostingEnvironment;
                //config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                //.AddJsonFile($"appsettings.{env.EnvironmentName}.json",
                //optional: true, reloadOnChange: true);
                //config.AddEnvironmentVariables();

                
                var keyVaultEndpoint = GetKeyVaultEndpoint();
                if (!string.IsNullOrEmpty(keyVaultEndpoint))
                {

                    //az login
                    //az account set --subscription "Windows Azure MSDN - Visual Studio Ultimate"
                    //az keyvault set-policy --name UpRaise --object-id f167ccbb-0ca2-4c1e-8e86-63136f1ff465 --secret-permissions get list

                    var azureServiceTokenProvider = new AzureServiceTokenProvider();
                    var keyVaultClient = new KeyVaultClient(
                        new KeyVaultClient.AuthenticationCallback(
                            azureServiceTokenProvider.KeyVaultTokenCallback));
                    config.AddAzureKeyVault(
                        keyVaultEndpoint, keyVaultClient, new DefaultKeyVaultSecretManager());
                }

                logger.Info("HostBuilder::ConfigureAppConfiguration - Ended");

            })
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.ClearProviders();
                    //logging.AddAzureWebAppDiagnostics();
                    logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                })
               .UseNLog();  // NLog: setup NLog for Dependency injection

        //private static string GetKeyVaultEndpoint() => "https://upraise.vault.azure.net";
        private static string GetKeyVaultEndpoint() => "https://upraisenew.vault.azure.net";
       //private static string GetKeyVaultEndpoint() => "https://yogesh.vault.azure.net";

    }



}
