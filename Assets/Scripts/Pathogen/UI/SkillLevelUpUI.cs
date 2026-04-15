using UnityEngine;
using System.Collections;

namespace Pathogen
{
    /// <summary>
    /// Thin coordinator that listens to champion level / skill-point events
    /// and triggers ordered stagger-show on each SkillButton's upgrade overlay.
    /// Each SkillButton owns its own "+" button and flash overlay.
    /// </summary>
    public class SkillLevelUpUI : MonoBehaviour
    {
        public Champion champion;
        public SkillButton[] skillButtons;      // The 4 SkillButton components

        private Coroutine staggerRoutine;

        // Lambda caches for safe unsubscription (IL2CPP)
        private System.Action<int> onPointsChanged;
        private System.Action<int> onLevelUp;

        void Start()
        {
            if (champion == null) return;

            onPointsChanged = _ => Refresh();
            onLevelUp = _ => Refresh();
            champion.OnSkillPointsChanged += onPointsChanged;
            champion.OnLevelUp += onLevelUp;

            Refresh();
        }

        void OnDestroy()
        {
            if (champion != null)
            {
                if (onPointsChanged != null)
                    champion.OnSkillPointsChanged -= onPointsChanged;
                if (onLevelUp != null)
                    champion.OnLevelUp -= onLevelUp;
            }
        }

        public void Refresh()
        {
            if (champion.pendingSkillPoints <= 0)
            {
                HideAll();
                return;
            }

            if (staggerRoutine != null)
                StopCoroutine(staggerRoutine);
            staggerRoutine = StartCoroutine(StaggerShow());
        }

        private IEnumerator StaggerShow()
        {
            for (int i = 0; i < skillButtons.Length; i++)
            {
                var sb = skillButtons[i];
                if (sb == null) continue;

                if (champion.CanRankUpSkill(i))
                {
                    sb.ShowUpgrade();
                    yield return new WaitForSeconds(0.08f); // ordered delay
                }
                else
                {
                    sb.HideUpgrade();
                }
            }
        }

        private void HideAll()
        {
            if (staggerRoutine != null)
            {
                StopCoroutine(staggerRoutine);
                staggerRoutine = null;
            }

            for (int i = 0; i < skillButtons.Length; i++)
            {
                if (skillButtons[i] != null)
                    skillButtons[i].HideUpgrade();
            }
        }
    }
}
