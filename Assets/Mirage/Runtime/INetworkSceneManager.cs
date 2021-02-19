using System;
using UnityEngine.Events;

namespace Mirage
{
    public enum SceneOperation : byte
    {
        Normal,
        LoadAdditive,
        UnloadAdditive
    }

    [Serializable] public class SceneChangeEvent : UnityEvent<string, SceneOperation> { }

    public interface INetworkSceneManager
    {
        /// <summary>
        /// Event fires when the Client starts changing scene.
        /// </summary>
        SceneChangeEvent ClientChangeScene { get; }

        /// <summary>
        /// Event fires after the Client has completed its scene change.
        /// </summary>
        SceneChangeEvent ClientSceneChanged { get; }

        /// <summary>
        /// Event fires before Server changes scene.
        /// </summary>
        SceneChangeEvent ServerChangeScene { get; }

        /// <summary>
        /// Event fires after Server has completed scene change.
        /// </summary>
        SceneChangeEvent ServerSceneChanged { get; }

        void ChangeServerScene(string scenePath, SceneOperation sceneOperation = SceneOperation.Normal);
    }
}
