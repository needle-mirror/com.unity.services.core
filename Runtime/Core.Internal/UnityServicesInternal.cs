using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using NotNull = JetBrains.Annotations.NotNullAttribute;

namespace Unity.Services.Core.Internal
{
    /// <summary>
    /// Utility to initialize all Unity services from a single endpoint.
    /// </summary>
    class UnityServicesInternal : IUnityServices
    {
        /// <summary>
        /// Initialization state.
        /// </summary>
        public ServicesInitializationState State { get; private set; }

        public InitializationOptions Options { get; private set; }

        internal bool CanInitialize;

        TaskCompletionSource<object> m_Initialization;

        [NotNull]
        CoreRegistry Registry { get; }

        [NotNull]
        CoreMetrics Metrics { get; }

        [NotNull]
        CoreDiagnostics Diagnostics { get; }

        public UnityServicesInternal([NotNull] CoreRegistry registry, [NotNull] CoreMetrics metrics, [NotNull] CoreDiagnostics diagnostics)
        {
            Registry = registry;
            Metrics = metrics;
            Diagnostics = diagnostics;
        }

        /// <summary>
        /// Single entry point to initialize all used services.
        /// </summary>
        /// <param name="options">
        /// The options to customize services initialization.
        /// </param>
        /// <returns>
        /// Return a handle to the initialization operation.
        /// </returns>
        public async Task InitializeAsync(InitializationOptions options)
        {
            if (!HasRequestedInitialization()
                || HasInitializationFailed())
            {
                Options = options;
                m_Initialization = new TaskCompletionSource<object>();
            }

            if (!CanInitialize
                || State != ServicesInitializationState.Uninitialized)
            {
                await m_Initialization.Task;
            }
            else
            {
                await InitializeServicesAsync();
            }

            bool HasInitializationFailed()
            {
                return m_Initialization.Task.IsCompleted
                    && m_Initialization.Task.Status != TaskStatus.RanToCompletion;
            }
        }

        bool HasRequestedInitialization()
        {
            return !(m_Initialization is null);
        }

        async Task InitializeServicesAsync()
        {
            State = ServicesInitializationState.Initializing;
            var initStopwatch = new Stopwatch();
            initStopwatch.Start();
            var sortedPackageTypeHashes = new List<int>(
                Registry.PackageRegistry.Tree?.PackageTypeHashToInstance?.Count ?? 0);

            try
            {
                SortPackages();
            }
            catch (Exception reason)
            {
                FailServicesInitialization(reason);
                throw;
            }

            try
            {
                await InitializePackagesAsync();
            }
            catch (Exception reason)
            {
                FailServicesInitialization(reason);
                throw;
            }

            SucceedServicesInitialization();

            void SortPackages()
            {
                var sorter = new DependencyTreeInitializeOrderSorter(
                    Registry.PackageRegistry.Tree, sortedPackageTypeHashes);
                sorter.SortRegisteredPackagesIntoTarget();
            }

            async Task InitializePackagesAsync()
            {
                var initializer = new CoreRegistryInitializer(Registry, sortedPackageTypeHashes);
                await initializer.InitializeRegistryAsync();
            }

            void FailServicesInitialization(Exception e)
            {
                State = ServicesInitializationState.Uninitialized;
                initStopwatch.Stop();
                m_Initialization.TrySetException(e);

                if (e is CircularDependencyException)
                {
                    Diagnostics.SendCircularDependencyDiagnostics(e);
                }
                else
                {
                    Diagnostics.SendOperateServicesInitDiagnostics(e);
                }
            }

            void SucceedServicesInitialization()
            {
                State = ServicesInitializationState.Initialized;
                Registry.LockComponentRegistration();
                initStopwatch.Stop();
                m_Initialization.TrySetResult(null);

                Metrics.SendAllPackagesInitSuccessMetric();
                Metrics.SendAllPackagesInitTimeMetric(initStopwatch.Elapsed.TotalSeconds);
            }
        }

        internal async Task EnableInitializationAsync()
        {
            CanInitialize = true;

            Registry.LockPackageRegistration();

            if (!HasRequestedInitialization())
                return;

            await InitializeServicesAsync();
        }
    }
}
