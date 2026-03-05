using System;
using UnityEngine;

namespace _Game.Scripts.Rhythm
{
    [DisallowMultipleComponent]
    public sealed class BossSpawnGuard : MonoBehaviour
    {
        [Header("차단 옵션")]
        [SerializeField, Tooltip("StageManager 경유가 아니면 즉시 파괴")] private bool destroyIfNotFromStageManager = true;
        [SerializeField, Tooltip("로그 출력 여부")] private bool logError = true;

        private void Awake()
        {
            if (BossSpawnContext.IsStageManagerSpawning) return;

            if (logError)
            {
                var stack = Environment.StackTrace;
                Debug.LogError($"[BossSpawnGuard] StageManager가 아닌 경로에서 보스가 생성됨. (원인 추적용)\nLastReason={BossSpawnContext.LastReason}\nStackTrace:\n{stack}", this);
            }

            if (destroyIfNotFromStageManager)
            {
                Destroy(gameObject);
            }
        }
    }
}