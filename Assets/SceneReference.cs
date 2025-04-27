using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace xksl
{
    public class SceneReference : ScriptableObject
    {
        [SerializeField]
        internal string path;

        public string Path => path;
    }

    public class SceneReference<T> : SceneReference where T : ISceneInitializer
    {
    }
}
