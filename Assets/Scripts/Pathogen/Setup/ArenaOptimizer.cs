using UnityEngine;
using UnityEngine.Rendering;

namespace Pathogen
{
    public static class ArenaOptimizer
    {
        public static void Apply(GameObject arenaInstance)
        {
            if (arenaInstance == null) return;

            DisableShadowCasting(arenaInstance);
            MarkStaticRecursive(arenaInstance.transform);
            StaticBatchingUtility.Combine(arenaInstance);
        }

        private static void DisableShadowCasting(GameObject arenaInstance)
        {
            foreach (var r in arenaInstance.GetComponentsInChildren<Renderer>())
                r.shadowCastingMode = ShadowCastingMode.Off;
        }

        private static void MarkStaticRecursive(Transform t)
        {
            t.gameObject.isStatic = true;
            for (int i = 0; i < t.childCount; i++)
                MarkStaticRecursive(t.GetChild(i));
        }
    }
}
