//방해요소 하나의 공통 정보(종류). SabotageData.json에 대응. 종류별 설정은 같은 Id로 상세표에 있다
using System;

[Serializable]
public class SabotageData : GameDataBase
{
    public string TypeName; //표에 적은 종류 이름. 로드할 때 Type으로 바꾼다

    public SabotageType Type { get; private set; } //JsonUtility는 프로퍼티를 못 읽으므로 TryParseTypeName으로 채운다

    //TypeName을 SabotageType으로 바꿔 Type에 넣는다. 없는 이름이거나 None이면 false
    public bool TryParseTypeName()
    {
        if (Enum.TryParse(TypeName, out SabotageType parsedType) == false)
        {
            return false;
        }

        //"5"같은 숫자 글자도 TryParse 는 성공시키므로 정의된 값인지 한 번 더 확인
        if (Enum.IsDefined(typeof(SabotageType), parsedType) == false || parsedType == SabotageType.None)
        {
            return false;
        }

        Type = parsedType;

        return true;
    }
}
