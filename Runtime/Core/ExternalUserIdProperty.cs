using System;
#if ENABLE_UNITY_CLOUD_IDENTIFIERS && UNITY_SUPPORTS_IDENTIFIERS_USER_ID
using UnityEngine.Identifiers;
#endif

namespace Unity.Services.Core
{
    class ExternalUserIdProperty : IDisposable
    {
#if ENABLE_UNITY_CLOUD_IDENTIFIERS && UNITY_SUPPORTS_IDENTIFIERS_USER_ID
        public event Action<string> UserIdChanged;

        public ExternalUserIdProperty()
        {
            Identifiers.userIdChanged += OnEngineUserIdChanged;
        }

        public void Dispose()
        {
            Identifiers.userIdChanged -= OnEngineUserIdChanged;
            UserIdChanged = null;
        }

        void OnEngineUserIdChanged(string newId)
        {
            UserIdChanged?.Invoke(newId);
        }

        public string UserId
        {
            get => Identifiers.userId;
            set => Identifiers.userId = value;
        }
#else
        public event Action<string> UserIdChanged;

        string m_UserId;

        public void Dispose()
        {
            UserIdChanged = null;
            m_UserId = null;
        }

        public string UserId
        {
            get => m_UserId;
            set
            {
                m_UserId = value;
                UserIdChanged?.Invoke(m_UserId);
            }
        }
#endif
    }
}
