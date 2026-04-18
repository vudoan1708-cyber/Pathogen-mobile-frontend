using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Pathogen
{
    /// <summary>
    /// Asynchronously loads and attaches a champion's visual model via Addressables.
    /// Hides the placeholder primitive renderer once the model is in place, and
    /// releases the Addressables instance when the host champion is destroyed.
    /// </summary>
    public class ChampionModelLoader : MonoBehaviour
    {
        public bool IsLoaded { get; private set; }
        public GameObject ModelInstance { get; private set; }
        public event Action<GameObject> OnModelLoaded;

        private AsyncOperationHandle<GameObject> instantiateHandle;
        private Renderer placeholderRenderer;

        public static ChampionModelLoader Begin(
            GameObject host,
            string address,
            Renderer placeholderToHide,
            Vector3 localPosition,
            Vector3 localEulerAngles,
            float modelScale)
        {
            if (string.IsNullOrEmpty(address)) return null;

            var loader = host.AddComponent<ChampionModelLoader>();
            loader.placeholderRenderer = placeholderToHide;
            loader.StartInstantiate(address, localPosition, localEulerAngles, modelScale);
            return loader;
        }

        private void StartInstantiate(string address, Vector3 localPos, Vector3 localEuler, float modelScale)
        {
            instantiateHandle = Addressables.InstantiateAsync(address, transform);
            instantiateHandle.Completed += h => OnInstantiateCompleted(h, localPos, localEuler, modelScale);
        }

        private void OnInstantiateCompleted(
            AsyncOperationHandle<GameObject> handle,
            Vector3 localPos,
            Vector3 localEuler,
            float modelScale)
        {
            if (handle.Status != AsyncOperationStatus.Succeeded || handle.Result == null)
            {
                Debug.LogError($"[ChampionModelLoader] Failed to load champion model '{name}': {handle.OperationException}");
                return;
            }

            ModelInstance = handle.Result;
            ModelInstance.name = "Model";

            var modelTransform = ModelInstance.transform;
            modelTransform.localPosition = localPos;
            modelTransform.localRotation = Quaternion.Euler(localEuler);
            modelTransform.localScale *= modelScale;

            if (placeholderRenderer != null)
                placeholderRenderer.enabled = false;

            var modelAnimator = ModelInstance.GetComponentInChildren<Animator>();
            var champion = GetComponent<Champion>();
            if (modelAnimator != null && champion != null)
                ChampionAnimator.Attach(champion, modelAnimator);

            IsLoaded = true;
            OnModelLoaded?.Invoke(ModelInstance);
        }

        private void OnDestroy()
        {
            // ReleaseInstance both destroys the instance and decrements the asset ref-count.
            if (ModelInstance != null)
                Addressables.ReleaseInstance(ModelInstance);
        }
    }
}
