using System;
using UnityEngine;

public sealed class LightningFx : MonoBehaviour
{
    [Header("번개")]
    [SerializeField, Tooltip("떨어지는 속도")] private float fallSpeed = 18f;
    [SerializeField, Tooltip("자동 회수 시간(초)")] private float maxLifeTime = 0.6f;

    private Vector3 targetPos;
    private float lifeTimer;
    private Action<LightningFx> returnToPool;

    public void Play(Vector3 from, Vector3 to, Action<LightningFx> onReturn)
    {
        transform.position = from;
        targetPos = to;
        lifeTimer = 0f;
        returnToPool = onReturn;
        gameObject.SetActive(true);
    }

    private void Update()
    {
        lifeTimer += Time.deltaTime;

        var pos = transform.position;
        transform.position = Vector3.MoveTowards(pos, targetPos, fallSpeed * Time.deltaTime);

        if (lifeTimer >= maxLifeTime || Vector3.SqrMagnitude(transform.position - targetPos) <= 0.001f)
            returnToPool?.Invoke(this);
    }
}