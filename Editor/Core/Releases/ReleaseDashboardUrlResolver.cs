using System;
using Unity.Services.Core.Editor.Environments;
using Unity.Services.Core.Editor.OrganizationHandler;

namespace Unity.Services.Core.Editor.Releases
{
    class ReleaseDashboardUrlResolver : IReleaseDashboardUrlResolver
    {
        readonly ICloudEnvironmentConfigurationProvider m_CloudEnvironmentConfigProvider;
        readonly IOrganizationHandler m_OrganizationHandler;
        readonly IProjectInfo m_ProjectInfo;

        internal ReleaseDashboardUrlResolver(
            ICloudEnvironmentConfigurationProvider cloudEnvironmentConfigProvider,
            IOrganizationHandler organizationHandler,
            IProjectInfo projectInfo)
        {
            m_CloudEnvironmentConfigProvider = cloudEnvironmentConfigProvider;
            m_OrganizationHandler = organizationHandler;
            m_ProjectInfo = projectInfo;
        }

        public ReleaseDashboardUrlResolver()
            : this(new CloudEnvironmentConfigProvider(), new OrganizationHandler.OrganizationHandler(), new ProjectInfo())
        {
        }

        string GetHost()
        {
            return m_CloudEnvironmentConfigProvider.IsStaging()
                ? "https://staging.cloud.unity.com"
                : "https://cloud.unity.com";
        }

        public string ManageReleasePointers(Guid environmentId)
        {
            var host = GetHost();
            return $"{host}/home/organizations/{m_OrganizationHandler.Key}/projects/{m_ProjectInfo.ProjectId}/environments/{environmentId}/releases/overview?tab=releasePointers";
        }

        public string ManageReleases(Guid environmentId)
        {
            var host = GetHost();
            return $"{host}/home/organizations/{m_OrganizationHandler.Key}/projects/{m_ProjectInfo.ProjectId}/environments/{environmentId}/releases/overview";
        }
    }
}
