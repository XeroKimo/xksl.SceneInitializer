# xksl.SceneInitializer
Add custom entry points to your scene in Unity.

Pass in payloads when loading to be able to customize how your next scene loads.

# Requirements
Minimum Unity Version Tested: 2022.3.36f1

# Installation Process
Honestly, I don't know how to support Unity Packaging for the initial release, so just pull this repos and add it under your assets folder and it should work fine for now.

# Usage
1. Create your custom initializer
```c++
using xksl;

//Define a custom type to use as a payload for your next scene
[System.Serializable]
public struct MyPayload 
{
    //Add variables here
    public int level;
    public GameObject playerPrefab;
}

//Create a non-generic class which inherits from DefaultSceneInitializer<Payload Type> 
// to expose it to the editor.
public class MyPayloadDefault : DefaultSceneInitializer<MyPayload> {}

//Create a class which inherits from SceneInitializer
//Optionally, you can inherit from ISceneInitializeWith<Payload Type> to expose
//additional Initialize() methods with custom payloads
public class MySceneInitializer : SceneInitializer, ISceneInitializeWith<MyPayload>
{
    //SceneInitializer is a component, you can add variables here as well
    //to expose to the editor.
    [SerializeField]
    private List<Transform> spawnPoints;

    //Exposed with SceneInitializer
    public override void Initialize()
    {

    }

    //Exposed with ISceneInitializeWith<MyPayload>
    public void Initialize(MyPayload args)
    {
        Spawn(arg.playerPrefab, spawnPoints.Random());
    }
}

//All of the above types can be declared in a single file

//Optional: Create a non-generic class which inherits from SceneReference<Initializer>, 
//this enables creating a scriptable object when assigning a initializer to a scene, 
//allowing you to store strongly typed scene references so you can get compile errors 
//instead of runtime ones if you try to load the scene with an incompatible payload. 
//These references will always be stored alongside the scene path.
public class MySceneReference : SceneReference<MySceneInitializer>
{

}
```
2. Create an Initializer in the scene. It is recommended that you create one through the Initializer Editor under Scenes->Initializer Window to get access to extra features like changing the default initializer type or automatic scene reference creation

![Editor Image](/Screenshots/EditorExample.png)
3. Fill in the data needed for your initializer.
4. Call `xksl.SceneManager.LoadScene()` with the scene you want to intialize with and the payload. Ex: `xksl.SceneManager.LoadScene(gameScene, new MyPayload{ level = 100, playerPrefab = defaultPlayerPrefab });`

# FAQ
### When is the Initializer Invoked?
The initializer just listens to `SceneManager.sceneLoaded` in order be invoked, which gets called before `MonoBehaviour.Start()` and after `MonoBehaviour.OnEnabled()` gets called.

### How do you know what type of payloads can be passed when loading scenes?
If you declare a SceneReference, it'll be able to check its interfaces to see if it matches any of the payload types, otherwise I can only rely on runtime checks to see if there's an initializer that has a matching `ISceneInitializeWith<>`.

### What types are supported in the payload?
There's no restriction except for if you want the payload to be exposed in the editor. Only types that play nice with the editor will get exposed. The other exception would be live game objects. Payloads can hold reference to them, but by themselves don't guarantee their lifetime, so it's up to the user to ensure they stay alive across scene transitions.

### What's the point of the `DefaultSceneInitializer`?
The default scene initializer allows you to pass in a payload as the start up scene so you don't have to create a 2nd scene to load in from. This allows for easier testing if your scene is meant to be loaded.

### Why do I have to create a non-generic type inheriting from `DefaultSceneInitializer<>` or `SceneReference<>`?
Both of these have to do with restrictions in Unity. `DefaultSceneInitializer<>` is needed because the editor can't display generic types nor can you reference assets of generic type. `SceneReference<>`'s issue is that you can't instantiate `ScriptableObject` which contain generic parameters.

### Are AssetBundle scenes supported?
Currently no. Since loading by path can take on different forms such as relative path, with or without the extension, and scene name, I want to make sure every single one of them correctly associates with a given scene, which I have not tried doing yet.

# Advanced
### Attaching a payload without calling `xksl.SceneManager.LoadScene()`
`xksl.SceneManager` exposes the function `AttachPayload()`, which is the same function being used internally to attach a payload when our scene loads. If you are using an external function to load a scene, this will allow adding a payload without going through `xksl.SceneManager.LoadScene()`. If you are using `AttachPayload()` when calling a networked `LoadScene()`, note that only the host invoking the `LoadScene()` function will have the payload. If you'd like to share it with clients, you have to make your own protocol.

Users must ensure that only one `AttachPayload()` get called per `LoadScene()` as there's no way to strongly associate the payload to a scene. Failure to do so will just mean the next time the scene is loaded, it'll load using old data.

### Custom Monobehaviour
If you don't want to inherit `SceneInitializer`, you can inherit from `ISceneInitializer` instead. It is implicitly required that `ISceneInitializer` is inherited by a type derived from `Monobehaviour`, adding the interface to any other type is unsupported.
