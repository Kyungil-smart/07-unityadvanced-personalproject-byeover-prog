using UnityEngine;

[DisallowMultipleComponent]
public sealed class NoteDamageProvider : MonoBehaviour, INoteDamageProvider
{
    [Header("데미지")]
    [Tooltip("플레이어가 맞았을 때 적용할 데미지")]
    [SerializeField, Min(0)] private int damage = 1;

    public int Damage => damage;
}