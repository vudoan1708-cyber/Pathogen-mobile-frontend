using UnityEngine;

namespace Pathogen
{
    /// <summary>
    /// Homing structure projectile — locks onto target and tracks until hit.
    /// Cannot be dodged. On destruction the attached muzzle-flash VFX detaches
    /// into world space and stops emitting, so its remaining particles fade
    /// out in place (dissolve) instead of disappearing with the projectile.
    /// </summary>
    public class StructureProjectile : MonoBehaviour
    {
        private Entity owner;
        private Entity target;
        private float damage;
        private float speed;
        private bool isTrueDamage;
        private float lifetime;
        private GameObject muzzleFlash;

        public void Initialize(Entity owner, Entity target, float damage,
            float speed, bool isTrueDamage, GameObject muzzleFlash = null)
        {
            this.owner = owner;
            this.target = target;
            this.damage = damage;
            this.speed = speed;
            this.isTrueDamage = isTrueDamage;
            this.lifetime = 5f;
            this.muzzleFlash = muzzleFlash;
        }

        void Update()
        {
            lifetime -= Time.deltaTime;
            if (lifetime <= 0f || target == null || target.IsDead)
            {
                DissolveAndDestroy();
                return;
            }

            Vector3 targetPos = target.transform.position + Vector3.up * 0.5f;
            Vector3 toTarget = targetPos - transform.position;
            float step = speed * Time.deltaTime;

            if (toTarget.sqrMagnitude <= step * step)
            {
                if (isTrueDamage)
                    target.TakeRawDamage(damage);
                else
                    target.TakeDamage(damage, false, owner);

                DissolveAndDestroy();
                return;
            }

            transform.position += toTarget.normalized * step;
        }

        private void DissolveAndDestroy()
        {
            if (muzzleFlash != null)
            {
                muzzleFlash.transform.SetParent(null, true);
                var ps = muzzleFlash.GetComponent<ParticleSystem>();
                if (ps != null)
                    ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                muzzleFlash = null;
            }
            Destroy(gameObject);
        }
    }
}
