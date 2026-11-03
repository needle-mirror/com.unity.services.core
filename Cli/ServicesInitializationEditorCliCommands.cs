#if UNITY_PIPELINE_AVAILABLE && UNITY_EDITOR
using System;
using System.Collections.Generic;
using Unity.Pipeline.Commands;
using Unity.Services.Core.Components;
using UnityEditor;
using UnityEditor.Search;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Unity.Services.Core.Cli
{
    static class ServicesInitializationEditorCliCommands
    {
        [CliGroupCommand(CoreCliConstants.Group,
            "Add the Services Initialization component to a GameObject in the open scene, so Unity Services initialize in Play Mode without any game code. Creates the GameObject when it does not exist. Re-running targets the component already in the scene and rewrites it from the arguments given. Refused in Play Mode. The scene is left unsaved: save it with " + CoreCliConstants.SceneGroup + " save")]
        public static ServicesInitializationComponentDto AddServicesInitializationComponent(
            [CliArg("game-object", "Name or hierarchy path (Parent/Child) of the GameObject to host the component. Created in the active scene when nothing matches a plain name. Defaults to the GameObject already holding the component, else 'Unity Services'.")] string gameObject = null,
            [CliArg("initialize-on-start", "Initialize Unity Services in Start(). Pass false to initialize from your own code instead.")] bool initializeOnStart = true,
            [CliArg("environment", "Environment name to initialize with. Omit to leave the component's environment untouched, which falls back to the environment configured in project settings. Pass an empty string to clear a custom environment.")] string environment = null)
        {
            RefusePlayMode();

            var existing = FindSingleInOpenScenes();
            var target = NormalizeTarget(gameObject);
            RefuseComponentOnAnotherGameObject(existing, target);

            var undoGroup = BeginUndoGroup(CoreCliConstants.ServicesInitializationUndoName);
            try
            {
                var component = existing ?? AddToScene(target ?? CoreCliConstants.DefaultGameObjectName);
                Configure(component, initializeOnStart, environment);
                EditorSceneManager.MarkSceneDirty(component.gameObject.scene);
                Undo.CollapseUndoOperations(undoGroup);
                return Describe(component, created: existing == null);
            }
            catch
            {
                Undo.RevertAllDownToGroup(undoGroup);
                throw;
            }
        }

        static void RefusePlayMode()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                throw new InvalidOperationException(
                    "The Editor is in Play Mode, and scene changes made in Play Mode are lost when it exits. "
                    + "Exit Play Mode with " + CoreCliConstants.PlayModeGroup + " stop, then run this command again.");
            }
        }

        static string NormalizeTarget(string gameObject)
        {
            var target = gameObject?.Replace('\\', '/').Trim().Trim('/');
            return string.IsNullOrEmpty(target) ? null : target;
        }

        static ServicesInitialization FindSingleInOpenScenes()
        {
            var components = FindAllInOpenScenes<ServicesInitialization>();
            if (components.Length > 1)
            {
                var hosts = new GameObject[components.Length];
                for (var i = 0; i < components.Length; i++)
                {
                    hosts[i] = components[i].gameObject;
                }

                throw new InvalidOperationException(
                    $"The open scenes hold {components.Length} Services Initialization components ({Describe(hosts)}), "
                    + "and initializing twice is a defect. Remove all but one with " + CoreCliConstants.ComponentGroup + " remove, then run this command again.");
            }

            return components.Length == 1 ? components[0] : null;
        }

        static T[] FindAllInOpenScenes<T>() where T : Object
        {
#if UNITY_6000_6_OR_NEWER
            return Object.FindObjectsByType<T>(FindObjectsInactive.Include);
#else
            return Object.FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None);
#endif
        }

        static void RefuseComponentOnAnotherGameObject(ServicesInitialization existing, string target)
        {
            if (existing == null || target == null || Matches(existing.gameObject, target))
            {
                return;
            }

            throw new InvalidOperationException(
                $"A Services Initialization component is already on '{HierarchyPath(existing.gameObject)}', and initializing twice is a defect. "
                + $"Either omit --game-object or pass --game-object \"{HierarchyPath(existing.gameObject)}\" to update it, "
                + "or remove it with " + CoreCliConstants.ComponentGroup + " remove first.");
        }

        static ServicesInitialization AddToScene(string target)
        {
            var host = FindOrCreateGameObject(target);
            var component = Undo.AddComponent<ServicesInitialization>(host);
            if (component == null)
            {
                throw new InvalidOperationException(
                    $"Unity did not add the Services Initialization component to '{HierarchyPath(host)}'. The Console holds the reason.");
            }

            return component;
        }

        static GameObject FindOrCreateGameObject(string target)
        {
            var matches = new List<GameObject>();
            foreach (var transform in FindAllInOpenScenes<Transform>())
            {
                if (Matches(transform.gameObject, target))
                {
                    matches.Add(transform.gameObject);
                }
            }

            if (matches.Count > 1)
            {
                throw new ArgumentException(
                    $"--game-object \"{target}\" matches {matches.Count} GameObjects ({Describe(matches)}). "
                    + "Pass the hierarchy path of one of them; " + CoreCliConstants.SceneGroup + " get_hierarchy lists the paths.");
            }

            if (matches.Count == 1)
            {
                return matches[0];
            }

            if (target.Contains("/"))
            {
                throw new ArgumentException(
                    $"No GameObject in the open scenes is at '{target}'. Check the path with " + CoreCliConstants.SceneGroup + " get_hierarchy, "
                    + "or pass a plain name to create a root GameObject.");
            }

            var host = new GameObject(target);
            Undo.RegisterCreatedObjectUndo(host, CoreCliConstants.ServicesInitializationUndoName);
            return host;
        }

        static void Configure(ServicesInitialization component, bool initializeOnStart, string environment)
        {
            var serialized = new SerializedObject(component);
            RequiredProperty(serialized, nameof(ServicesInitialization.InitializeOnStart)).boolValue = initializeOnStart;

            if (environment != null)
            {
                var custom = environment.Trim();
                RequiredProperty(serialized, nameof(ServicesInitialization.UseCustomEnvironment)).boolValue = custom.Length > 0;
                if (custom.Length > 0)
                {
                    RequiredProperty(serialized, nameof(ServicesInitialization.EnvironmentName)).stringValue = custom;
                }
            }

            serialized.ApplyModifiedProperties();
        }

        static SerializedProperty RequiredProperty(SerializedObject serialized, string name)
        {
            var property = serialized.FindProperty(name);
            if (property == null)
            {
                throw new InvalidOperationException(
                    $"The Services Initialization component has no serialized field '{name}', so this command does not match "
                    + "the installed com.unity.services.core. Nothing was changed. Update com.unity.services.core, "
                    + "or set the field in the Inspector instead.");
            }

            return property;
        }

        static bool Matches(GameObject candidate, string target)
        {
            return string.Equals(candidate.name, target, StringComparison.Ordinal)
                || string.Equals(HierarchyPath(candidate), target, StringComparison.Ordinal);
        }

        static int BeginUndoGroup(string name)
        {
            Undo.IncrementCurrentGroup();
            var group = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName(name);
            return group;
        }

        static string HierarchyPath(GameObject gameObject)
        {
            return SearchUtils.GetHierarchyPath(gameObject, false).TrimStart('/');
        }

        static string Describe(IReadOnlyList<GameObject> gameObjects)
        {
            var descriptions = new string[gameObjects.Count];
            for (var i = 0; i < gameObjects.Count; i++)
            {
                descriptions[i] = $"'{gameObjects[i].scene.name}:{HierarchyPath(gameObjects[i])}'";
            }

            return string.Join(", ", descriptions);
        }

        static string InitializationProblem(ServicesInitialization component)
        {
            if (!component.isActiveAndEnabled)
            {
                return $"The component on '{HierarchyPath(component.gameObject)}' is disabled or on an inactive GameObject, so Start() never runs "
                    + "and the services do not initialize. Enable the component and activate the GameObject.";
            }

            if (!component.InitializeOnStart)
            {
                return "initialize-on-start is off, so the services initialize only when your own code calls UnityServices.InitializeAsync.";
            }

            return null;
        }

        static ServicesInitializationComponentDto Describe(ServicesInitialization component, bool created)
        {
            return new ServicesInitializationComponentDto
            {
                GameObject = HierarchyPath(component.gameObject),
                Scene = component.gameObject.scene.path,
                Created = created,
                InitializeOnStart = component.InitializeOnStart,
                Environment = component.UseCustomEnvironment ? component.EnvironmentName : null,
                Message = InitializationProblem(component)
            };
        }

        public class ServicesInitializationComponentDto
        {
            public string GameObject { get; set; }
            public string Scene { get; set; }
            public bool Created { get; set; }
            public bool InitializeOnStart { get; set; }
            public string Environment { get; set; }
            public string Message { get; set; }
        }
    }
}
#endif
