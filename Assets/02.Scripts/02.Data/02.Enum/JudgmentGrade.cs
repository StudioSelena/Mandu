//판정 등급. 좋을수록 큰 값이라 최고 기록 비교를 > 한 번으로 할 수 있다
public enum JudgmentGrade
{
    None,
    Fail,
    Good,
    Nice,
    Perfect,
    Monster,    //오차 0ms일 때만 받는 등급. 결과 화면에 MONSTER로 표시
}
