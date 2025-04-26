using System;
using System.Collections;
using System.Collections.Generic;
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
        static Dictionary<int, List<Action<Scene>>> initializerMap = new Dictionary<int, List<Action<Scene>>>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void SubsystemLoad()
        {
            UnitySceneManager.sceneLoaded += (scene, loadType) =>
            {
                if (initializerMap.TryGetValue(scene.buildIndex, out List<Action<Scene>> actions))
                {
                    if(actions.Count == 0)
                        return;

                    actions[0].Invoke(scene);
                    actions.RemoveAt(0);
                }
            };
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void InitializeFirstScene()
        {
            SceneInitializer initializer = GameObject.FindObjectOfType<SceneInitializer>();
            if (!initializer)
            {
                LogWarning("No initializer found for start up scene: " + UnitySceneManager.GetActiveScene().name);
                return;
            }

            Log("Initializing start up scene: " + UnitySceneManager.GetActiveScene().name);
            if (initializer.defaultInitializer != null)
            {
                initializer.defaultInitializer.Initialize(initializer);
            }
            else
            {
                Log("Invoking default initializer on ObjectID: " + initializer.GetInstanceID() + " as Void Initialize()");
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
        public static void LoadScene(int buildIndex, NoPayload payload)
        {
            UnitySceneManager.LoadScene(buildIndex, LoadSceneMode.Single);
            AttachPayload(buildIndex, payload);
        }

        //Equivalent to calling LoadScene(LoadSceneMode.Additive)
        public static void LoadSubscene<T>(int buildIndex, T payload)
        {
            UnitySceneManager.LoadScene(buildIndex, LoadSceneMode.Additive);
            AttachPayload(buildIndex, payload);
        }

        //Equivalent to calling LoadScene(LoadSceneMode.Additive)
        public static void LoadSubscene(int buildIndex, NoPayload payload)
        {
            UnitySceneManager.LoadScene(buildIndex, LoadSceneMode.Additive);
            AttachPayload(buildIndex, payload);
        }

        //Equivalent to calling LoadScene(LoadSceneMode.Single)
        public static AsyncOperation LoadSceneAsync<T>(int buildIndex, T payload)
        {
            var op = UnitySceneManager.LoadSceneAsync(buildIndex, LoadSceneMode.Single);
            AttachPayload(buildIndex, payload);
            return op;
        }

        //Equivalent to calling LoadScene(LoadSceneMode.Single)
        public static AsyncOperation LoadSceneAsync(int buildIndex, NoPayload payload)
        {
            var op = UnitySceneManager.LoadSceneAsync(buildIndex, LoadSceneMode.Single);
            AttachPayload(buildIndex, payload);
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
        public static AsyncOperation LoadSubsceneAsync(int buildIndex, NoPayload payload)
        {
            var op = UnitySceneManager.LoadSceneAsync(buildIndex, LoadSceneMode.Additive);
            AttachPayload(buildIndex, payload);
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
                initializerMap.Add(buildIndex, new List<Action<Scene>>());
            }

            initializerMap[buildIndex].Add(scene =>
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

                Log("Initializer found in scene \"" + sceneName + "\" with ObjectID: " + (initializer as SceneInitializer).gameObject.GetInstanceID() +
                    "\nInvoking initializer \"" + initializer.GetType() + "\" as " + typeof(ISceneInitializeWith<T>).GetMethod(nameof(initializer.Initialize)));
                initializer.Initialize(payload);
            });
        }

        //Attaches no payload to the scene, invoking the empty arguments initializer
        //Only call this if you're using some external function to load the scenes
        //Only call this up to once per LoadScene call, multiple calls cannot be detected
        //due to not being able to strongly associate a payload with the loaded scene
        //Calling this function before or after the LoadScene call has 0 effect
        public static void AttachPayload(int buildIndex, NoPayload payload)
        {
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(buildIndex));
            Log("Loading \"" + sceneName + "\" with payload \"" + typeof(NoPayload).Name + "\"");

            if (!initializerMap.ContainsKey(buildIndex))
            {
                initializerMap.Add(buildIndex, new List<Action<Scene>>());
            }

            initializerMap[buildIndex].Add(scene =>
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