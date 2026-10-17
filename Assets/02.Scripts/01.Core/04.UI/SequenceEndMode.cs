//UISpriteImageSequencer가 마지막 그림까지 재생한 뒤 할 일

public enum SequenceEndMode
{
    None,   //정하지 않음. 마지막 그림에서 멈춤과 같게 처리한다
    Loop,
    HoldLastFrame,
    ReturnToFirstFrame,
}