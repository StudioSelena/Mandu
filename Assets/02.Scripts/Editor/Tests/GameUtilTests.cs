//GameUtil 판정 함수의 경계값과 오차 버림 규칙을 확인하는 EditMode 테스트
using NUnit.Framework;

public class GameUtilTests
{
    //테스트 전용 목표 시간과 판정폭(ms). 실제 스테이지 데이터와 상관없이 규칙만 확인한다
    private const int TestTargetTimeMs = 10000;
    private const int TestPerfectWindowMs = 50;
    private const int TestNiceWindowMs = 100;
    private const int TestGoodWindowMs = 200;

    //테스트 전용 판정 데이터를 만든다
    private TargetTimeData CreateTestTargetTimeData()
    {
        TargetTimeData targetTimeData = new TargetTimeData();
        targetTimeData.TargetTimeMs = TestTargetTimeMs;
        targetTimeData.PerfectWindowMs = TestPerfectWindowMs;
        targetTimeData.NiceWindowMs = TestNiceWindowMs;
        targetTimeData.GoodWindowMs = TestGoodWindowMs;
        return targetTimeData;
    }

    //오차0만 Monster, 경계값은 상위 등급, 경계값 +1은 다음 등급
    [TestCase(0, JudgmentGrade.Monster)]
    [TestCase(1, JudgmentGrade.Perfect)]
    [TestCase(TestPerfectWindowMs, JudgmentGrade.Perfect)]
    [TestCase(TestPerfectWindowMs + 1, JudgmentGrade.Nice)]
    [TestCase(TestNiceWindowMs, JudgmentGrade.Nice)]
    [TestCase(TestNiceWindowMs + 1, JudgmentGrade.Good)]
    [TestCase(TestGoodWindowMs, JudgmentGrade.Good)]
    [TestCase(TestGoodWindowMs + 1, JudgmentGrade.Fail)]
    public void JudgeTimingGradeAtBoundary(int timingErrorMs, JudgmentGrade expectedGrade)
    {
        TargetTimeData targetTimeData = CreateTestTargetTimeData();

        JudgmentGrade actualGrade = GameUtil.JudgeTimingGrade(timingErrorMs, targetTimeData);

        Assert.AreEqual(expectedGrade, actualGrade);
    }

    //판정 데이터가 없으면 None
    [Test]
    public void JudgeTimingGradeWithNullData()
    {
        JudgmentGrade actualGrade = GameUtil.JudgeTimingGrade(0, null);

        Assert.AreEqual(JudgmentGrade.None, actualGrade);
    }

    //목표 10초 기준. 오차는 ms로 버림한다. 1ms 미만은 0(Monster), 50.9ms는 50(Perfect). 일찍 누른 쪽도 같다
    [TestCase(10.0, 0)]
    [TestCase(10.0009, 0)]
    [TestCase(9.9991, 0)]
    [TestCase(10.0509, TestPerfectWindowMs)]
    [TestCase(9.9491, TestPerfectWindowMs)]
    public void CalculateTimingErrorMsWithTruncation(double elapsedSeconds, int expectedErrorMs)
    {
        int actualErrorMs = GameUtil.CalculateTimingErrorMs(elapsedSeconds, TestTargetTimeMs);

        Assert.AreEqual(expectedErrorMs, actualErrorMs);
    }
}
