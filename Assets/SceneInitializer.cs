using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace xksl
{
    public interface ISceneInitializer
    {
        protected internal DefaultSceneInitializer DefaultInitializer { get; set; }
        GameObject gameObject { get; }
        void Initialize();
    }

    public interface ISceneInitializeWith<T>
    {
        void Initialize(T args);
    }

    [Serializable]
    public abstract class DefaultSceneInitializer
    {
        public abstract void Initialize(SceneInitializer self);
    }

    public class DefaultSceneInitializer<T> : DefaultSceneInitializer
    {
        [SerializeField]
        public T args;

        public override void Initialize(SceneInitializer self)
        {
            Debug.Assert(self as ISceneInitializeWith<T> != null, "Somehow an initializer of a non-matching type was attached");

            SceneManager.Log("Invoking default initializer on ObjectID: " + self.GetInstanceID() + " as " + typeof(ISceneInitializeWith<T>).GetMethod(nameof(ISceneInitializeWith<T>.Initialize)));
            (self as ISceneInitializeWith<T>).Initialize(args);
        }
    }

    public abstract class SceneInitializer : MonoBehaviour, ISceneInitializer
    {
        [SerializeReference]
        internal DefaultSceneInitializer defaultInitializer;

        DefaultSceneInitializer ISceneInitializer.DefaultInitializer { get => defaultInitializer; set => defaultInitializer = value; }

        virtual public void Initialize()
        {

        }
    }
}