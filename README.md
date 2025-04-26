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
}

//Create a class which inherits from DefaultSceneInitializer<Payload Type> to expose
//it to the editor.
public class MyPayloadDefault : DefaultSceneInitializer<MyPayload> {}

//Create a class which inherits from SceneInitializer
//Optionally, you can inherit from ISceneInitializeWith<Payload Type> to expose
//additional Initialize() methods with custom payloads
public class MySceneInitializer : SceneInitializer, ISceneInitializeWith<MyPayload>
{
    //SceneInitializer is a component, you can add variables here as well
    //to expose to the editor.

    //Exposed with SceneInitializer
    public override void Initialize()
    {

    }

    //Exposed with ISceneInitializeWith<MyPayload>
    public void Initialize(MyPayload args)
    {

    }
}
```
2. Create an Initializer in the scene. There are 2 approaches, you can do it manually by creating a game object and attaching an initializer script to it, or you can open the Initializer Editor under Scenes->Initializer Window
![Editor Image](/Screenshots/EditorExample.png)
3. Fill in the data needed for your initializer.
4. Call `xksl.SceneManager.LoadScene()`

# FAQ
### When is the Initializer Invoked?
The initializer just listens to `SceneManager.sceneLoaded` in order be invoked, which gets called before `MonoBehaviour.Start()` and after `MonoBehaviour.OnEnabled()` gets called.

### How do you know what type of payloads can be passed when loading scenes?
Sadly I don't. I can only rely on runtime checks to see if there's an initializer that has a matching `ISceneInitializeWith<>` with the payload type.

### What types are supported in the payload?
There's no restriction except for if you want the payload to be exposed in the editor. Only types that play nice with the editor will get exposed. The other exception would be live game objects. Payloads can hold reference to them, but by themselves don't guarantee their lifetime, so it's up to the user to ensure they stay alive across scene transitions.

### What's the point of the `DefaultSceneInitializer`?
The default scene initializer allows you to pass in a payload as the start up scene so you don't have to create a 2nd scene to load in from. This allows for easier testing if your scene is meant to be loaded.

# Advanced
### Attaching a payload without calling `xksl.SceneManager.LoadScene()`
`xksl.SceneManager` exposes the function `AttachPayload()`, which is the same function being used internally to attach a payload when our scene loads. If you are using an external function to load a scene, this will allow adding a payload without going through `xksl.SceneManager.LoadScene()`. If you are using `AttachPayload()` when calling a networked `LoadScene()`, note that only the host invoking the `LoadScene()` function will have the payload. If you'd like to share it with clients, you have to make your own protocol.

Users must ensure that only one `AttachPayload()` get called per `LoadScene()` as there's no way to strongly associate the payload to a scene. Failure to do so will just mean the next time the scene is loaded, it'll load using old data.

### Custom Monobehaviour
If you don't want to inherit `SceneInitializer`, you can inherit from `ISceneInitializer` instead. It is implicitly required that `ISceneInitializer` is inherited by a type derived from `Monobehaviour`, adding the interface to any other type is unsupported.
