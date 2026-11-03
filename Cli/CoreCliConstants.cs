namespace Unity.Services.Core.Cli
{
    /// <summary>
    /// The constants this package's CLI commands use: their groups, tags, and the other literals they depend on,
    /// so each has one source of truth. Keep one class per package: a const read from another assembly is
    /// compiled into the reader, so a shared class would go stale in every package not rebuilt with it.
    /// </summary>
    static class CoreCliConstants
    {
        public const string Group = "services.core";
        public const string EnvironmentGroup = "services.environment";
        /// <summary>com.unity.pipeline's Play Mode command group, which failure messages point the caller at.</summary>
        public const string PlayModeGroup = "editor.playmode";
        /// <summary>com.unity.pipeline's scene command group, which failure messages point the caller at.</summary>
        public const string SceneGroup = "scene";
        /// <summary>com.unity.pipeline's component command group, which failure messages point the caller at.</summary>
        public const string ComponentGroup = "component";
        /// <summary>
        /// The GameObject the scene component commands host their component on when none is named. Must match
        /// <c>AuthenticationCliConstants.DefaultGameObjectName</c>, so both components land on one GameObject.
        /// </summary>
        public const string DefaultGameObjectName = "Unity Services";
        public const string ServicesInitializationUndoName = "Create Services Initialization";
    }
}
