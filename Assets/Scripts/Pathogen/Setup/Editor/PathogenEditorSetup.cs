#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Pathogen.Editor
{
    /// <summary>
    /// Editor menu items for quick Pathogen scene setup.
    /// </summary>
    public static class PathogenEditorSetup
    {
        [MenuItem("Pathogen/Setup Scene (Add Bootstrap)", false, 1)]
        public static void SetupScene()
        {
            // Check if bootstrap already exists
            var existing = Object.FindAnyObjectByType<GameBootstrap>();
            if (existing != null)
            {
                EditorUtility.DisplayDialog("Pathogen",
                    "GameBootstrap already exists in the scene!", "OK");
                Selection.activeGameObject = existing.gameObject;
                return;
            }

            var go = new GameObject("── PATHOGEN BOOTSTRAP ──");
            go.AddComponent<GameBootstrap>();
            Selection.activeGameObject = go;
            EditorSceneManager.MarkSceneDirty(
                EditorSceneManager.GetActiveScene());

            Debug.Log("[Pathogen] Bootstrap added to scene. Press Play to start!");
        }

        [MenuItem("Pathogen/Clear Scene (Remove All Pathogen Objects)", false, 2)]
        public static void ClearScene()
        {
            if (!EditorUtility.DisplayDialog("Pathogen",
                "Remove all Pathogen objects from the scene?", "Yes", "Cancel"))
                return;

            var bootstrap = Object.FindAnyObjectByType<GameBootstrap>();
            if (bootstrap != null)
                Object.DestroyImmediate(bootstrap.gameObject);

            Debug.Log("[Pathogen] Scene cleared.");
        }
    }
}
#endif
