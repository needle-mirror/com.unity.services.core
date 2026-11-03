using Unity.Services.Core.Editor.Environments;
using Unity.Services.Core.Editor.Releases.UI;
using Unity.Services.Core.Editor.Settings;
using UnityEditor;

namespace Unity.Services.Core.Editor.Releases
{
    [InitializeOnLoad]
    static class ReleaseSettingsInitializer
    {
        static ReleaseSettingsInitializer()
        {
            EnvironmentsAndReleasesSettingsProvider.s_TitleSuffix = " & " + ReleasesConstants.ServiceName;
            EnvironmentsAndReleasesSettingsProvider.s_ReleaseUiSetup = (environmentSelector, envService) =>
            {
                var releaseService = ReleaseServiceRegistry.Instance.GetService<IReleasesService>();
                new ReleaseSelector(environmentSelector, releaseService, envService);
            };
        }
    }
}
