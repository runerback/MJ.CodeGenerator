using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.CommandLine;
using System.Text;
using System.Threading.Tasks;

namespace MJ.CodeGenerator.Host
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                var comands = new RootCommand("MJ Test Code Generation");

                foreach (var option in CommandLineOptions.Options)
                {
                    comands.AddOption(option);
                }

                comands.SetHandler(async context =>
                {
                    await Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder()
                        .ConfigureServices((hostContext, services) =>
                        {
                            services.AddHostedService(provider => new MJCodeGenHostedService(
                                provider,
                                new CommandLineOptions(context),
                                provider.GetRequiredService<ILogger<MJCodeGenHostedService>>()));
                        })
                        .UseSerilog((context, config) =>
                        {
                            config.WriteTo.Console();
                        })
                        .RunConsoleAsync(context.GetCancellationToken());
                });

                await comands.InvokeAsync(args);
            }
            catch (Exception ex)
            {
                var errorBuffer = Encoding.UTF8.GetBytes(ex.ToString());

                using (var output = Console.OpenStandardError(1024))
                {
                    await output.WriteAsync(errorBuffer);
                    await output.FlushAsync();
                }
            }
        }
    }
}