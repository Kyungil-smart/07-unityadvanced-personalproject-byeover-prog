using UnityEngine;

namespace _Game.Scripts.System
{
    [CreateAssetMenu(fileName = "StageSequence_", menuName = "RhythmGame/Stage Sequence Data")]
    public sealed class SequencerStageDataSO : ScriptableObject
    {
        [Header("시퀀서 설정 (1칸 = 0.5박자)")]
        [Tooltip("패턴의 총 길이 (16 = 8박자마다 루프, 32 = 16박자마다 루프)")]
        public int PatternLength = 16;

        [Header("1번 레인 (맨 위 - 까마귀)")]
        public GameObject Lane1Prefab;
        [Tooltip("1은 스폰, 0은 빈 공간입니다. (엇박이나 빈 공간을 만들어보세요)")]
        public string Lane1Pattern = "0000100000001000";

        [Header("2번 레인 (멧돼지)")]
        public GameObject Lane2Prefab;
        public string Lane2Pattern = "1000000010000000";

        [Header("3번 레인 (가운데 아래 - 유령)")]
        public GameObject Lane3Prefab;
        public string Lane3Pattern = "1000100010001000";

        [Header("4번 레인 (맨 아래 - 뱀)")]
        public GameObject Lane4Prefab;
        [Tooltip("뱀은 엇박자 스폰이 가장 어울립니다.")]
        public string Lane4Pattern = "0100000100000100";
    }
}