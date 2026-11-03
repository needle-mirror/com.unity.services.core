using System.Runtime.CompilerServices;
using UnityEngine.Scripting;

[assembly: AlwaysLinkAssembly]

// Access to AsyncOperation (deprecated)
[assembly: InternalsVisibleTo("Unity.Services.Authentication")]
[assembly: InternalsVisibleTo("Unity.Services.Authentication.Editor")]
#if UNITY_INCLUDE_TESTS
[assembly: InternalsVisibleTo("Unity.Services.Authentication.Tests")]
[assembly: InternalsVisibleTo("Unity.Services.Authentication.EditorTests")]
#endif

[assembly: InternalsVisibleTo("Unity.Services.Core.TestUtils")]

// Required for access to Networking API
[assembly: InternalsVisibleTo("Unity.Services.Core.Editor")]
[assembly: InternalsVisibleTo("Unity.Services.Core.Networking")]

// Required for CoreLogger access
[assembly: InternalsVisibleTo("Unity.Services.Core.Configuration")]
[assembly: InternalsVisibleTo("Unity.Services.Core.Configuration.Editor")]
[assembly: InternalsVisibleTo("Unity.Services.Core.Registration")]
[assembly: InternalsVisibleTo("Unity.Services.Core.Scheduler")]
[assembly: InternalsVisibleTo("Unity.Services.Core.Telemetry")]
[assembly: InternalsVisibleTo("Unity.Services.Core.Threading")]
[assembly: InternalsVisibleTo("Unity.Services.Core.Environments.Editor")]
[assembly: InternalsVisibleTo("Unity.Services.Core.Releases.Editor")]

// Test assemblies
#if UNITY_INCLUDE_TESTS
[assembly: InternalsVisibleTo("Unity.Services.Core.Tests")]
[assembly: InternalsVisibleTo("Unity.Services.Core.EditorTests.Environments")]
[assembly: InternalsVisibleTo("Unity.Services.Core.EditorTests")]
[assembly: InternalsVisibleTo("Unity.Services.Core.TestUtils.Tests")]
[assembly: InternalsVisibleTo("Unity.Services.Core.TestUtils.EditorTests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
#endif
