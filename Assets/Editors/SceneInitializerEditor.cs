using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace xksl
{
    public class SceneInitializerEditor : EditorWindow
    {
        private List<Type> defaultInitializerTypes = new List<Type>();
        private List<Type> sceneReferenceTypes = new List<Type>();

        private List<Type> compatibleDefaultInitializerTypes = new List<Type>();
        private string[] compatibleDefaultInitializerTypesNames;
        private int compatibleDefaultInitializerSelectedIndex;

        private List<Type> initializerTypes = new List<Type>();
        private string[] initializerTypesNames;
        private int initializerSelectedIndex;

        const string undoGroupInitializerCreationName = "Scene Initalizer Creation";
        const string undoGroupDefaultInitializerAssignmentName = "Default Initalizer Assignment";

        [MenuItem("Scenes/Initializer Window")]
        public static void ShowWindow()
        {
            GetWindow<SceneInitializerEditor>();
        }

        private void OnEnable()
        {
            Undo.undoRedoEvent += OnUndoRedo;
            BuildTypesList();
            InitializeMetaData();
        }

        private void OnDisable()
        {
            Undo.undoRedoEvent -= OnUndoRedo;
        }

        private void OnUndoRedo(in UndoRedoInfo info)
        {
            if(info.undoName == undoGroupInitializerCreationName || info.undoName == undoGroupDefaultInitializerAssignmentName)
                InitializeMetaData();
        }

        private void InitializeMetaData()
        {
            ISceneInitializer initializer = EditorSceneManager.GetActiveScene().FindInitializer();

            initializerSelectedIndex = 0;
            if (initializer != null)
            {
                initializerSelectedIndex = initializerTypes.IndexOf(initializer.GetType()) + 1;
                BuildCompatibleDefaultInitializerTypeList(initializer);

                if (initializer.DefaultInitializer != null)
                    compatibleDefaultInitializerSelectedIndex = compatibleDefaultInitializerTypes.IndexOf(initializer.DefaultInitializer.GetType()) + 1;
            }
        }

        private void OnGUI()
        {
            Scene activeScene = EditorSceneManager.GetActiveScene();
            ISceneInitializer initializer = activeScene.FindInitializer();


            int newIndex = EditorGUILayout.Popup("Initializer Type", initializerSelectedIndex, initializerTypesNames);
            if (newIndex != initializerSelectedIndex)
            {
                Undo.IncrementCurrentGroup();
                Undo.SetCurrentGroupName(undoGroupInitializerCreationName);
                initializerSelectedIndex = newIndex;

                if (newIndex > 0)
                {
                    GameObject obj = new GameObject("Scene Initializer");
                    obj.hideFlags = HideFlags.HideInHierarchy;
                    initializer = obj.AddComponent(initializerTypes[newIndex - 1]) as ISceneInitializer;
                    BuildCompatibleDefaultInitializerTypeList(initializer);
                    Undo.RegisterCreatedObjectUndo(obj, "Scene Initializer Creation");

                    CreateSceneReference(initializer, activeScene);
                }
                else
                {
                    DeleteSceneReference(activeScene);
                    Undo.DestroyObjectImmediate(initializer.gameObject);
                    Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
                    initializer = null;
                }

                Undo.CollapseUndoOperations(Undo.GetCurrentGroup());

                EditorSceneManager.MarkSceneDirty(activeScene);

            }
            if (initializer == null)
            {
                GUILayout.Label("Initializer not found");
            }
            else
            {
                if (GUILayout.Button("Recreate Reference Asset"))
                {
                    CreateSceneReference(initializer, activeScene);
                }

                bool hideInitializer = initializer.gameObject.hideFlags == HideFlags.HideInHierarchy;
                bool newHideInitializer = GUILayout.Toggle(hideInitializer, "Hide Initializer");

                if (newHideInitializer != hideInitializer)
                {
                    initializer.gameObject.hideFlags = newHideInitializer ? HideFlags.HideInHierarchy : HideFlags.None;
                    EditorSceneManager.MarkSceneDirty(activeScene);
                }

                newIndex = EditorGUILayout.Popup("Default Initializer Type", compatibleDefaultInitializerSelectedIndex, compatibleDefaultInitializerTypesNames);

                Undo.RecordObject(initializer as MonoBehaviour, undoGroupDefaultInitializerAssignmentName);
                if (newIndex != compatibleDefaultInitializerSelectedIndex)
                {
                    
                    if (newIndex == 0)
                        initializer.DefaultInitializer = null;
                    else
                        initializer.DefaultInitializer = Activator.CreateInstance(compatibleDefaultInitializerTypes[newIndex - 1]) as DefaultSceneInitializer;
                }
                compatibleDefaultInitializerSelectedIndex = newIndex;
                Editor.CreateEditor(initializer as MonoBehaviour).DrawDefaultInspector();
            }
        }

        private void BuildTypesList()
        {
            foreach (var type in AppDomain.CurrentDomain.GetAssemblies().Where(a => !a.IsDynamic)
                .SelectMany(a => a.GetExportedTypes())
                .Where(t => !t.IsGenericType && !t.IsAbstract &&
                    (t.GetInterface(nameof(ISceneInitializer)) != null || t.IsSubclassOf(typeof(DefaultSceneInitializer)) || t.IsSubclassOf(typeof(SceneReference)))))
            {
                if (type.GetInterface(nameof(ISceneInitializer)) != null)
                {
                    initializerTypes.Add(type);
                }
                else if (type.IsSubclassOf(typeof(DefaultSceneInitializer)))
                {
                    defaultInitializerTypes.Add(type);
                }
                else
                {
                    sceneReferenceTypes.Add(type);
                }
            }

            initializerSelectedIndex = 0;
            initializerTypesNames = new string[initializerTypes.Count + 1];
            initializerTypesNames[0] = "None";
            for (int i = 0; i < initializerTypes.Count; i++)
            {
                initializerTypesNames[i + 1] = initializerTypes[i].Name;
            }
        }

        private void BuildCompatibleDefaultInitializerTypeList(ISceneInitializer initializer)
        {
            compatibleDefaultInitializerTypes = defaultInitializerTypes.Where(t =>
                initializer.GetType().GetInterfaces().Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ISceneInitializeWith<>)).Any(i => t.IsSubclassOf(typeof(DefaultSceneInitializer<>).MakeGenericType(i.GenericTypeArguments[0])))).ToList();

            compatibleDefaultInitializerSelectedIndex = 0;
            compatibleDefaultInitializerTypesNames = new string[compatibleDefaultInitializerTypes.Count + 1];
            compatibleDefaultInitializerTypesNames[0] = "None";
            for (int i = 0; i < compatibleDefaultInitializerTypes.Count; i++)
            {
                compatibleDefaultInitializerTypesNames[i + 1] = compatibleDefaultInitializerTypes[i].BaseType.GenericTypeArguments[0].Name;
            }
        }

        private void CreateSceneReference(ISceneInitializer initializer, Scene scene)
        {
            Type type = sceneReferenceTypes.FirstOrDefault(t => t.IsSubclassOf(typeof(SceneReference<>).MakeGenericType(initializer.GetType())));
            if (type == null)
            {
                SceneManager.LogWarning("Failed to generate a SceneReference asset: No SceneReference type found which is a subclass of " + typeof(SceneReference<>).MakeGenericType(initializer.GetType()));
                return;
            }

            SceneReference reference = ScriptableObject.CreateInstance(type) as SceneReference;
            reference.path = scene.path;
            string path = Path.GetDirectoryName(scene.path) + "/" + Path.GetFileNameWithoutExtension(scene.path) + "_reference.asset";

            AssetDatabase.DeleteAsset(path);

            AssetDatabase.CreateAsset(reference, path);
            AssetDatabase.SaveAssetIfDirty(reference);
        }

        private void DeleteSceneReference(Scene scene)
        {
            string path = Path.GetDirectoryName(scene.path) + "/" + Path.GetFileNameWithoutExtension(scene.path) + "_reference.asset";
            AssetDatabase.DeleteAsset(path);
        }
    }
}
#endif