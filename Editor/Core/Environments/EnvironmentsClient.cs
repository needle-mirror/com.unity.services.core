using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Services.Core.Environments.Client.Apis.Default;
using Unity.Services.Core.Environments.Client.Default;

namespace Unity.Services.Core.Editor.Environments
{
    class EnvironmentsClient
    {
        readonly string m_ProjectId;
        readonly IDefaultApiClient m_EnvironmentClient;

        public EnvironmentsClient(string projectId, IDefaultApiClient client)
        {
            m_ProjectId = projectId;
            m_EnvironmentClient = client;
        }

        public async Task<List<EnvironmentInfo>> GetEnvironments()
        {
            var request = new UnityGetEnvironmentsRequest(m_ProjectId);
            var response = await m_EnvironmentClient.UnityGetEnvironmentsAsync(request);
            return response.Result.Results.Select(unityEnvironment => new EnvironmentInfo(unityEnvironment)).ToList();
        }

        public async Task<EnvironmentInfo> GetEnvironment(string environmentId)
        {
            var request = new UnityGetEnvironmentRequest(m_ProjectId, environmentId);
            var response = await m_EnvironmentClient.UnityGetEnvironmentAsync(request);
            return new EnvironmentInfo(response.Result);
        }

        public async Task<EnvironmentInfo> CreateEnvironment(string name)
        {
            var body = new Unity.Services.Core.Environments.Client.Models.UnityCreateEnvironmentRequest(name);
            var request = new UnityCreateEnvironmentRequest(m_ProjectId, body);
            var response = await m_EnvironmentClient.UnityCreateEnvironmentAsync(request);
            return new EnvironmentInfo(response.Result);
        }

        public async Task DeleteEnvironment(string environmentId)
        {
            var request = new UnityDeleteEnvironmentRequest(m_ProjectId, environmentId);
            await m_EnvironmentClient.UnityDeleteEnvironmentAsync(request);
        }
    }
}
