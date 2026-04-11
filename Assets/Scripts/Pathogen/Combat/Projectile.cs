using UnityEngine;

namespace Pathogen
{
    public class Projectile : MonoBehaviour
    {
        private Entity owner;
        private Vector3 direction;
        private float damage;
        private float speed;
        private float maxRange;
        private bool isMagic;
        private Vector3 startPosition;
        private Rigidbody rb;
        private ProjectilePiercing piercing;
        private SkillVisuals visuals;

        public void Initialize(Entity owner, Vector3 direction, float damage,
            float speed, float maxRange, bool isMagic,
            ProjectilePiercing piercing, SkillVisuals visuals)
        {
            this.owner = owner;
            this.direction = direction.normalized;
            this.damage = damage;
            this.speed = speed;
            this.maxRange = maxRange;
            this.isMagic = isMagic;
            this.startPosition = transform.position;
            this.piercing = piercing;
            this.visuals = visuals;
        }

        void Start()
        {
            rb = GetComponent<Rigidbody>();

            if (visuals != null && visuals.hasTrail)
                AddTrail();
        }

        void FixedUpdate()
        {
            if (rb != null)
                rb.MovePosition(rb.position + direction * speed * Time.fixedDeltaTime);
            else
                transform.position += direction * speed * Time.deltaTime;

            if (Vector3.Distance(startPosition, transform.position) > maxRange)
                Destroy(gameObject);
        }

        void OnTriggerEnter(Collider other)
        {
            var entity = other.GetComponent<Entity>();
            if (entity == null || entity.IsDead) return;
            if (owner != null && entity.team == owner.team) return;
            if (entity.entityType == EntityType.Structure) return;

            entity.TakeDamage(damage, isMagic, owner, visuals);

            switch (piercing)
            {
                case ProjectilePiercing.StopOnFirst:
                    Destroy(gameObject);
                    break;
                case ProjectilePiercing.PierceMinions:
                    if (entity.entityType == EntityType.Champion)
                        Destroy(gameObject);
                    break;
                case ProjectilePiercing.PierceAll:
                    break;
            }
        }

        private void AddTrail()
        {
            var trail = gameObject.AddComponent<TrailRenderer>();
            trail.startWidth = visuals.trailWidth;
            trail.endWidth = 0f;
            trail.time = 0.2f;
            trail.material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            trail.startColor = visuals.trailColor;
            trail.endColor = new Color(visuals.trailColor.r, visuals.trailColor.g, visuals.trailColor.b, 0f);
            trail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }
    }
}
