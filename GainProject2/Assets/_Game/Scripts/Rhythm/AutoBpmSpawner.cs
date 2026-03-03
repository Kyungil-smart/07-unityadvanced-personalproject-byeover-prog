using UnityEngine;
using GnalIhu.Rhythm;

namespace _Game.Scripts.Rhythm
{
    [DisallowMultipleComponent]
    public sealed class AutoBpmSpawner : MonoBehaviour
    {
        [Header("시스템 연결")]
        [SerializeField] private RhythmConductor conductor;
        [SerializeField] private NotePool notePool;
        [SerializeField] private JudgeSystem judgeSystem;

        [Header("스폰 및 레인 설정")]
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private Transform hitLine;
        
        [Header("자동 생성 규칙")]
        [Tooltip("몇 박자마다 몬스터를 생성할 것인가? (1 = 매 박자, 2 = 2박자마다, 4 = 4박자마다)")]
        [SerializeField, Min(1)] private int spawnEveryXBeats = 1;
        
        [Tooltip("생성된 몬스터가 판정선까지 도달하는 데 걸리는 시간(초)")]
        [SerializeField, Min(0.1f)] private float travelTimeSeconds = 1.5f;

        private void OnEnable()
        {
            if (conductor != null)
            {
                conductor.OnBeat += HandleBeat;
            }
        }

        private void OnDisable()
        {
            if (conductor != null)
            {
                conductor.OnBeat -= HandleBeat;
            }
        }

        private void HandleBeat(int beatIndex)
        {
            if (beatIndex < 0) return;

            if (beatIndex % spawnEveryXBeats == 0)
            {
                double targetHitTimeSec = conductor.SongTime + travelTimeSeconds;
                SpawnMonster(targetHitTimeSec);
            }
        }

        private void SpawnMonster(double targetHitTimeSec)
        {
            if (notePool == null || spawnPoint == null || hitLine == null) return;

            var note = notePool.Rent();
            if (note == null) return;

            note.transform.SetParent(null, true);
            
            note.Init(conductor, spawnPoint, hitLine, targetHitTimeSec);

            if (judgeSystem != null)
            {
                judgeSystem.Register(note);
            }
        }
    }
}