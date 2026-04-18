#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Pathogen.Editor
{
    /// <summary>
    /// Builds the runtime VFX prefabs referenced by <see cref="VFXLibrary"/>.
    /// Artists can open the generated prefabs in the Inspector / Scene view to
    /// tune emission, curves, and gradients without touching code — the
    /// ParticleSystem authored here is the single source of truth at runtime.
    /// </summary>
    public static class VFXPrefabBuilder
    {
        private const string ResourcesRoot = "Assets/Resources";
        private const string VFXFolder = "Assets/Resources/VFX";
        private const string MuzzleFlashMatPath = VFXFolder + "/StructureMuzzleFlash.mat";
        private const string MuzzleFlashPrefabPath = VFXFolder + "/StructureMuzzleFlash.prefab";
        private const string SoftParticleTexPath = VFXFolder + "/SoftParticle.png";
        private const string VFXLibraryPath = ResourcesRoot + "/VFXLibrary.asset";

        [MenuItem("Pathogen/Build VFX Prefabs", false, 10)]
        public static void BuildAll()
        {
            EnsureFolder(ResourcesRoot);
            EnsureFolder(VFXFolder);

            var tex = BuildSoftParticleTexture();
            var mat = BuildMuzzleFlashMaterial(tex);
            var prefab = BuildMuzzleFlashPrefab(mat);
            LinkVFXLibrary(prefab);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Pathogen] VFX prefabs built: " + MuzzleFlashPrefabPath);
        }

        private static Texture2D BuildSoftParticleTexture()
        {
            const int size = 128;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false, true);
            float half = size * 0.5f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = (x - half) / half;
                    float dy = (y - half) / half;
                    float d = Mathf.Sqrt(dx * dx + dy * dy);
                    float a = Mathf.Clamp01(1f - d);
                    a = a * a * (3f - 2f * a); // smoothstep for soft falloff
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
                }
            }
            tex.Apply();

            File.WriteAllBytes(SoftParticleTexPath, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);

            AssetDatabase.ImportAsset(SoftParticleTexPath, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(SoftParticleTexPath);
            importer.textureType = TextureImporterType.Default;
            importer.alphaSource = TextureImporterAlphaSource.FromInput;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Bilinear;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.sRGBTexture = true;
            importer.SaveAndReimport();

            return AssetDatabase.LoadAssetAtPath<Texture2D>(SoftParticleTexPath);
        }

        private static Material BuildMuzzleFlashMaterial(Texture2D softParticle)
        {
            var shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (shader == null)
            {
                Debug.LogError("[Pathogen] URP Particles/Unlit shader not found — is URP installed?");
                return null;
            }

            var mat = new Material(shader) { name = "StructureMuzzleFlash" };

            // URP Particles/Unlit → Transparent + Additive
            mat.SetFloat("_Surface", 1f);
            mat.SetFloat("_Blend", 2f);
            mat.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            mat.SetFloat("_DstBlend", (float)BlendMode.One);
            mat.SetFloat("_ZWrite", 0f);
            mat.SetFloat("_AlphaClip", 0f);
            mat.SetColor("_BaseColor", Color.white);
            mat.SetColor("_EmissionColor", Color.white);
            mat.SetTexture("_BaseMap", softParticle);
            mat.DisableKeyword("_SURFACE_TYPE_OPAQUE");
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.EnableKeyword("_ALPHAMODULATE_ON");
            mat.renderQueue = (int)RenderQueue.Transparent;

            AssetDatabase.CreateAsset(mat, MuzzleFlashMatPath);
            return AssetDatabase.LoadAssetAtPath<Material>(MuzzleFlashMatPath);
        }

        private static GameObject BuildMuzzleFlashPrefab(Material material)
        {
            var root = new GameObject("StructureMuzzleFlash");
            var ps = root.AddComponent<ParticleSystem>();

            ConfigureMuzzleFlashParticles(ps);

            var psr = root.GetComponent<ParticleSystemRenderer>();
            psr.renderMode = ParticleSystemRenderMode.Billboard;
            psr.material = material;
            psr.sortingFudge = -2f;
            psr.shadowCastingMode = ShadowCastingMode.Off;
            psr.receiveShadows = false;
            psr.lightProbeUsage = LightProbeUsage.Off;
            psr.reflectionProbeUsage = ReflectionProbeUsage.Off;

            if (File.Exists(MuzzleFlashPrefabPath))
                AssetDatabase.DeleteAsset(MuzzleFlashPrefabPath);

            var saved = PrefabUtility.SaveAsPrefabAsset(root, MuzzleFlashPrefabPath);
            Object.DestroyImmediate(root);
            return saved;
        }

        private static void ConfigureMuzzleFlashParticles(ParticleSystem ps)
        {
            var main = ps.main;
            main.duration = 2f;
            main.loop = true;
            main.startLifetime = 0.4f;
            main.startSpeed = 0f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.55f, 0.9f);
            main.startColor = new Color(1f, 1f, 1f, 0.95f);
            main.maxParticles = 24;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.gravityModifier = 0f;
            main.playOnAwake = true;
            main.stopAction = ParticleSystemStopAction.Destroy;

            var emission = ps.emission;
            emission.enabled = true;
            emission.rateOverTime = 32f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 5) });

            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.1f;
            shape.radiusThickness = 0f;

            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(
                new[]
                {
                    new GradientColorKey(Color.white, 0f),
                    new GradientColorKey(Color.white, 1f)
                },
                new[]
                {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(1f, 0.75f),
                    new GradientAlphaKey(0f, 1f)
                });
            colorOverLifetime.color = new ParticleSystem.MinMaxGradient(gradient);

            var sizeOverLifetime = ps.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            var sizeCurve = new AnimationCurve(
                new Keyframe(0f, 0.7f),
                new Keyframe(0.15f, 1f),
                new Keyframe(0.85f, 1f),
                new Keyframe(1f, 0.3f));
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);
        }

        private static void LinkVFXLibrary(GameObject prefab)
        {
            var lib = AssetDatabase.LoadAssetAtPath<VFXLibrary>(VFXLibraryPath);
            if (lib == null)
            {
                lib = ScriptableObject.CreateInstance<VFXLibrary>();
                AssetDatabase.CreateAsset(lib, VFXLibraryPath);
            }
            lib.structureMuzzleFlash = prefab;
            EditorUtility.SetDirty(lib);
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            var parent = Path.GetDirectoryName(path).Replace('\\', '/');
            var leaf = Path.GetFileName(path);
            if (!AssetDatabase.IsValidFolder(parent))
                EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, leaf);
        }
    }
}
#endif
