#if UNITY_6000_4_OR_NEWER
using UnityEditor;
using UnityEngine;
using UnityEditor.Build.Profile;

namespace Unity.Services.Core.Editor.Releases
{
    [CreateAssetMenu(fileName = "ReleasesBuildProfileSettings", menuName = "Scriptable Objects/BuildProfile")]
    public class ReleasesBuildProfileSettings : ScriptableObject
    {
        static readonly string k_Tooltip = L10n.Tr("Set release pointer for this build profile");

        public string selectedReleasePointerName;
        public string selectedEnvironmentName;

        [BuildProfileSettingsProvider(typeof(ReleasesBuildProfileSettings))]
        public static BuildProfileSettingsProvider createProvider() => new BuildProfileSettingsProvider("Live Settings")
        {
            tooltip = k_Tooltip,
            canAddSetting = (BuildProfile profile) => true,
            hasCustomEditor = true
        };
    }
}
#endif
