using System.Threading.Tasks;
using Unity.Services.Core.Editor.Environments;
using Unity.Services.Core.Editor.Shared.Clients;
using Unity.Services.Core.Internal;
using Unity.Services.Core.Releases.Client.Http;

namespace Unity.Services.Core.Editor.Releases
{
    class ReleasesHttpClientBuilder
    {
        const string k_ProductionUrl = "https://services.api.unity.com/live-releases/v1";
        const string k_StagingUrl = "https://staging.services.api.unity.com/live-releases/v1";

        readonly IAccessTokens m_AccessTokens;

        public ReleasesHttpClientBuilder(IAccessTokens accessTokens)
        {
            m_AccessTokens = accessTokens;
        }

        public async Task<Core.Releases.Client.Configuration> BuildConfiguration()
        {
            var gatewayToken = await m_AccessTokens.GetServicesGatewayTokenAsync();

            if (gatewayToken == null)
            {
                CoreLogger.LogWarning("[Releases] No services gateway token available — editor may not be logged in.");
                return null;
            }

            var env = new CloudEnvironmentConfigProvider();
            string baseUrl = GetLiveReleasesBaseUrl(env.IsStaging());

            var headers = new AdminApiHeaders<ReleasesHttpClientBuilder>(gatewayToken);
            return new Core.Releases.Client.Configuration(baseUrl, null, null, headers.ToDictionary());
        }

        internal static string GetLiveReleasesBaseUrl(bool isStaging)
        {
            return isStaging ? k_StagingUrl : k_ProductionUrl;
        }
    }
}
