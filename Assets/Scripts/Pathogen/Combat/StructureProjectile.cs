using UnityEngine;

namespace Pathogen
{
    /// <summary>
    /// Homing structure projectile — locks onto target and tracks until hit.
    /// Cannot be dodged.
    /// </summary>
    public class StructureProjectile : MonoBehaviour
    {
        private Entity owner;
        private Entity target;
        private float damage;
        private float speed;
        private bool isTrueDamage;
        private float lifetime;

        public void Initialize(Entity owner, Entity target, float damage,
            float speed, bool isTrueDamage)
        {
            this.owner = owner;
            this.target = target;
            this.damage = damage;
            this.speed = speed;
            this.isTrueDamage = isTrueDamage;
            this.lifetime = 5f;
        }

        void Update()
        {
            lifetime -= Time.deltaTime;
            if (lifetime <= 0f || target == null || target.IsDead)
            {
                Destroy(gameObject);
                return;
            }

            Vector3 targetPos = target.transform.position + Vector3.up * 0.5f;
            Vector3 toTarget = targetPos - transform.position;
            float step = speed * Time.deltaTime;

            if (toTarget.sqrMagnitude <= step * step)
            {
                // Hit
                if (isTrueDamage)
                    target.TakeRawDamage(damage);
                else
                    target.TakeDamage(damage, false, owner);

                Destroy(gameObject);
                return;
            }

            transform.position += toTarget.normalized * step;
        }
    }
}
