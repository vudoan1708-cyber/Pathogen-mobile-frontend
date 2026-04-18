using UnityEngine;
using System;

namespace Pathogen
{
    [Serializable]
    public class ChampionDefinition
    {
        public string championName;
        public Color color;
        public float championHeight = 1f;

        // Addressables key for the champion's visual model (FBX or wrapped prefab).
        // Loaded asynchronously at spawn. Leave empty to keep the primitive placeholder.
        public string modelAddress;
        public Vector3 modelLocalPosition;
        public Vector3 modelLocalEulerAngles;
        public float modelScale = 1f;

        // Base stats
        public float maxHealth = 500f;
        public float maxMana = 250f;
        public float attackDamage = 50f;
        public float attackSpeed = 1f;
        public float attackRange = 2.5f;
        public float moveSpeed = 3.5f;
        public float armor = 15f;
        public float magicResist = 12f;
        public float healthRegen = 2f;
        public float manaRegen = 4f;
        public float sightRange = 12f;

        // Per-level scaling
        public float healthPerLevel = 50f;
        public float manaPerLevel = 25f;
        public float attackDamagePerLevel = 3f;
        public float armorPerLevel = 2f;
        public float magicResistPerLevel = 1.5f;

        // Skills
        public SkillDefinition[] skills;
    }
}
