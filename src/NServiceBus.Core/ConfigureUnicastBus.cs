namespace NServiceBus
{
    using System;
    using Config;
    using Timeout;
    using Transports;
    using Unicast.Config;
    using Unicast.Publishing;
    using Unicast.Queuing;
    using Unicast.Subscriptions;

    /// <summary>
    /// Contains extension methods to NServiceBus.Configure.
    /// </summary>
    public static class ConfigureUnicastBus
    {
        /// <summary>
        /// Use unicast messaging (your best option on nServiceBus right now).
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public static ConfigUnicastBus UnicastBus(this Configure config)
        {
            if (Instance == null)
            {
                Instance = new ConfigUnicastBus();
                Instance.Configure(config);
            }

            return Instance;
        }

        /// <summary>
        /// Return Timeout Manager Address. Uses "TimeoutManagerAddress" parameter form config file if defined, if not, uses "EndpointName.Timeouts".
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public static Address GetTimeoutManagerAddress(this Configure config)
        {
            var unicastConfig = Configure.GetConfigSection<UnicastBusConfig>();

            if ((unicastConfig != null) && (!String.IsNullOrWhiteSpace(unicastConfig.TimeoutManagerAddress)))
                return Address.Parse(unicastConfig.TimeoutManagerAddress);

            return config.GetMasterNodeAddress().SubScope("Timeouts");
        }

        internal static ConfigUnicastBus Instance { get; private set; }
    }

    class EnsureLoadMessageHandlersWasCalled : INeedInitialization
    {
        void INeedInitialization.Init()
        {
            if (ConfigureUnicastBus.Instance != null)
                if (!ConfigureUnicastBus.Instance.LoadMessageHandlersCalled)
                    ConfigureUnicastBus.Instance.LoadMessageHandlers();
        }
    }

    class EnsureDefaultSubscriptionManager : IWantToRunBeforeConfigurationIsFinalized
    {

        public void Run()
        {
            if (!Configure.Instance.Configurer.HasComponent<IManageSubscriptions>())
                Configure.Instance.Configurer.ConfigureComponent<MessageDrivenSubscriptionManager>(
                    DependencyLifecycle.SingleInstance);

        }
    }


    class EnsureDefaultPublisher : IWantToRunBeforeConfigurationIsFinalized
    {

        public void Run()
        {
            if (!Configure.Instance.Configurer.HasComponent<IPublishMessages>())
                Configure.Instance.Configurer.ConfigureComponent<StorageDrivenPublisher>(DependencyLifecycle.InstancePerCall);

        }
    }
}
