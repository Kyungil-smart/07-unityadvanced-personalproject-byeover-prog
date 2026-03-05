using UnityEngine;

namespace _Game.Scripts.System
{
    [DisallowMultipleComponent]
    public sealed class LaneVisibilityController : MonoBehaviour
    {
        [Header("레인 오브젝트")]
        [SerializeField, Tooltip("1번 레인 루트")]
        private GameObject lane1;

        [SerializeField, Tooltip("2번 레인 루트")]
        private GameObject lane2;

        [SerializeField, Tooltip("3번 레인 루트")]
        private GameObject lane3;

        [SerializeField, Tooltip("4번 레인 루트")]
        private GameObject lane4;

        public void SetActiveLaneCount(int count)
        {
            int c = Mathf.Clamp(count, 1, 4);

            if (lane1 != null) lane1.SetActive(c >= 1);
            if (lane2 != null) lane2.SetActive(c >= 2);
            if (lane3 != null) lane3.SetActive(c >= 3);
            if (lane4 != null) lane4.SetActive(c >= 4);
        }

        public void SetActiveLaneMask(bool lane1Active, bool lane2Active, bool lane3Active, bool lane4Active)
        {
            if (lane1 != null) lane1.SetActive(lane1Active);
            if (lane2 != null) lane2.SetActive(lane2Active);
            if (lane3 != null) lane3.SetActive(lane3Active);
            if (lane4 != null) lane4.SetActive(lane4Active);
        }
    }
}
