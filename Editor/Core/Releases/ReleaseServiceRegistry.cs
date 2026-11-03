using Unity.Services.Core.Editor.Environments;
using Unity.Services.Core.Editor.Releases.Save;
using Unity.Services.Core.Editor.Shared.DependencyInversion;

namespace Unity.Services.Core.Editor.Releases
{
    class ReleaseServiceRegistry : AbstractRuntimeServices<ReleaseServiceRegistry>
    {
        public ReleaseServiceRegistry()
        {
            Initialize(new ServiceCollection());
        }

        public override void Register(ServiceCollection collection)
        {
            // Shared services resolved from the Environments registry
            collection.Register(_ => EnvironmentServiceRegistry.Instance.GetService<IEnvironmentService>());
            collection.Register(_ => EnvironmentServiceRegistry.Instance.GetService<IAccessTokens>());
            collection.Register(_ => EnvironmentServiceRegistry.Instance.GetService<IFileSystem>());
            collection.Register(_ => EnvironmentServiceRegistry.Instance.GetService<IProjectInfo>());

            // Releases-specific services
            collection.RegisterSingleton(Factories.Default<IReleasesApi, ReleasesApiInternal>);
            collection.RegisterSingleton(Factories.Default<IReleasesService, ReleasesService>);
            collection.RegisterSingleton(Factories.Default<IReleaseSaveSystem, ReleaseSaveSystem>);
            collection.Register(Factories.Default<IReleaseFetcher, ReleaseClientFactory>);
            collection.RegisterSingleton(c => new ReleasesHttpClientBuilder((IAccessTokens)c.GetService(typeof(IAccessTokens))));
            collection.RegisterSingleton(Factories.Default<IReleaseProvider, ReleaseProvider>);
        }
    }
}
