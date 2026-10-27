//효과음을 재생하는 매니저. 클립은 쓰는 쪽이 빌려와서 사운드매니저에 넘기고 반납도 쓰는 쪽이 한다(타이밍 게임의 특성 상)
using UnityEngine;

public class SoundManager : SingletonBase<SoundManager>
{
    [SerializeField] private AudioSource AudioSource_SFX;

    //효과음을 겹쳐서 재생한다. 같은 소리가 빠르게 이어져도 앞소리를 끊지 않는다
    public void PlaySFX(AudioClip clip)
    {
        if (clip == null)
        {
            return;
        }

        AudioSource_SFX.PlayOneShot(clip);
    }
}
