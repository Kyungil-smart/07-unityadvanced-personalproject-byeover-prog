using UnityEngine;

namespace _Game.Scripts.Rhythm
{
    [DisallowMultipleComponent]
    public sealed class PlayerNoteDamage2D : MonoBehaviour
    {
        [Header("참조")]
        [Tooltip("플레이어 체력")]
        [SerializeField] private PlayerHealth2D playerHealth;

        [Header("데미지")]
        [Tooltip("노트가 닿을 때 깎을 HP")]
        [Min(1)]
        [SerializeField] private int touchDamage = 1;

        private void Awake()
        {
            if (playerHealth == null)
                playerHealth = GetComponentInParent<PlayerHealth2D>();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var note = other.GetComponentInParent<NoteMonster2D>();
            if (note == null) return;

            playerHealth?.Damage(touchDamage);
            note.Despawn();
        }
    }
}