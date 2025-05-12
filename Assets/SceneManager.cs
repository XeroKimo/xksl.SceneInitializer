using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;


namespace xksl
{
    public struct NoPayload { };

    public static class SceneManager
    {
        static Dictionary<int, Queue<Action<Scene>>> initializerMap = new Dictionary<int, Queue<Action<Scene>>>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void SubsystemLoad()
        {
            UnitySceneManager.sceneLoaded += (scene, loadType) =>
            {
                if (initializerMap.TryGetValue(scene.buildIndex, out Queue<Action<Scene>> actions))
                {
                    if (actions.Count == 0)
                    {
                        LogWarning("No payload was found upon loading scene: " + scene.name);
                        return;
                    }

                    actions.Dequeue().Invoke(scene);
                }
            };
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void InitializeFirstScene()
        {
            ISceneInitializer initializer = UnitySceneManager.GetActiveScene().FindInitializer();
            if (initializer == null)
            {
                LogWarning("No initializer found for start up scene: " + UnitySceneManager.GetActiveScene().name);
                return;
            }

            Log("Initializing start up scene: " + UnitySceneManager.GetActiveScene().name);
            if (initializer.DefaultInitializer != null)
            {
                initializer.DefaultInitializer.Initialize(initializer);
            }
            else
            {
                Log("Invoking default initializer on ObjectID: " + initializer.gameObject.GetInstanceID() + " as Void Initialize()");
                initializer.Initialize();
            }
        }

        //Equivalent to calling LoadScene(LoadSceneMode.Single)
        public static void LoadScene<T>(int buildIndex, T payload)
        {
            UnitySceneManager.LoadScene(buildIndex, LoadSceneMode.Single);
            AttachPayload(buildIndex, payload);
        }

        //Equivalent to calling LoadScene(LoadSceneMode.Single)
        public static void LoadScene<T>(string path, T payload)
        {
            UnitySceneManager.LoadScene(path, LoadSceneMode.Single);
            AttachPayload(path, payload);
        }

        //Equivalent to calling LoadScene(LoadSceneMode.Single)
        public static void LoadScene<T, U>(SceneReference<U> scene, T payload) where U : ISceneInitializer, ISceneInitializeWith<T>
        {
            UnitySceneManager.LoadScene(scene.Path, LoadSceneMode.Single);
            AttachPayload(scene, payload);
        }

        //Equivalent to calling LoadScene(LoadSceneMode.Single)
        public static void LoadScene(int buildIndex)
        {
            UnitySceneManager.LoadScene(buildIndex, LoadSceneMode.Single);
            AttachEmptyPayload(buildIndex);
        }

        //Equivalent to calling LoadScene(LoadSceneMode.Single)
        public static void LoadScene(string path)
        {
            UnitySceneManager.LoadScene(path, LoadSceneMode.Single);
            AttachEmptyPayload(path);
        }

        //Equivalent to calling LoadScene(LoadSceneMode.Single)
        public static void LoadScene(SceneReference scene)
        {
            UnitySceneManager.LoadScene(scene.Path, LoadSceneMode.Single);
            AttachEmptyPayload(scene);
        }

        [Obsolete("Use LoadScene without parameters instead")]
        //Equivalent to calling LoadScene(LoadSceneMode.Single)
        public static void LoadScene(int buildIndex, NoPayload payload)
        {
            UnitySceneManager.LoadScene(buildIndex, LoadSceneMode.Single);
            AttachPayload(buildIndex, payload);
        }

        [Obsolete("Use LoadScene without parameters instead")]
        //Equivalent to calling LoadScene(LoadSceneMode.Single)
        public static void LoadScene(string path, NoPayload payload)
        {
            UnitySceneManager.LoadScene(path, LoadSceneMode.Single);
            AttachPayload(path, payload);
        }

        [Obsolete("Use LoadScene without parameters instead")]
        //Equivalent to calling LoadScene(LoadSceneMode.Single)
        public static void LoadScene(SceneReference scene, NoPayload payload)
        {
            UnitySceneManager.LoadScene(scene.Path, LoadSceneMode.Single);
            AttachPayload(scene, payload);
        }

        //Equivalent to calling LoadScene(LoadSceneMode.Additive)
        public static void LoadSubscene<T>(int buildIndex, T payload)
        {
            UnitySceneManager.LoadScene(buildIndex, LoadSceneMode.Additive);
            AttachPayload(buildIndex, payload);
        }

        //Equivalent to calling LoadScene(LoadSceneMode.Additive)
        public static void LoadSubscene<T>(string path, T payload)
        {
            UnitySceneManager.LoadScene(path, LoadSceneMode.Additive);
            AttachPayload(path, payload);
        }

        //Equivalent to calling LoadScene(LoadSceneMode.Additive)
        public static void LoadSubscene<T, U>(SceneReference<U> scene, T payload) where U : ISceneInitializer, ISceneInitializeWith<T>
        {
            UnitySceneManager.LoadScene(scene.Path, LoadSceneMode.Additive);
            AttachPayload(scene, payload);
        }

        //Equivalent to calling LoadScene(LoadSceneMode.Additive)
        public static void LoadSubscene(int buildIndex)
        {
            UnitySceneManager.LoadScene(buildIndex, LoadSceneMode.Additive);
            AttachEmptyPayload(buildIndex);
        }

        //Equivalent to calling LoadScene(LoadSceneMode.Additive)
        public static void LoadSubscene(string path)
        {
            UnitySceneManager.LoadScene(path, LoadSceneMode.Additive);
            AttachEmptyPayload(path);
        }

        //Equivalent to calling LoadScene(LoadSceneMode.Additive)
        public static void LoadSubscene(SceneReference scene)
        {
            UnitySceneManager.LoadScene(scene.Path, LoadSceneMode.Additive);
            AttachEmptyPayload(scene);
        }

        [Obsolete("Use LoadSubscene without parameters instead")]
        //Equivalent to calling LoadScene(LoadSceneMode.Additive)
        public static void LoadSubscene(int buildIndex, NoPayload payload)
        {
            UnitySceneManager.LoadScene(buildIndex, LoadSceneMode.Additive);
            AttachPayload(buildIndex, payload);
        }

        [Obsolete("Use LoadSubscene without parameters instead")]
        //Equivalent to calling LoadScene(LoadSceneMode.Additive)
        public static void LoadSubscene(string path, NoPayload payload)
        {
            UnitySceneManager.LoadScene(path, LoadSceneMode.Additive);
            AttachPayload(path, payload);
        }

        [Obsolete("Use LoadSubscene without parameters instead")]
        //Equivalent to calling LoadScene(LoadSceneMode.Additive)
        public static void LoadSubscene(SceneReference scene, NoPayload payload)
        {
            UnitySceneManager.LoadScene(scene.Path, LoadSceneMode.Additive);
            AttachPayload(scene, payload);
        }

        //Equivalent to calling LoadScene(LoadSceneMode.Single)
        public static AsyncOperation LoadSceneAsync<T>(int buildIndex, T payload)
        {
            var op = UnitySceneManager.LoadSceneAsync(buildIndex, LoadSceneMode.Single);
            AttachPayload(buildIndex, payload);
            return op;
        }

        //Equivalent to calling LoadScene(LoadSceneMode.Single)
        public static AsyncOperation LoadSceneAsync<T, U>(string path, T payload)
        {
            var op = UnitySceneManager.LoadSceneAsync(path, LoadSceneMode.Single);
            AttachPayload(path, payload);
            return op;
        }

        //Equivalent to calling LoadScene(LoadSceneMode.Single)
        public static AsyncOperation LoadSceneAsync<T, U>(SceneReference<U> scene, T payload) where U : ISceneInitializer, ISceneInitializeWith<T>
        {
            var op = UnitySceneManager.LoadSceneAsync(scene.Path, LoadSceneMode.Single);
            AttachPayload(scene, payload);
            return op;
        }

        //Equivalent to calling LoadScene(LoadSceneMode.Single)
        public static AsyncOperation LoadSceneAsync(int buildIndex)
        {
            var op = UnitySceneManager.LoadSceneAsync(buildIndex, LoadSceneMode.Single);
            AttachEmptyPayload(buildIndex);
            return op;
        }

        //Equivalent to calling LoadScene(LoadSceneMode.Single)
        public static AsyncOperation LoadSceneAsync(string path)
        {
            var op = UnitySceneManager.LoadSceneAsync(path, LoadSceneMode.Single);
            AttachEmptyPayload(path);
            return op;
        }

        //Equivalent to calling LoadScene(LoadSceneMode.Single)
        public static AsyncOperation LoadSceneAsync(SceneReference scene)
        {
            var op = UnitySceneManager.LoadSceneAsync(scene.Path, LoadSceneMode.Single);
            AttachEmptyPayload(scene);
            return op;
        }

        [Obsolete("Use LoadSceneAsync without parameters instead")]
        //Equivalent to calling LoadScene(LoadSceneMode.Single)
        public static AsyncOperation LoadSceneAsync(int buildIndex, NoPayload payload)
        {
            var op = UnitySceneManager.LoadSceneAsync(buildIndex, LoadSceneMode.Single);
            AttachPayload(buildIndex, payload);
            return op;
        }

        [Obsolete("Use LoadSceneAsync without parameters instead")]
        //Equivalent to calling LoadScene(LoadSceneMode.Single)
        public static AsyncOperation LoadSceneAsync(string path, NoPayload payload)
        {
            var op = UnitySceneManager.LoadSceneAsync(path, LoadSceneMode.Single);
            AttachPayload(path, payload);
            return op;
        }

        [Obsolete("Use LoadSceneAsync without parameters instead")]
        //Equivalent to calling LoadScene(LoadSceneMode.Single)
        public static AsyncOperation LoadSceneAsync(SceneReference scene, NoPayload payload)
        {
            var op = UnitySceneManager.LoadSceneAsync(scene.Path, LoadSceneMode.Single);
            AttachPayload(scene, payload);
            return op;
        }

        //Equivalent to calling LoadScene(LoadSceneMode.Additive)
        public static AsyncOperation LoadSubsceneAsync<T>(int buildIndex, T payload)
        {
            var op = UnitySceneManager.LoadSceneAsync(buildIndex, LoadSceneMode.Additive);
            AttachPayload(buildIndex, payload);
            return op;
        }

        //Equivalent to calling LoadScene(LoadSceneMode.Additive)
        public static AsyncOperation LoadSubsceneAsync<T>(string path, T payload)
        {
            var op = UnitySceneManager.LoadSceneAsync(path, LoadSceneMode.Additive);
            AttachPayload(path, payload);
            return op;
        }

        //Equivalent to calling LoadScene(LoadSceneMode.Additive)
        public static AsyncOperation LoadSubsceneAsync<T, U>(SceneReference<U> scene, T payload) where U : ISceneInitializer, ISceneInitializeWith<T>
        {
            var op = UnitySceneManager.LoadSceneAsync(scene.Path, LoadSceneMode.Additive);
            AttachPayload(scene, payload);
            return op;
        }

        //Equivalent to calling LoadScene(LoadSceneMode.Additive)
        public static AsyncOperation LoadSubsceneAsync(int buildIndex)
        {
            var op = UnitySceneManager.LoadSceneAsync(buildIndex, LoadSceneMode.Additive);
            AttachEmptyPayload(buildIndex);
            return op;
        }

        //Equivalent to calling LoadScene(LoadSceneMode.Additive)
        public static AsyncOperation LoadSubsceneAsync(string path)
        {
            var op = UnitySceneManager.LoadSceneAsync(path, LoadSceneMode.Additive);
            AttachEmptyPayload(path);
            return op;
        }

        //Equivalent to calling LoadScene(LoadSceneMode.Additive)
        public static AsyncOperation LoadSubsceneAsync(SceneReference scene)
        {
            var op = UnitySceneManager.LoadSceneAsync(scene.Path, LoadSceneMode.Additive);
            AttachEmptyPayload(scene);
            return op;
        }

        [Obsolete("Use LoadSubsceneAsync without parameters instead")]
        //Equivalent to calling LoadScene(LoadSceneMode.Additive)
        public static AsyncOperation LoadSubsceneAsync(int buildIndex, NoPayload payload)
        {
            var op = UnitySceneManager.LoadSceneAsync(buildIndex, LoadSceneMode.Additive);
            AttachPayload(buildIndex, payload);
            return op;
        }

        [Obsolete("Use LoadSubsceneAsync without parameters instead")]
        //Equivalent to calling LoadScene(LoadSceneMode.Additive)
        public static AsyncOperation LoadSubsceneAsync(string path, NoPayload payload)
        {
            var op = UnitySceneManager.LoadSceneAsync(path, LoadSceneMode.Additive);
            AttachPayload(path, payload);
            return op;
        }

        [Obsolete("Use LoadSubsceneAsync without parameters instead")]
        //Equivalent to calling LoadScene(LoadSceneMode.Additive)
        public static AsyncOperation LoadSubsceneAsync(SceneReference scene, NoPayload payload)
        {
            var op = UnitySceneManager.LoadSceneAsync(scene.Path, LoadSceneMode.Additive);
            AttachPayload(scene, payload);
            return op;
        }

        //Attaches a payload to the scene, invoking the initializer with the same payload type
        //Only call this if you're using some external function to load the scenes
        //Only call this up to once per LoadScene call, multiple calls cannot be detected
        //due to not being able to strongly associate a payload with the loaded scene
        //Calling this function before or after the LoadScene call has 0 effect
        public static void AttachPayload<T>(int buildIndex, T payload)
        {
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(buildIndex));
            Log("Loading \"" + sceneName + "\" with payload \"" + typeof(T).Name + "\"");
            if (!initializerMap.ContainsKey(buildIndex))
            {
                initializerMap.Add(buildIndex, new Queue<Action<Scene>>());
            }

            initializerMap[buildIndex].Enqueue(scene =>
            {
                ISceneInitializer untypedInitializer = FindInitializer(scene);
                if (untypedInitializer == null)
                {
                    LogWarning("Could not find an initializer when trying to load \"" + sceneName + "\"\nInitialization will be skipped");
                }

                ISceneInitializeWith<T> initializer = untypedInitializer as ISceneInitializeWith<T>;
                if (initializer == null)
                {
                    LogError("Could not find \"" + typeof(ISceneInitializeWith<T>) + "\" to match the payload in the initializer \"" + untypedInitializer.GetType() + "\" when trying to load \"" + sceneName + "\"\nInitialization will be skipped");
                    return;
                }

                Log("Initializer found in scene \"" + sceneName + "\" with ObjectID: " + initializer.gameObject.GetInstanceID() +
                    "\nInvoking initializer \"" + initializer.GetType() + $"\" as Void Initialize({typeof(T).Name})");
                initializer.Initialize(payload);
            });
        }

        //Attaches a payload to the scene, invoking the initializer with the same payload type
        //Only call this if you're using some external function to load the scenes
        //Only call this up to once per LoadScene call, multiple calls cannot be detected
        //due to not being able to strongly associate a payload with the loaded scene
        //Calling this function before or after the LoadScene call has 0 effect
        public static void AttachPayload<T>(string path, T payload)
        {
            AttachPayload(SceneUtility.GetBuildIndexByScenePath(path), payload);
        }

        //Attaches a payload to the scene, invoking the initializer with the same payload type
        //Only call this if you're using some external function to load the scenes
        //Only call this up to once per LoadScene call, multiple calls cannot be detected
        //due to not being able to strongly associate a payload with the loaded scene
        //Calling this function before or after the LoadScene call has 0 effect
        public static void AttachPayload<U, T>(SceneReference<U> scene, T payload) where U : ISceneInitializer, ISceneInitializeWith<T>
        {
            AttachPayload(scene.Path, payload);
        }

        //Attaches no payload to the scene, invoking the empty arguments initializer
        //Only call this if you're using some external function to load the scenes
        //Only call this up to once per LoadScene call, multiple calls cannot be detected
        //due to not being able to strongly associate a payload with the loaded scene
        //Calling this function before or after the LoadScene call has 0 effect
        public static void AttachEmptyPayload(int buildIndex)
        {
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(buildIndex));
            Log("Loading \"" + sceneName + "\" with an empty payload");

            if (!initializerMap.ContainsKey(buildIndex))
            {
                initializerMap.Add(buildIndex, new Queue<Action<Scene>>());
            }

            initializerMap[buildIndex].Enqueue(scene =>
            {
                ISceneInitializer initializer = FindInitializer(scene);
                if (initializer == null)
                {
                    LogWarning("Could not find an initializer when trying to load \"" + sceneName + "\"\nInitialization will be skipped");
                    return;
                }

                Log("Initializer found in scene \"" + sceneName + "\" with ObjectID: " + initializer.gameObject.GetInstanceID() +
                    "\nInvoking initializer \"" + initializer.GetType() + "\" as Void Initialize()");
                initializer.Initialize();
            });
        }

        //Attaches no payload to the scene, invoking the empty arguments initializer
        //Only call this if you're using some external function to load the scenes
        //Only call this up to once per LoadScene call, multiple calls cannot be detected
        //due to not being able to strongly associate a payload with the loaded scene
        //Calling this function before or after the LoadScene call has 0 effect
        public static void AttachEmptyPayload(string path)
        {
            AttachEmptyPayload(SceneUtility.GetBuildIndexByScenePath(path));
        }

        //Attaches no payload to the scene, invoking the empty arguments initializer
        //Only call this if you're using some external function to load the scenes
        //Only call this up to once per LoadScene call, multiple calls cannot be detected
        //due to not being able to strongly associate a payload with the loaded scene
        //Calling this function before or after the LoadScene call has 0 effect
        public static void AttachEmptyPayload(SceneReference scene)
        {
            AttachEmptyPayload(scene.Path);
        }

        //Attaches no payload to the scene, invoking the empty arguments initializer
        //Only call this if you're using some external function to load the scenes
        //Only call this up to once per LoadScene call, multiple calls cannot be detected
        //due to not being able to strongly associate a payload with the loaded scene
        //Calling this function before or after the LoadScene call has 0 effect
        [Obsolete("Use AttachEmptyPayload without parameters instead")]
        public static void AttachPayload(int buildIndex, NoPayload payload)
        {
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(buildIndex));
            Log("Loading \"" + sceneName + "\" with payload \"" + typeof(NoPayload).Name + "\"");

            if (!initializerMap.ContainsKey(buildIndex))
            {
                initializerMap.Add(buildIndex, new Queue<Action<Scene>>());
            }

            initializerMap[buildIndex].Enqueue(scene =>
            {
                ISceneInitializer initializer = FindInitializer(scene);
                if (initializer == null)
                {
                    LogWarning("Could not find an initializer when trying to load \"" + sceneName + "\"\nInitialization will be skipped");
                    return;
                }

                Log("Initializer found in scene \"" + sceneName + "\" with ObjectID: " + initializer.gameObject.GetInstanceID() +
                    "\nInvoking initializer \"" + initializer.GetType() + "\" as Void Initialize()");
                initializer.Initialize();
            });
        }

        //Attaches no payload to the scene, invoking the empty arguments initializer
        //Only call this if you're using some external function to load the scenes
        //Only call this up to once per LoadScene call, multiple calls cannot be detected
        //due to not being able to strongly associate a payload with the loaded scene
        //Calling this function before or after the LoadScene call has 0 effect
        [Obsolete("Use AttachEmptyPayload without parameters instead")]
        public static void AttachPayload(string path, NoPayload payload)
        {
            AttachPayload(SceneUtility.GetBuildIndexByScenePath(path), payload);
        }

        //Attaches no payload to the scene, invoking the empty arguments initializer
        //Only call this if you're using some external function to load the scenes
        //Only call this up to once per LoadScene call, multiple calls cannot be detected
        //due to not being able to strongly associate a payload with the loaded scene
        //Calling this function before or after the LoadScene call has 0 effect
        [Obsolete("Use AttachEmptyPayload without parameters instead")]
        public static void AttachPayload(SceneReference scene, NoPayload payload)
        {
            AttachPayload(scene.Path, payload);
        }

        public static ISceneInitializer FindInitializer(this Scene scene)
        {
            return scene.GetRootGameObjects()
                .SelectMany(go => go.GetComponents<MonoBehaviour>())
                .Select(comp => comp as ISceneInitializer)
                .FirstOrDefault(init => init != null);
        }

        public static ISceneInitializeWith<T> FindInitializer<T>(this Scene scene)
        {
            return FindInitializer(scene) as ISceneInitializeWith<T>;
        }

        internal static void Log(string message)
        {
            Debug.Log("xksl.SceneManager: " + message);
        }

        internal static void LogWarning(string message)
        {
            Debug.LogWarning("xksl.SceneManager: " + message);
        }

        internal static void LogError(string message)
        {
            Debug.LogError("xksl.SceneManager: " + message);
        }
    }
}