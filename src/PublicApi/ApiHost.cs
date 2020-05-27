// <copyright file="ApiHost.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.PublicApi
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using apache.log4net.Extensions.Logging;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using MUnique.OpenMU.Interfaces;
    using Nito.AsyncEx.Synchronous;

    /// <summary>
    /// Hosts the public API server.
    /// </summary>
    public class ApiHost
    {
        private readonly IHost host;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiHost"/> class.
        /// </summary>
        /// <param name="gameServers">The game servers.</param>
        /// <param name="connectServers">The connect servers.</param>
        /// <param name="loggingConfigurationPath">The path to the logging configuration.</param>
        /// <returns>The async task.</returns>
        public ApiHost(ICollection<IGameServer> gameServers, IEnumerable<IConnectServer> connectServers, string? loggingConfigurationPath)
        {
            this.host = BuildHost(gameServers, connectServers, loggingConfigurationPath);
        }

        /// <summary>
        /// Creates the Host instance.
        /// </summary>
        /// <param name="gameServers">The game servers.</param>
        /// <param name="connectServers">The connect servers.</param>
        /// <param name="loggingConfigurationPath">The path to the logging configuration.</param>
        /// <returns>The created host.</returns>
        public static IHost BuildHost(ICollection<IGameServer> gameServers, IEnumerable<IConnectServer> connectServers, string? loggingConfigurationPath)
        {
            var builder = Host.CreateDefaultBuilder();
            if (!string.IsNullOrEmpty(loggingConfigurationPath))
            {
                builder = builder.ConfigureLogging(configureLogging =>
                {
                    configureLogging.ClearProviders();
                    var settings = new Log4NetSettings { ConfigFile = loggingConfigurationPath, Watch = true };

                    configureLogging.AddLog4Net(settings);
                });
            }

            return builder
                .ConfigureServices(s =>
                {
                    s.Add(new ServiceDescriptor(typeof(ICollection<IGameServer>), gameServers));
                    s.Add(new ServiceDescriptor(typeof(IEnumerable<IConnectServer>), connectServers));
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                        .UseStartup<Startup>()
                        .UseUrls("http://*:80", "https://*:443");
                })
                .Build();
        }

        /// <summary>
        /// Creates and runs a host instance.
        /// </summary>
        /// <param name="gameServers">The game servers.</param>
        /// <param name="connectServers">The connect servers.</param>
        /// <param name="loggingConfigurationPath">The path to the logging configuration.</param>
        public static Task RunAsync(ICollection<IGameServer> gameServers, IEnumerable<IConnectServer> connectServers, string? loggingConfigurationPath)
        {
            return BuildHost(gameServers, connectServers, loggingConfigurationPath).StartAsync();
        }

        /// <summary>
        /// Start the host.
        /// </summary>
        public void Start()
        {
            this.host.Start();
        }

        /// <summary>
        /// Stops the host.
        /// </summary>
        public void Shutdown()
        {
            this.host.StopAsync().WaitAndUnwrapException();
            (this.host as IDisposable).Dispose();
        }
    }
}
