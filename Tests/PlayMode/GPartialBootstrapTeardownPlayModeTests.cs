#if UNITY_EDITOR && UNITY_INCLUDE_TESTS

using DingoProjectAppStructure.SceneRoot;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace DingoProjectAppStructure.Tests.PlayMode
{
    [Category("Runtime")]
    public class GPartialBootstrapTeardownPlayModeTests
    {
        [Test]
        public void DestroyBeforeModelRootExists_DoesNotEmitSecondaryException_AndDisposesDependencies()
        {
            var gameObject = new GameObject(nameof(GPartialBootstrapTeardownPlayModeTests));
            var registerer = gameObject.AddComponent<TrackingExternalDependenciesRegisterer>();
            var root = gameObject.AddComponent<G>();

            try
            {
                var serializedRoot = new SerializedObject(root);
                serializedRoot
                    .FindProperty("_externalDependenciesRegisterer")
                    .objectReferenceValue = registerer;
                serializedRoot.ApplyModifiedPropertiesWithoutUndo();

                Object.DestroyImmediate(root);

                Assert.That(registerer.DisposeCount, Is.EqualTo(1));
            }
            finally
            {
                Object.DestroyImmediate(gameObject);
            }
        }

        [Test]
        public void ApplicationQuitThenDestroy_DisposesDependenciesExactlyOnce()
        {
            var gameObject = new GameObject(nameof(GPartialBootstrapTeardownPlayModeTests));
            var registerer = gameObject.AddComponent<TrackingExternalDependenciesRegisterer>();
            var root = gameObject.AddComponent<G>();

            try
            {
                var serializedRoot = new SerializedObject(root);
                serializedRoot
                    .FindProperty("_externalDependenciesRegisterer")
                    .objectReferenceValue = registerer;
                serializedRoot.ApplyModifiedPropertiesWithoutUndo();

                root.SendMessage(
                    "OnApplicationQuit",
                    SendMessageOptions.DontRequireReceiver);
                Object.DestroyImmediate(root);

                Assert.That(registerer.DisposeCount, Is.EqualTo(1));
            }
            finally
            {
                Object.DestroyImmediate(gameObject);
            }
        }
    }

    public class TrackingExternalDependenciesRegisterer : ExternalDependenciesRegistererBase
    {
        public int DisposeCount;

        public override void Dispose()
        {
            DisposeCount++;
        }
    }
}

#endif
