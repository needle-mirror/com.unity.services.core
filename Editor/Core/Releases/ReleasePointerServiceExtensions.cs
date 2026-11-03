using System.Linq;

namespace Unity.Services.Core.Editor.Releases
{
    static class ReleasePointerServiceExtensions
    {
        public static ReleasePointerInfo? ReleasePointerInfoFromName(this IReleasesService service, string releasePointerName)
        {
            if (string.IsNullOrEmpty(releasePointerName)
                || service.ReleasePointers == null)
            {
                return null;
            }

            return service.ReleasePointers
                .Where(info => info.Name == releasePointerName)
                .Cast<ReleasePointerInfo?>() // ensure FirstOrDefault will return null instead of default(ReleasePointerInfo)
                .FirstOrDefault();
        }
    }
}
