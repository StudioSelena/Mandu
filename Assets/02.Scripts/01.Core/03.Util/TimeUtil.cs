//시간 단위 변환 상수와, 시간을 화면 표시용 문자열로 바꾸는 함수 모음
using System;
using System.Globalization;

public static class TimeUtil
{
    //초를 밀리초로 바꿀 때 곱하는 값
    public const int MillisecondsPerSecond = 1000;

    //화면에 초를 보여줄 때 형식 (예: 10.00)
    private const string SecondsDisplayFormat = "0.00";

    //입력이 없을 때 초 자리에 보여줄 글자
    private const string NoInputSecondsDisplayText = "-.--";

    //오차 뒤에 붙이는 단위와, 입력이 없어 오차가 없을 때 보여줄 글자
    private const string TimingErrorUnitText = "MS";
    private const string NoInputTimingErrorText = "-";

    //날짜와 시각 표시 형식. 분까지 표시. 언어와 상관없이 숫자로 2줄로 보여준다 (예: 2026.11.01 / 18:59)
    private const string DateTimeDisplayFormat = "yyyy.MM.dd\nHH:mm";

    //밀리초를 화면 표시용 초 문자열로 돌려준다. 지역 설정과 상관없이 소수점은 마침표로 표시한다
    public static string GetSecondsDisplayText(int timeMs)
    {
        double timeSeconds = (double)timeMs / MillisecondsPerSecond;
        return timeSeconds.ToString(SecondsDisplayFormat, CultureInfo.InvariantCulture);
    }

    //입력 시간(ms)을 화면 표시용 문자열로 돌려준다. 입력이 없으면 -.--
    public static string GetInputTimeDisplayText(int inputTimeMs)
    {
        if (inputTimeMs == StageManager.NoInputTimeMs)
        {
            return NoInputSecondsDisplayText;
        }

        return GetSecondsDisplayText(inputTimeMs);
    }

    //오차(ms)를 화면 표시용 문자열로 돌려준다. 입력이 없으면 -
    public static string GetTimingErrorDisplayText(int timingErrorMs)
    {
        if (timingErrorMs == StageManager.NoInputTimingErrorMs)
        {
            return NoInputTimingErrorText;
        }

        return timingErrorMs.ToString(CultureInfo.InvariantCulture) + TimingErrorUnitText;
    }

    //UTC로 저장한 Ticks를 플레이어PC의 현지 시각으로 바꿔 날짜 문자열로 돌려준다
    public static string GetDateTimeDisplayText(long utcTicks)
    {
        DateTime localDateTime = new DateTime(utcTicks, DateTimeKind.Utc).ToLocalTime();
        return localDateTime.ToString(DateTimeDisplayFormat, CultureInfo.InvariantCulture);
    }
}
