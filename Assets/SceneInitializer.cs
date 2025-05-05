using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace xksl
{
    public interface ISceneInitializer
    {
        protected internal DefaultSceneInitializer DefaultInitializer { get; set; }
        public GameObject gameObject { get; }
        protected internal abstract void Initialize();
    }

    public interface ISceneInitializeWith<T>
    {
        public GameObject gameObject { get; }
        protected internal abstract void Initialize(T args);
    }

    [Serializable]
    public abstract class DefaultSceneInitializer
    {
        internal abstract void Initialize(ISceneInitializer self);
    }

    public class DefaultSceneInitializer<T> : DefaultSceneInitializer
    {
        [SerializeField]
        public T args;

        internal sealed override void Initialize(ISceneInitializer self) 
        {
            Debug.Assert(self as ISceneInitializeWith<T> != null, "Somehow an initializer of a non-matching type was attached");

            SceneManager.Log("Invoking default initializer on ObjectID: " + self.gameObject.GetInstanceID() + " as " + typeof(ISceneInitializeWith<T>).GetMethod(nameof(ISceneInitializeWith<T>.Initialize)));
            (self as ISceneInitializeWith<T>).Initialize(args);
        }
    }

    public abstract class SceneInitializer : MonoBehaviour, ISceneInitializer
    {
        [SerializeReference]
        internal DefaultSceneInitializer defaultInitializer;

        DefaultSceneInitializer ISceneInitializer.DefaultInitializer { get => defaultInitializer; set => defaultInitializer = value; }

        void ISceneInitializer.Initialize()
        {
            Initialize();
        }

        protected abstract void Initialize();
    }
}