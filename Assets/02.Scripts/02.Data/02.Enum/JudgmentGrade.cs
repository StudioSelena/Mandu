//판정 등급. 좋을수록 큰 값이라 최고 기록 비교를 > 한 번으로 할 수 있다
public enum JudgmentGrade
{
    None,
    Fail,
    Good,
    Nice,
    Perfect,
    Monster, //숨겨진 등급. 오차 0ms. 화면에는 Perfect로 표시
}
