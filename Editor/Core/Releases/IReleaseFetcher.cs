using System.Collections.Generic;
using System.Threading.Tasks;

namespace Unity.Services.Core.Editor.Releases
{
    interface IReleaseFetcher
    {
        Task<List<ReleasePointerInfo>> FetchReleasePointers(string environmentId);
        Task<List<ReleaseInfo>> FetchReleases(string environmentId);
    }
}
