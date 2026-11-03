using System.Collections.Generic;
using System.Threading.Tasks;

namespace Unity.Services.Core.Editor.Environments
{
    interface IEnvironmentFetcher
    {
        Task<List<EnvironmentInfo>> FetchEnvironments();
        Task<EnvironmentInfo?> FetchEnvironment(string environmentId);
        Task<EnvironmentInfo?> CreateEnvironment(string name);
        Task<bool> DeleteEnvironment(string environmentId);
    }
}
