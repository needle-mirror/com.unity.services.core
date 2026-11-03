using System;

namespace Unity.Services.Core.Editor.Releases
{
    interface IReleaseDashboardUrlResolver
    {
        string ManageReleasePointers(Guid environmentId);
        string ManageReleases(Guid environmentId);
    }
}
