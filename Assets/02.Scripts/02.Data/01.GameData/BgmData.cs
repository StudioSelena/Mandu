//배경음 곡 하나. BgmData.json에 대응. 어느 목록에 들어가는지와 어드레서블 주소를 가진다
using System;

[Serializable]
public class BgmData : GameDataBase
{
    public string PlaylistTypeName; //표에 적은 목록 이름. 로드할 때 PlaylistType으로 바꾼다
    public string Address;

    public BgmPlaylistType PlaylistType { get; private set; }   //TryParsePlaylistTypeName으로 채운다

    //PlaylistTypeName을 BgmPlaylistType으로 바꿔 PlaylistType에 넣는다. 없는 이름이거나 None이면 false
    public bool TryParsePlaylistTypeName()
    {
        if (Enum.TryParse(PlaylistTypeName, out BgmPlaylistType parsedType) == false)
        {
            return false;
        }

        //"5"같은 숫자 글자도 TryParse는 성공시키므로 정의된 값인지 한번 더 확인
        if (Enum.IsDefined(typeof(BgmPlaylistType), parsedType) == false || parsedType == BgmPlaylistType.None)
        {
            return false;
        }

        PlaylistType = parsedType;

        return true;
    }
}