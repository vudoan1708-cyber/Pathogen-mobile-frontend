using UnityEngine;

namespace Pathogen
{
    /// <summary>
    /// Bridges Champion gameplay events to Animator parameters.
    /// Attached at runtime by ChampionModelLoader once the visual model is loaded.
    ///
    /// Expected Animator Controller parameters:
    ///   Bool    IsMoving
    ///   Trigger Attack, Skill1, Skill2, Skill3, Skill4, Recall, Die, Respawn
    /// </summary>
    public class ChampionAnimator : MonoBehaviour
    {
        private static readonly int IsMovingHash = Animator.StringToHash("IsMoving");
        private static readonly int AttackHash = Animator.StringToHash("Attack");
        private static readonly int RecallHash = Animator.StringToHash("Recall");
        private static readonly int DieHash = Animator.StringToHash("Die");
        private static readonly int RespawnHash = Animator.StringToHash("Respawn");
        private static readonly int[] SkillHashes =
        {
            Animator.StringToHash("Skill1"),
            Animator.StringToHash("Skill2"),
            Animator.StringToHash("Skill3"),
            Animator.StringToHash("Skill4")
        };

        // Below this units/sec threshold the champion is considered standing still.
        private const float MovementSpeedThreshold = 0.1f;

        private Animator animator;
        private Champion champion;
        private Vector3 previousPosition;

        public static void Attach(Champion champion, Animator animator)
        {
            if (champion == null || animator == null) return;

            var driver = champion.gameObject.AddComponent<ChampionAnimator>();
            driver.champion = champion;
            driver.animator = animator;
            driver.previousPosition = champion.transform.position;
            driver.Subscribe();
        }

        private void Subscribe()
        {
            champion.OnSkillUsed += HandleSkillUsed;
            champion.OnAutoAttackPerformed += HandleAutoAttack;
            champion.OnDeath += HandleDeath;
            champion.OnRespawn += HandleRespawn;
            champion.OnRecallStarted += HandleRecall;
        }

        private void OnDestroy()
        {
            if (champion == null) return;
            champion.OnSkillUsed -= HandleSkillUsed;
            champion.OnAutoAttackPerformed -= HandleAutoAttack;
            champion.OnDeath -= HandleDeath;
            champion.OnRespawn -= HandleRespawn;
            champion.OnRecallStarted -= HandleRecall;
        }

        private void Update()
        {
            if (animator == null || champion == null) return;

            Vector3 position = champion.transform.position;
            float deltaTime = Mathf.Max(Time.deltaTime, 0.0001f);
            float speed = (position - previousPosition).magnitude / deltaTime;
            animator.SetBool(IsMovingHash, speed > MovementSpeedThreshold);
            previousPosition = position;
        }

        private void HandleSkillUsed(int index)
        {
            if (index < 0 || index >= SkillHashes.Length) return;
            animator.SetTrigger(SkillHashes[index]);
        }

        private void HandleAutoAttack() => animator.SetTrigger(AttackHash);
        private void HandleDeath(Entity _) => animator.SetTrigger(DieHash);
        private void HandleRespawn() => animator.SetTrigger(RespawnHash);
        private void HandleRecall() => animator.SetTrigger(RecallHash);
    }
}
