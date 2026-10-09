// 프로토타입 전용. 숨은 타이머로 10초를 맞추는 손맛과 가짜 숫자 방해요소의 효과를 검증한다
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class ProtoTimingTester : MonoBehaviour
{
    private enum ProtoState
    {
        None,
        Ready,
        Boiling,
        Result,
    }

    //판정 기준은 프로토타입 전용이다. 본 게임은 스테이지 데이터를 따른다
    private const double TARGET_SECONDS = 10.0;
    private const double PERFECT_ERROR_SECONDS = 0.1;
    private const double NICE_ERROR_SECONDS = 0.3;
    private const double GOOD_ERROR_SECONDS = 0.6;
    private const double AUTO_FAIL_GRACE_SECONDS = 5.0;

    private const float FAKE_UPDATE_INTERVAL_SECONDS = 0.15f;
    private const float FAKE_MAX_SECONDS = 15f;

    [SerializeField] private TextMeshPro Text_Guide;
    [SerializeField] private TextMeshPro Text_Result;
    [SerializeField] private TextMeshPro Text_Fake;

    private ProtoState _state;
    private double _boilStartTime;
    private bool _isFakeTimerOn;
    private float _nextFakeUpdateTime;
    private int _tryCount;

    private void Start()
    {
        EnterReadyState();
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return;
        }

        switch (_state)
        {
            case ProtoState.Ready:
                if (keyboard.digit1Key.wasPressedThisFrame)
                {
                    ToggleFakeTimer();
                }
                if (keyboard.spaceKey.wasPressedThisFrame)
                {
                    StartBoiling();
                }
                break;

            case ProtoState.Boiling:
                UpdateFakeTimer();
                if (keyboard.spaceKey.wasPressedThisFrame)
                {
                    PullDumpling();
                }
                else if (GetElapsedSeconds() > TARGET_SECONDS + GOOD_ERROR_SECONDS + AUTO_FAIL_GRACE_SECONDS)
                {
                    PullDumpling();
                }
                break;

            case ProtoState.Result:
                if (keyboard.rKey.wasPressedThisFrame)
                {
                    EnterReadyState();
                }
                break;
        }
    }

    private void EnterReadyState()
    {
        _state = ProtoState.Ready;
        Text_Guide.text = "Press SPACE to START (target: 10 seconds)";
        Text_Fake.text = "";
        RefreshFakeTimerStatusText();
    }

    private void StartBoiling()
    {
        _state = ProtoState.Boiling;
        _boilStartTime = Time.realtimeSinceStartupAsDouble;
        _nextFakeUpdateTime = 0f;
        Text_Guide.text = "Press SPACE to STOP";
        Text_Result.text = "";
    }

    private void PullDumpling()
    {
        double elapsedSeconds = GetElapsedSeconds();
        double errorSeconds = System.Math.Abs(elapsedSeconds - TARGET_SECONDS);

        _state = ProtoState.Result;
        _tryCount++;

        Text_Guide.text = "Press R to Retry";
        Text_Fake.text = "";
        Text_Result.text = $"Try {_tryCount} | {elapsedSeconds:0.000}s (error {errorSeconds:0.000}s)\n{GetGradeName(errorSeconds)}";
    }

    private double GetElapsedSeconds()
    {
        return Time.realtimeSinceStartupAsDouble - _boilStartTime;
    }

    private string GetGradeName(double errorSeconds)
    {
        if (errorSeconds <= PERFECT_ERROR_SECONDS)
        {
            return "PERFECT: 3 STARS";
        }
        if (errorSeconds <= NICE_ERROR_SECONDS)
        {
            return "NICE: 2 STARS";
        }
        if (errorSeconds <= GOOD_ERROR_SECONDS)
        {
            return "GOOD: 1 STAR";
        }
        return "FAIL";
    }

    private void ToggleFakeTimer()
    {
        _isFakeTimerOn = !_isFakeTimerOn;
        RefreshFakeTimerStatusText();
    }

    private void RefreshFakeTimerStatusText()
    {
        string status = _isFakeTimerOn ? "ON" : "OFF";
        Text_Result.text = $"[1] Fake Timer : {status}";
    }

    private void UpdateFakeTimer()
    {
        if (_isFakeTimerOn == false)
        {
            return;
        }
        if (Time.unscaledTime < _nextFakeUpdateTime)
        {
            return;
        }

        _nextFakeUpdateTime = Time.unscaledTime + FAKE_UPDATE_INTERVAL_SECONDS;
        Text_Fake.text = Random.Range(0f, FAKE_MAX_SECONDS).ToString("0.00");
    }
}
