using UnityEngine;

namespace Pathogen
{
    /// <summary>
    /// Holds direct prefab references for runtime VFX so Unity's build analyzer
    /// keeps the prefab chain (material, shader, variants) alive through stripping.
    /// Mirrors <see cref="ShaderLibrary"/>. Edit-time prefab generation lives in
    /// Assets/Scripts/Pathogen/Setup/Editor/VFXPrefabBuilder.cs.
    /// </summary>
    [CreateAssetMenu(fileName = "VFXLibrary", menuName = "Pathogen/VFX Library")]
    public class VFXLibrary : ScriptableObject
    {
        public GameObject structureMuzzleFlash;

        private static VFXLibrary cached;

        public static VFXLibrary Instance
        {
            get
            {
                if (cached == null)
                    cached = Resources.Load<VFXLibrary>(nameof(VFXLibrary));
                return cached;
            }
        }
    }
}
