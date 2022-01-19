using System;
using System.Threading.Tasks;
using Unity.Services.Core.Telemetry.Internal;

namespace Unity.Services.Core.Internal
{
    class CoreDiagnostics
    {
        internal const string CorePackageName = "com.unity.services.core";

        internal const string CircularDependencyDiagnosticName = "circular_dependency";

        internal const string CorePackageInitDiagnosticName = "core_package_init";

        internal const string OperateServicesInitDiagnosticName = "operate_services_init";

        public static CoreDiagnostics Instance { get; internal set; }

        internal IDiagnosticsComponentProvider DiagnosticsComponentProvider { get; set; }

        internal IDiagnostics Diagnostics { get; set; }

        async Task<IDiagnostics> GetOrCreateDiagnostics()
        {
            if (Diagnostics is null)
            {
                var diagnosticFactory = await DiagnosticsComponentProvider.CreateDiagnosticsComponents();
                Diagnostics = diagnosticFactory.Create(CorePackageName);
            }
            return Diagnostics;
        }

        public void SendCircularDependencyDiagnostics(Exception e)
        {
            var sendTask = SendCoreDiagnostics(CircularDependencyDiagnosticName, e);
            sendTask.ContinueWith(OnSendFailed, TaskContinuationOptions.OnlyOnFaulted);
        }

        public void SendCorePackageInitDiagnostics(Exception e)
        {
            var sendTask = SendCoreDiagnostics(CorePackageInitDiagnosticName, e);
            sendTask.ContinueWith(OnSendFailed, TaskContinuationOptions.OnlyOnFaulted);
        }

        public void SendOperateServicesInitDiagnostics(Exception e)
        {
            var sendTask = SendCoreDiagnostics(OperateServicesInitDiagnosticName, e);
            sendTask.ContinueWith(OnSendFailed, TaskContinuationOptions.OnlyOnFaulted);
        }

        private static void OnSendFailed(Task failedSendTask)
        {
            CoreLogger.LogException(failedSendTask.Exception);
        }

        async Task SendCoreDiagnostics(string diagnosticName, Exception e)
        {
            var diagnostics = await GetOrCreateDiagnostics();
            diagnostics.SendDiagnostic(diagnosticName, BuildExceptionMessage(e));
        }

        static string BuildExceptionMessage(Exception e)
        {
            var message = e.Message;
            while (e.InnerException != null)
            {
                message += $"\n{e.InnerException.Message}";
                e = e.InnerException;
            }

            return message;
        }
    }
}
