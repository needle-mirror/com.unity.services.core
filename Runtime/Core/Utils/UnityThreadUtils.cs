using System.Threading;
using System.Threading.Tasks;
#if UNITY_6000_7_OR_NEWER
using Unity.Scripting.LifecycleManagement;
#endif
using UnityEngine;

namespace Unity.Services.Core
{
#if UNITY_6000_7_OR_NEWER
    [NoAutoStaticsCleanup]
#endif
    static class UnityThreadUtils
    {
        static int s_UnityThreadId;

        internal static TaskScheduler UnityThreadScheduler { get; private set; }

#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
#endif
        internal static void CaptureUnityThreadInfo()
        {
            s_UnityThreadId = Thread.CurrentThread.ManagedThreadId;
            UnityThreadScheduler = TaskScheduler.FromCurrentSynchronizationContext();
        }

        public static bool IsRunningOnUnityThread => Thread.CurrentThread.ManagedThreadId == s_UnityThreadId;
    }
}
