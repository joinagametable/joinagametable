using System;
using System.Reflection;
using EtcdNet;
using log4net;
using Microsoft.Extensions.Configuration;

namespace JoinAGameTable.Extensions
{
    /// <summary>
    /// This extension adds new methods to the configuration builder.
    /// </summary>
    /// <seealso cref="IConfigurationBuilderExtension"/>
    public static class IConfigurationBuilderExtension
    {
        /// <summary>
        /// Add etcd remote configuration.
        /// </summary>
        /// <param name="configurationBuilder">Configuration builder</param>
        /// <param name="configurationOptions">Etcd options</param>
        /// <returns>Configuration builder</returns>
        public static IConfigurationBuilder AddEtcdConfiguration(this IConfigurationBuilder configurationBuilder,
                                                                 EtcdConfigurationOptions configurationOptions)
        {
            return configurationBuilder.Add(new EtcdConfigurationSource(configurationOptions));
        }
    }

    /// <summary>
    /// Etcd configuration options.
    /// </summary>
    public class EtcdConfigurationOptions
    {
        /// <summary>
        /// Node URLs.
        /// </summary>
        public string[] Urls { get; set; }

        /// <summary>
        /// Username.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Password.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Root path.
        /// </summary>
        public string RootPath { get; set; }

        /// <summary>
        /// Ignore SSL certificate error.
        /// </summary>
        public bool IgnoreCertificateError { get; set; }
    }

    /// <summary>
    /// Etcd configuration source.
    /// </summary>
    public class EtcdConfigurationSource : IConfigurationSource
    {
        /// <summary>
        /// Etcd options.
        /// </summary>
        public EtcdConfigurationOptions Options { get; }

        /// <summary>
        /// Build a new instance.
        /// </summary>
        /// <param name="options">Etcd options</param>
        public EtcdConfigurationSource(EtcdConfigurationOptions options)
        {
            Options = options;
        }

        /// <summary>
        /// Build the configuration provider.
        /// </summary>
        /// <param name="builder">Handle to the configuration builder</param>
        /// <returns></returns>
        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new EtcdConfigurationProvider(this);
        }
    }

    /// <summary>
    /// Etcd configuration provider.
    /// </summary>
    public class EtcdConfigurationProvider : ConfigurationProvider
    {
        /// <summary>
        /// Logger.
        /// </summary>
        private readonly ILog LOGGER = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Handle to etcd configuration source.
        /// </summary>
        private EtcdConfigurationSource source;

        /// <summary>
        /// Build a new instance.
        /// </summary>
        /// <param name="source">Handle to etcd configuration source</param>
        public EtcdConfigurationProvider(EtcdConfigurationSource source)
        {
            this.source = source;
        }

        public override void Load()
        {
            LOGGER.Info("Fetching remote configuration");

            try
            {
                // Options
                var options = new EtcdClientOpitions()
                {
                    Urls = source.Options.Urls,
                    Username = source.Options.Username,
                    Password = source.Options.Password,
                    UseProxy = false,
                    IgnoreCertificateError = source.Options.IgnoreCertificateError
                };

                // Initialize etcd client
                var etcdClient = new EtcdClient(options);

                // Fetch all keys
                var resp = etcdClient.GetNodeAsync(source.Options.RootPath, recursive: true, sorted: true).Result;
                if (resp.Node.Nodes != null)
                {
                    foreach (var node in resp.Node.Nodes)
                    {
                        // Store on the local configuration data storage
                        Data[node.Key] = node.Value;
                    }
                }
            }
            catch (ArgumentException ex)
            {
                LOGGER.ErrorFormat("Bad configuration: {0}", ex.Message);
                Environment.Exit(1);
            }
            catch (AggregateException ex)
            {
                LOGGER.ErrorFormat("Bad configuration: {0}", ex.Message);
                Environment.Exit(1);
            }
            catch (EtcdCommonException.KeyNotFound ex)
            {
                LOGGER.ErrorFormat("Key not found: {0}", ex.Message);
                Environment.Exit(1);
            }

            LOGGER.InfoFormat("Remote configuration fetched with success. {0} keys retrieved", Data.Count);
        }
    }
}
