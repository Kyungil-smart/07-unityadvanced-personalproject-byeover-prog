using UnityEngine;

public sealed class MonsterBeatMover : MonoBehaviour
{
    [Header("이동 설정")]
    [SerializeField, Tooltip("한 박자당 이동할 거리")] private float stepDistance = 1.2f;
    [SerializeField, Tooltip("이동 방향")] private Vector3 direction = Vector3.left;
    [SerializeField, Range(0.1f, 1.0f), Tooltip("박자 중 이동 시간 비율")] private float moveFraction = 0.8f;

    private Vector3 _startPos;
    private Vector3 _targetPos;
    private double _moveStartDsp;
    private double _duration;
    private bool _isMoving;

    public void OnBeat(BeatInfo info)
    {
        if (!gameObject.activeInHierarchy) return;

        _startPos = transform.position;
        _targetPos = _startPos + (direction.normalized * stepDistance);
        _moveStartDsp = info.BeatDspTime;
        _duration = info.BeatDuration * moveFraction;
        _isMoving = true;
    }

    private void Update()
    {
        if (!_isMoving) return;

        double now = AudioSettings.dspTime;
        float t = (float)((now - _moveStartDsp) / _duration);

        if (t >= 1f)
        {
            transform.position = _targetPos;
            _isMoving = false;
            return;
        }

        transform.position = Vector3.LerpUnclamped(_startPos, _targetPos, t);
    }
}