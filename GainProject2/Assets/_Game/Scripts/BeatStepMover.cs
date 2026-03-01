using UnityEngine;

public sealed class BeatStepMover : MonoBehaviour
{
    [Header("이동")]
    [SerializeField, Tooltip("이동 방향")] private Vector3 stepDirection = Vector3.left;
    [SerializeField, Tooltip("로컬 좌표 이동 여부")] private bool useLocalSpace;

    private GameEventHub events;
    private float stepDistance;
    private float moveFraction;

    private Vector3 startPos;
    private Vector3 endPos;

    private double moveStartDspTime;
    private double moveDuration;
    private bool moving;

    public void Initialize(GameEventHub hub, GameSettingsSO settings)
    {
        events = hub;
        stepDistance = settings != null ? settings.NoteStepDistance : 1f;
        moveFraction = settings != null ? Mathf.Clamp01(settings.NoteMoveFraction) : 0.8f;
    }

    private void OnEnable()
    {
        if (events != null)
            events.Beat += OnBeat;
    }

    private void OnDisable()
    {
        if (events != null)
            events.Beat -= OnBeat;
    }

    private void OnBeat(BeatInfo info)
    {
        if (!isActiveAndEnabled) return;

        moveDuration = Mathf.Max(0.01f, info.BeatDuration * moveFraction);
        moveStartDspTime = info.BeatDspTime;

        var pos = useLocalSpace ? transform.localPosition : transform.position;
        startPos = pos;
        endPos = pos + (stepDirection.normalized * stepDistance);

        moving = true;
    }

    private void Update()
    {
        if (!moving) return;

        var now = AudioSettings.dspTime;
        var t = (now - moveStartDspTime) / moveDuration;

        if (t >= 1.0)
        {
            if (useLocalSpace) transform.localPosition = endPos;
            else transform.position = endPos;

            moving = false;
            return;
        }

        var p = Vector3.LerpUnclamped(startPos, endPos, (float)t);
        if (useLocalSpace) transform.localPosition = p;
        else transform.position = p;
    }
}