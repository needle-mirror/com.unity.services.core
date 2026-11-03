using System.Collections.Generic;
#if UNITY_6000_7_OR_NEWER
using Unity.Scripting.LifecycleManagement;
#endif

namespace Unity.Services.Core.Editor
{
#if UNITY_6000_7_OR_NEWER
    [NoAutoStaticsCleanup]
#endif
    class ServiceUrlBuilder : IServiceUrlBuilder
    {
        ICloudEnvironmentConfigurationProvider m_ConfigurationProvider;
        public ServiceUrlBuilder(ICloudEnvironmentConfigurationProvider configurationProvider)
        {
            m_ConfigurationProvider = configurationProvider;
        }

        public string GetRequestUri(UriPathName endpoint)
        {
            return m_ConfigurationProvider.IsStaging() ?
                "https://staging.services.unity.com/api/unity/legacy/v1" + k_UriPathParameters[endpoint] :
                "https://services.unity.com/api/unity/legacy/v1" + k_UriPathParameters[endpoint];
        }

        static readonly Dictionary<UriPathName, string> k_UriPathParameters = new Dictionary<UriPathName, string>
        {
            {UriPathName.GetCurrentUserRoleInProject, "/projects/{0}/users/me"},
        };
    }
}
