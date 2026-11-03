#if UNITY_PIPELINE_AVAILABLE
using Unity.Pipeline.Commands;

namespace Unity.Services.Core.Cli
{
    /// <summary>
    /// Pipeline CLI commands that surface Unity Services runtime initialization state
    /// (<see cref="UnityServices.State"/>). Lives in a runtime (all-platforms) assembly
    /// so the on-device pipeline server can execute it in a Player build.
    /// Only compiled when the <c>com.unity.pipeline</c> package is present
    /// (gated by the <c>UNITY_PIPELINE_AVAILABLE</c> version define on the asmdef).
    /// </summary>
    /// <remarks>
    /// Unlike the other runtime-singleton commands, this one is safe to call at any time:
    /// it reports <c>Uninitialized</c> before <c>UnityServices.InitializeAsync()</c> has run,
    /// so it is the precondition check to run before commands that require initialization.
    /// </remarks>
    static class CoreServicesRuntimeCliCommands
    {
        [CliGroupCommand(CoreCliConstants.Group, "Get the Unity Services initialization state: Uninitialized, Initializing or Initialized. Only reaches Initialized after UnityServices.InitializeAsync() runs in Play Mode")]
        public static string GetInitializationState()
        {
            return UnityServices.State.ToString();
        }

        [CliGroupCommand(CoreCliConstants.Group, "Return whether Unity Services have finished initializing (UnityServices.State == Initialized). Only true after UnityServices.InitializeAsync() runs in Play Mode")]
        public static bool GetInitialized()
        {
            return UnityServices.State == ServicesInitializationState.Initialized;
        }
    }
}
#endif
