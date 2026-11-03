using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Core.Editor.Environments;
using Unity.Services.Core.Internal;
using Unity.Services.Core.Releases.Client.Apis.ReleasePointers;
using Unity.Services.Core.Releases.Client.Apis.Releases;
using Unity.Services.Core.Releases.Client.Http;

namespace Unity.Services.Core.Editor.Releases
{
    class ReleaseClientFactory : IReleaseFetcher
    {
        readonly ReleasesHttpClientBuilder m_HttpClientBuilder;

        readonly IProjectInfo m_ProjectInfo;

        public ReleaseClientFactory(ReleasesHttpClientBuilder httpClientBuilder, IProjectInfo projectInfo)
        {
            m_HttpClientBuilder = httpClientBuilder;
            m_ProjectInfo = projectInfo;
        }

        public async Task<List<ReleasePointerInfo>> FetchReleasePointers(string environmentId)
        {
            var client = await Build(environmentId);
            if (client == null)
                return null;
            return await client.GetReleasePointers(environmentId);
        }

        public async Task<List<ReleaseInfo>> FetchReleases(string environmentId)
        {
            var client = await Build(environmentId);
            if (client == null)
                return null;
            return await client.GetReleases(environmentId);
        }

        async Task<ReleaseClient> Build(string environmentId)
        {
            var projectId = m_ProjectInfo.ProjectId;
            if (!IsRequestScopeKnown(projectId, environmentId))
            {
                CoreLogger.LogVerbose($"[Releases] Skipped fetch: project ID '{projectId}' or environment ID '{environmentId}' is not available yet.");
                return null;
            }

            var configuration = await m_HttpClientBuilder.BuildConfiguration();
            if (configuration == null)
                return null;

            return new ReleaseClient(
                projectId,
                new ReleasePointersApiClient(new HttpClient(), configuration),
                new ReleasesApiClient(new HttpClient(), configuration));
        }

        // Both IDs are route segments; a blank one turns the request into a gateway route miss (bare 404).
        static bool IsRequestScopeKnown(string projectId, string environmentId)
        {
            return !string.IsNullOrWhiteSpace(projectId) && !string.IsNullOrWhiteSpace(environmentId);
        }
    }
}
