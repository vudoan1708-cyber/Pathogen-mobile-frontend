using UnityEngine;

namespace Pathogen
{
    /// <summary>
    /// Draws skill aim indicators when holding a skill key.
    /// All visuals use world-space positioning so champion movement/rotation
    /// doesn't misalign the indicator.
    /// </summary>
    public class SkillAimIndicator : MonoBehaviour
    {
        private LineRenderer aimLine;
        private GameObject aoeCircle;
        private bool isActive;

        [Header("Visuals")]
        public Color lineColor = new Color(1f, 1f, 0.3f, 0.7f);
        public Color aoeColor = new Color(1f, 0.4f, 0.2f, 0.3f);

        void Awake()
        {
            CreateAimLine();
            CreateAOECircle();
            Hide();
        }

        private void CreateAimLine()
        {
            // Not parented to champion — avoids rotation issues
            var lineGO = new GameObject("AimLine");
            aimLine = lineGO.AddComponent<LineRenderer>();
            aimLine.useWorldSpace = true;
            aimLine.positionCount = 2;
            aimLine.startWidth = 0.15f;
            aimLine.endWidth = 0.08f;
            aimLine.material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            aimLine.material.color = lineColor;
            aimLine.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            aimLine.receiveShadows = false;
            lineGO.SetActive(false);
        }

        private void CreateAOECircle()
        {
            // Not parented to champion — positioned in world space
            aoeCircle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            aoeCircle.name = "AOEIndicator";
            DestroyImmediate(aoeCircle.GetComponent<CapsuleCollider>());

            var renderer = aoeCircle.GetComponent<Renderer>();
            renderer.material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            renderer.material.color = aoeColor;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            aoeCircle.SetActive(false);
        }

        public void ShowDirectionLine(Vector3 origin, Vector3 direction, float range)
        {
            isActive = true;
            aimLine.gameObject.SetActive(true);
            aoeCircle.SetActive(false);

            // Keep line at ground level so it aligns with the mouse cursor
            Vector3 start = new Vector3(origin.x, 0.05f, origin.z);
            Vector3 end = start + direction.normalized * range;
            aimLine.SetPosition(0, start);
            aimLine.SetPosition(1, end);
        }

        public void ShowAOECircle(Vector3 position, float radius)
        {
            isActive = true;
            aimLine.gameObject.SetActive(false);
            aoeCircle.SetActive(true);

            aoeCircle.transform.position = position + Vector3.up * 0.05f;
            aoeCircle.transform.localScale = new Vector3(radius * 2f, 0.02f, radius * 2f);
        }

        public void ShowDirectionWithAOE(Vector3 origin, Vector3 direction, float range, float aoeRadius)
        {
            isActive = true;
            aimLine.gameObject.SetActive(true);
            aoeCircle.SetActive(true);

            Vector3 start = new Vector3(origin.x, 0.05f, origin.z);
            Vector3 end = start + direction.normalized * range;
            aimLine.SetPosition(0, start);
            aimLine.SetPosition(1, end);

            aoeCircle.transform.position = new Vector3(end.x, 0.05f, end.z);
            aoeCircle.transform.localScale = new Vector3(aoeRadius * 2f, 0.02f, aoeRadius * 2f);
        }

        public void Hide()
        {
            isActive = false;
            if (aimLine != null) aimLine.gameObject.SetActive(false);
            if (aoeCircle != null) aoeCircle.SetActive(false);
        }

        void OnDestroy()
        {
            if (aimLine != null) Destroy(aimLine.gameObject);
            if (aoeCircle != null) Destroy(aoeCircle);
        }

        private static readonly Color cancelColor = new Color(1f, 0.2f, 0.2f, 0.3f);

        public void SetCancelTint(bool cancelling)
        {
            Color color = cancelling ? cancelColor : lineColor;
            if (aimLine != null) aimLine.material.color = color;
            if (aoeCircle != null) aoeCircle.GetComponent<Renderer>().material.color =
                cancelling ? cancelColor : aoeColor;
        }

        public bool IsActive => isActive;
    }
}
