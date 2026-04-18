using UnityEngine;

namespace Pathogen
{
    [CreateAssetMenu(fileName = "ShaderLibrary", menuName = "Pathogen/Shader Library")]
    public class ShaderLibrary : ScriptableObject
    {
        public Shader urpUnlit;
        public Shader bioPulse;
        public Shader slimeBeam;
        public Shader uiEntityBar;
        public Shader uiEntityBarCanvas;
        public Shader uiHealthGradient;
        public Shader uiSkillCooldown;
        public Shader uiUpgradeButton;
        public Shader uiGoldButton;
        public Shader crosshair;

        private static ShaderLibrary cached;

        public static ShaderLibrary Instance
        {
            get
            {
                if (cached == null)
                    cached = Resources.Load<ShaderLibrary>(nameof(ShaderLibrary));
                return cached;
            }
        }
    }
}
