using System;
using Unity.Services.Core.Editor.Settings;

namespace Unity.Services.Core.Editor.Environments.Save
{
    interface IEnvironmentSaveSystem
    {
        DateTime GetSettingsFileWriteTime();
        void SaveEnvironment(EnvironmentSettings environment);
        EnvironmentSettings LoadEnvironment();
    }
}
