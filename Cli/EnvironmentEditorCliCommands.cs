#if UNITY_PIPELINE_AVAILABLE && UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Pipeline.Commands;
using Unity.Services.Core.Cli;

namespace Unity.Services.Core.Editor.Environments
{
    /// <summary>
    /// Pipeline CLI commands for managing the active Unity Services environment.
    /// Only compiled when the <c>com.unity.pipeline</c> package is present
    /// (gated by the <c>UNITY_PIPELINE_AVAILABLE</c> version define on the asmdef).
    /// </summary>
    static class EnvironmentEditorCliCommands
    {
        [CliGroupCommand(CoreCliConstants.EnvironmentGroup, "Get the name of the active Unity Services environment")]
        public static string GetActiveName()
        {
            return EnvironmentsApi.Instance.ActiveEnvironmentName ?? string.Empty;
        }

        [CliGroupCommand(CoreCliConstants.EnvironmentGroup, "Get the id (guid) of the active Unity Services environment")]
        public static string GetActiveId()
        {
            return EnvironmentsApi.Instance.ActiveEnvironmentId?.ToString() ?? string.Empty;
        }

        [CliGroupCommand(CoreCliConstants.EnvironmentGroup, "List the Unity Services environments available for the project")]
        public static async Task<List<EnvironmentDto>> List()
        {
            var api = EnvironmentsApi.Instance;
            await api.RefreshAsync();
            var environments = api.Environments ?? (IReadOnlyCollection<EnvironmentInfo>)Array.Empty<EnvironmentInfo>();
            return environments.Select(EnvironmentDto.From).ToList();
        }

        [CliGroupCommand(CoreCliConstants.EnvironmentGroup, "Create a Unity Services environment for the active project")]
        public static async Task<EnvironmentDto> Create(
            [CliArg("name", "The name of the environment to create", Required = true)] string name)
        {
            var api = EnvironmentsApi.Instance;
            var created = await api.CreateEnvironmentAsync(name);
            return EnvironmentDto.From(created);
        }

        [CliGroupCommand(CoreCliConstants.EnvironmentGroup, "Delete a Unity Services environment for the active project by name")]
        public static async Task<List<EnvironmentDto>> Delete(
            [CliArg("name", "The name of the environment to delete", Required = true)] string name)
        {
            var api = EnvironmentsApi.Instance;
            await api.DeleteEnvironmentAsync(name);
            var environments = api.Environments ?? (IReadOnlyCollection<EnvironmentInfo>)Array.Empty<EnvironmentInfo>();
            return environments.Select(EnvironmentDto.From).ToList();
        }

        [CliGroupCommand(CoreCliConstants.EnvironmentGroup, "Set the active Unity Services environment by name")]
        public static async Task<EnvironmentDto> SetActiveByName(
            [CliArg("name", "The name of the environment to set as active", Required = true)] string name)
        {
            var api = EnvironmentsApi.Instance;
            await api.RefreshAsync();
            api.SetActiveEnvironment(name);
            return ActiveEnvironment(api);
        }

        [CliGroupCommand(CoreCliConstants.EnvironmentGroup, "Set the active Unity Services environment by id (guid)")]
        public static async Task<EnvironmentDto> SetActiveById(
            [CliArg("id", "The id (guid) of the environment to set as active", Required = true)] string id)
        {
            if (!Guid.TryParse(id, out var environmentGuid))
            {
                throw new ArgumentException($"'{id}' is not a valid environment id (guid).", nameof(id));
            }

            var api = EnvironmentsApi.Instance;
            await api.RefreshAsync();
            api.SetActiveEnvironment(environmentGuid);
            return ActiveEnvironment(api);
        }

        static EnvironmentDto ActiveEnvironment(IEnvironmentsApi api)
        {
            var activeId = api.ActiveEnvironmentId;
            var isDefault = false;
            if (activeId != null && api.Environments != null)
            {
                foreach (var environment in api.Environments)
                {
                    if (environment.Id == activeId.Value)
                    {
                        isDefault = environment.IsDefault;
                        break;
                    }
                }
            }

            return new EnvironmentDto
            {
                Name = api.ActiveEnvironmentName ?? string.Empty,
                Id = api.ActiveEnvironmentId?.ToString() ?? string.Empty,
                IsDefault = isDefault
            };
        }

        /// <summary>
        /// Serializable view of an <see cref="EnvironmentInfo"/> for CLI JSON output
        /// (exposes the id as a string for a stable, readable shape).
        /// </summary>
        public class EnvironmentDto
        {
            public string Name { get; set; }
            public string Id { get; set; }
            public bool IsDefault { get; set; }

            public static EnvironmentDto From(EnvironmentInfo info)
            {
                return new EnvironmentDto
                {
                    Name = info.Name,
                    Id = info.Id.ToString(),
                    IsDefault = info.IsDefault
                };
            }
        }
    }
}
#endif
