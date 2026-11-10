//효과음/배경음을 재생하는 매니저. 효과음 클립은 쓰는 쪽이 빌려와서 넘기고 반납도 쓰는 쪽이 한다(타이밍 게임의 특성 상). 배경음은 목록에서 골라 직접 빌리고 반납한다
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class SoundManager : SingletonBase<SoundManager>
{
    private const int NoBgmIndex = -1;  //아직 튼 곡이 없을 때

    [SerializeField] private AudioSource AudioSource_SFX;
    [SerializeField] private AudioSource AudioSource_BGM;

    private BgmPlaylistType _currentPlaylistType = BgmPlaylistType.None;
    private int _lastBgmIndex = NoBgmIndex;     //직전에 튼 곡의 목록 번호. 같은 곡이 연달아 나오지 않게 한다
    private string _currentBgmAddress;          //지금 빌려둔 곡. 다음 곡을 튼 뒤에 반납한다
    private bool _isLoadingBgm;                 //곡을 불러오는 동안 Update가 다음 곡을 또 고르지 않게 한다

    //틀고 있어야 하는데 소리가 멈췄으면 곡이 끝난 것이므로 다음 곡을 고른다
    private void Update()
    {
        if (_currentPlaylistType == BgmPlaylistType.None || _isLoadingBgm)
        {
            return;
        }

        if (AudioSource_BGM.isPlaying == false)
        {
            PlayNextBgm().Forget();
        }
    }

    //직전 곡을 뺀 나머지에서 무작위로 고른다. 곡이 하나뿐이거나 처음 고르는 것이면 전체에서 고른다
    private int PickNextBgmIndex(int bgmCount)
    {
        if (bgmCount == 1 || _lastBgmIndex == NoBgmIndex)
        {
            return Random.Range(0, bgmCount);
        }

        int nextIndex = Random.Range(0, bgmCount - 1); //직전 곡 한 자리를 뺀 개수에서 고른다
        if (nextIndex >= _lastBgmIndex)
        {
            nextIndex++; //직전 곡 자리부터 한 칸씩 밀어 직전 곡을 건너뛴다
        }

        return nextIndex;
    }

    //다음 곡을 빌려 튼 뒤 이전 곡을 반납한다
    private async UniTaskVoid PlayNextBgm()
    {
        IReadOnlyList<string> addressList = GameDataManager.Instance.GetBgmAddressList(_currentPlaylistType);
        if (addressList == null || addressList.Count == 0)
        {
            _currentPlaylistType = BgmPlaylistType.None; //곡이 없는 목록이면 매 프레임 다시 고르지 않게 멈춘다
            return;
        }

        BgmPlaylistType requestedPlaylistType = _currentPlaylistType;
        int nextIndex = PickNextBgmIndex(addressList.Count);
        string nextAddress = addressList[nextIndex];

        _isLoadingBgm = true;
        AudioClip clip = await ResourceManager.Instance.LoadAsset<AudioClip>(nextAddress);
        _isLoadingBgm = false;

        //불러오는 동안 목록이 바뀌었으면 이 곡은 틀지 않고 반납한다. 새 목록의 곡은 Update가 다시 고른다
        if (requestedPlaylistType != _currentPlaylistType)
        {
            ResourceManager.Instance.Release(nextAddress);
            return;
        }

        if (clip == null)
        {
            ResourceManager.Instance.Release(nextAddress);
            _currentPlaylistType = BgmPlaylistType.None; //불러오기에 실패하면 매 프레임 다시 시도하지 않게 멈춘다
            return;
        }

        AudioSource_BGM.clip = clip;
        AudioSource_BGM.Play();

        if (string.IsNullOrEmpty(_currentBgmAddress) == false)
        {
            ResourceManager.Instance.Release(_currentBgmAddress);
        }

        _currentBgmAddress = nextAddress;
        _lastBgmIndex = nextIndex;
    }

    //효과음을 겹쳐서 재생한다. 같은 소리가 빠르게 이어져도 앞소리를 끊지 않는다
    public void PlaySFX(AudioClip clip)
    {
        if (clip == null)
        {
            return;
        }

        AudioSource_SFX.PlayOneShot(clip);
    }

    //메뉴와 플레이 구간이 바뀔 때 게임매니저가 부른다. 같은 목록을 이미 틀고 있으면 그대로 둔다
    public void PlayBgmPlaylist(BgmPlaylistType playlistType)
    {
        if (playlistType == _currentPlaylistType)
        {
            return;
        }

        _currentPlaylistType = playlistType;
        _lastBgmIndex = NoBgmIndex; //목록이 바뀌면 이전 목록의 번호는 의미가 없다
        AudioSource_BGM.Stop();     //멈추면 Update가 새 목록에서 곡을 고른다
    }
}
