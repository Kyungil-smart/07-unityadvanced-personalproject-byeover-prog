using System;
using UnityEngine;

namespace _Game.Scripts.System
{
    [DisallowMultipleComponent]
    public sealed class StageObstacleController : MonoBehaviour
    {
        [Serializable]
        public struct ObstacleEntry
        {
            [Tooltip("방해요소 타입")] public StageObstacleType type;
            [Tooltip("해당 타입일 때 켤 오브젝트")] public GameObject root;
        }

        [Header("방해요소")]
        [SerializeField, Tooltip("스테이지 방해요소 매핑")]
        private ObstacleEntry[] obstacles;

        public void Apply(StageObstacleType type)
        {
            if (obstacles == null) return;

            for (int i = 0; i < obstacles.Length; i++)
            {
                var entry = obstacles[i];
                if (entry.root == null) continue;
                entry.root.SetActive(type != StageObstacleType.None && entry.type == type);
            }
        }

        public void ClearAll()
        {
            if (obstacles == null) return;

            for (int i = 0; i < obstacles.Length; i++)
            {
                var entry = obstacles[i];
                if (entry.root == null) continue;
                entry.root.SetActive(false);
            }
        }
    }
}