using System;
using UnityEditor;
using UnityEngine.UIElements;

namespace Unity.Services.Core.Editor.ProjectBindRedirect
{
    class ProjectBindRedirectPopupUI
    {
        const string k_ProjectSettingsPath = "Project/Services";

        Action m_OnCloseButtonFired;

        public ProjectBindRedirectPopupUI(VisualElement parentElement, Action closeAction)
        {
            SetupUxmlAndUss(parentElement);
            SetupButtons(parentElement);

            EditorGameServiceSettingsProvider.TranslateStringsInTree(parentElement);

            m_OnCloseButtonFired = closeAction;
        }

        static void SetupUxmlAndUss(VisualElement containerElement)
        {
            var visualAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UxmlPath.Base);
            if (visualAsset != null)
            {
                visualAsset.CloneTree(containerElement);
            }

            VisualElementHelper.AddStyleSheetFromPath(containerElement, UssPath.Base);
        }

        void SetupButtons(VisualElement containerElement)
        {
            var cancelButton = containerElement.Q<Button>(className: UxmlClassNames.CancelButton);
            if (cancelButton != null)
            {
                cancelButton.clickable.clicked += CloseButtonAction;
            }

            var confirmButton = containerElement.Q<Button>(className: UxmlClassNames.ConfirmationButton);
            if (confirmButton != null)
            {
                confirmButton.clickable.clicked += ConfirmButtonAction;
            }
        }

        void CloseButtonAction()
        {
            m_OnCloseButtonFired?.Invoke();
        }

        void ConfirmButtonAction()
        {
            SettingsService.OpenProjectSettings(k_ProjectSettingsPath);
            CloseButtonAction();
        }

        static class UxmlPath
        {
            public const string Base = "Packages/com.unity.services.core/Editor/Core/ProjectBindRedirect/UXML/General.uxml";
        }

        static class UxmlClassNames
        {
            public const string CancelButton = "cancel-button";
            public const string ConfirmationButton = "confirmation-button";
        }

        static class UssPath
        {
            public const string Base = "Packages/com.unity.services.core/Editor/Core/ProjectBindRedirect/USS/ServiceActivationWindow.uss";
        }
    }
}
