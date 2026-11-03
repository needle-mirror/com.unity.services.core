#if UNITY_6000_7_OR_NEWER
using Unity.Scripting.LifecycleManagement;
#endif
using Unity.Services.Core.Internal.Serialization;
using UnityEngine;

namespace Unity.Services.Core.Configuration
{
#if UNITY_6000_7_OR_NEWER
    [NoAutoStaticsCleanup]
#endif
    static class ConfigurationUtils
    {
        public const string ConfigFileName = "UnityServicesProjectConfiguration.json";

        public static IConfigurationLoader ConfigurationLoader { get; internal set; }
            = new StreamingAssetsConfigurationLoader(new NewtonsoftSerializer());

        internal static void ResetStaticsOnLoad()
        {
            ConfigurationLoader = new StreamingAssetsConfigurationLoader(new NewtonsoftSerializer());
        }
    }
}
