using UnityEngine;

namespace Pathogen
{
    public class StructureProjectile : MonoBehaviour
    {
        private Entity owner;
        private Vector3 direction;
        private float damage;
        private float speed;
        private float maxRange;
        private bool isTrueDamage;
        private Vector3 startPosition;
        private Rigidbody rb;

        public void Initialize(Entity owner, Vector3 direction, float damage,
            float speed, float maxRange, bool isTrueDamage)
        {
            this.owner = owner;
            this.direction = direction.normalized;
            this.damage = damage;
            this.speed = speed;
            this.maxRange = maxRange;
            this.isTrueDamage = isTrueDamage;
            this.startPosition = transform.position;
        }

        void Start()
        {
            rb = GetComponent<Rigidbody>();
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

            if (isTrueDamage)
                entity.TakeRawDamage(damage);
            else
                entity.TakeDamage(damage, false, owner);

            Destroy(gameObject);
        }
    }
}
