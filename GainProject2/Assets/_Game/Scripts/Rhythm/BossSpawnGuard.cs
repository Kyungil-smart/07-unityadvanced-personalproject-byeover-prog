using System;
using UnityEngine;
using _Game.Scripts.UI;

namespace _Game.Scripts.Rhythm
{
    [DisallowMultipleComponent]
    public sealed class BossSpawnGuard : MonoBehaviour
    {
        [Header("차단 옵션")]
        [SerializeField, Tooltip("StageManager 경유가 아니면 즉시 파괴")]
        private bool destroyIfNotFromStageManager = true;

        [SerializeField, Tooltip("로그 출력 여부")]
        private bool logError = true;

        [Header("HP바 자동 연동")]
        [SerializeField, Tooltip("Awake에서 BossHpBarUI를 자동으로 찾아 BossController에 연결")]
        private bool autoBindHpBar = true;

        [SerializeField, Tooltip("보스 머리 위 HP바 오프셋 (월드 Y)")]
        private float hpBarYOffset = 2.5f;

        private void Awake()
        {
            if (!BossSpawnContext.IsStageManagerSpawning)
            {
                if (logError)
                {
                    var stack = Environment.StackTrace;
                    Debug.LogError(
                        $"[BossSpawnGuard] StageManager가 아닌 경로에서 보스가 생성됨.\n" +
                        $"LastReason={BossSpawnContext.LastReason}\nStackTrace:\n{stack}", this);
                }

                if (destroyIfNotFromStageManager)
                {
                    Destroy(gameObject);
                    return;
                }
            }

            // HP바 자동 연동
            if (autoBindHpBar)
            {
                BindHpBar();
            }
        }

        private void BindHpBar()
        {
            var bossController = GetComponent<BossController>();
            if (bossController == null)
            {
                // 부모에 있을 수도 있음
                bossController = GetComponentInParent<BossController>();
            }

            if (bossController == null) return;

            // 씬에서 BossHpBarUI 찾기
            var hpBarUI = FindFirstObjectByType<BossHpBarUI>(FindObjectsInactive.Include);
            if (hpBarUI == null) return;

            bossController.SetHpBarUI(hpBarUI);
            hpBarUI.Show();
            hpBarUI.SetNormalized(1f);
        }
    }
}