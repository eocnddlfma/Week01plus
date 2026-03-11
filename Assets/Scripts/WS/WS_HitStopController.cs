using System.Collections;
using UnityEngine;

public class WS_HitStopController : MonoBehaviour
{
    [SerializeField] private float _defaultDuration = 0.035f;
    [SerializeField] private float _minInterval = 0.08f;

    private Coroutine _routine;
    private float _originalFixedDeltaTime;
    private float _lastPlayUnscaledTime = -999f;

    private void Awake()
    {
        _originalFixedDeltaTime = Time.fixedDeltaTime;
    }

    public bool TryPlay(float duration = -1f)
    {
        if (Time.unscaledTime - _lastPlayUnscaledTime < _minInterval)
            return false;

        if (duration <= 0f)
            duration = _defaultDuration;

        _lastPlayUnscaledTime = Time.unscaledTime;

        if (_routine != null)
            StopCoroutine(_routine);

        _routine = StartCoroutine(HitStopRoutine(duration));
        return true;
    }

    private IEnumerator HitStopRoutine(float duration)
    {
        Time.timeScale = 0f;
        Time.fixedDeltaTime = 0f;

        yield return new WaitForSecondsRealtime(duration);

        Time.timeScale = 1f;
        Time.fixedDeltaTime = _originalFixedDeltaTime;
        _routine = null;
    }

    private void OnDisable()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = _originalFixedDeltaTime;
    }

    private void OnDestroy()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = _originalFixedDeltaTime;
    }
}