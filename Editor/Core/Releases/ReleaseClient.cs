using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Services.Core.Internal;
using Unity.Services.Core.Releases.Client.Apis.ReleasePointers;
using Unity.Services.Core.Releases.Client.Apis.Releases;
using Unity.Services.Core.Releases.Client.Http;
using Unity.Services.Core.Releases.Client.ReleasePointers;
using Unity.Services.Core.Releases.Client.Releases;

namespace Unity.Services.Core.Editor.Releases
{
    class ReleaseClient
    {
        const int k_PageSize = 100;

        readonly string m_ProjectId;
        readonly IReleasePointersApiClient m_ReleasePointersClient;
        readonly IReleasesApiClient m_ReleasesClient;

        public ReleaseClient(string projectId, IReleasePointersApiClient releasePointersClient, IReleasesApiClient releasesClient)
        {
            m_ProjectId = projectId;
            m_ReleasePointersClient = releasePointersClient;
            m_ReleasesClient = releasesClient;
        }

        public async Task<List<ReleasePointerInfo>> GetReleasePointers(string environmentId)
        {
            var allPointers = new List<ReleasePointerInfo>();
            try
            {
                var page = 1;
                List<ReleasePointerInfo> pageResults;
                do
                {
                    var request = new GetReleasePointersRequest(m_ProjectId, environmentId, k_PageSize, page);
                    var response = await m_ReleasePointersClient.GetReleasePointersAsync(request);
                    pageResults = response.Result
                        .Select(rp => new ReleasePointerInfo(rp.Name, new Guid(rp.Id), false, rp.Release?.Name))
                        .ToList();
                    allPointers.AddRange(pageResults);
                    page++;
                }
                while (pageResults.Count == k_PageSize);
            }
            catch (HttpException e)
            {
                CoreLogger.LogWarning($"[Releases] Failed to fetch release pointers ({e.Response?.StatusCode}) for {RequestPath(environmentId, "releasepointers")}: {e.Message}");
                return null;
            }
            return allPointers;
        }

        public async Task<ReleasePointerInfo?> GetReleasePointer(string environmentId, string releasePointerName)
        {
            var request = new GetReleasePointerByNameRequest(m_ProjectId, environmentId, releasePointerName);
            var response = await m_ReleasePointersClient.GetReleasePointerByNameAsync(request);
            if (response.Status != 200)
            {
                CoreLogger.LogWarning($"[Releases] Failed to get release pointer '{releasePointerName}' for {environmentId}");
                return null;
            }
            return new ReleasePointerInfo(response.Result.Name, new Guid(response.Result.Id), false, response.Result.Release?.Name);
        }

        public async Task<List<ReleaseInfo>> GetReleases(string environmentId)
        {
            var allReleases = new List<ReleaseInfo>();
            try
            {
                var page = 1;
                List<ReleaseInfo> pageResults;
                do
                {
                    var request = new GetReleasesRequest(m_ProjectId, environmentId, k_PageSize, page);
                    var response = await m_ReleasesClient.GetReleasesAsync(request);
                    pageResults = response.Result
                        .Select(r => new ReleaseInfo(r.Name, r.Id, r.Description))
                        .ToList();
                    allReleases.AddRange(pageResults);
                    page++;
                }
                while (pageResults.Count == k_PageSize);
            }
            catch (HttpException e)
            {
                CoreLogger.LogWarning($"[Releases] Failed to fetch releases ({e.Response?.StatusCode}) for {RequestPath(environmentId, "releases")}: {e.Message}");
                return new List<ReleaseInfo>();
            }
            return allReleases;
        }

        string RequestPath(string environmentId, string resource)
        {
            return $"/projects/{m_ProjectId}/environments/{environmentId}/{resource}";
        }
    }
}
