#if UNITY_6000_7_OR_NEWER
using Unity.Scripting.LifecycleManagement;
#endif

namespace Unity.Services.Core.Editor.OrganizationHandler
{
    /// <summary>
    /// Helper class to get information of the current organization.
    /// </summary>
#if UNITY_6000_7_OR_NEWER
    [NoAutoStaticsCleanup]
#endif
    public static class OrganizationProvider
    {
        static IOrganizationHandler s_OrganizationHandler = new OrganizationHandler();

        /// <summary>
        /// Current organization info.
        /// </summary>
        public static IOrganizationHandler Organization
        {
            get => s_OrganizationHandler;
            set
            {
                if (value != null)
                {
                    s_OrganizationHandler = value;
                }
            }
        }
    }
}
