//월드 오브젝트를 어드레서블 프리팹으로 만들고 번호를 붙여 보관하다가 요청이 오면 없애고 반납하는 매니저
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

public class GameObjectManager : SingletonBase<GameObjectManager>
{
    //생성에 실패했을 때 돌려주는 번호. 카운터를 1부터 올리므로 실제 오브젝트에는 쓰이지 않는다
    public const int InvalidInstanceId = 0;

    //만든 오브젝트와 반납에 쓸 주소를 한번에 넣고 한번에 지우기 위한 묶음
    private readonly struct SpawnedObjectInfo
    {
        public GameObject SpawnedObject { get; }
        public string Address { get; }

        public SpawnedObjectInfo(GameObject spawnedObject, string address)
        {
            SpawnedObject = spawnedObject;
            Address = address;
        }
    }

    //마지막으로 발급한 번호. 지운 번호는 다시 쓰지 않는다
    private int _lastInstanceId = InvalidInstanceId;

    //번호별로 만든 오브젝트 정보
    private Dictionary<int, SpawnedObjectInfo> _spawnedObjectInfos = new Dictionary<int, SpawnedObjectInfo>();

    //주소의 프리팹을 빌려와 월드에 만들고 번호를 돌려준다. 실패하면 InvalidInstanceId
    public async UniTask<int> RequestSpawnWorldObject(string address)
    {
        GameObject prefab = await ResourceManager.Instance.LoadAsset<GameObject>(address);
        if (prefab == null)
        {
#if UNITY_EDITOR
            Debug.LogWarning($"[GameObjectManager] 프리팹을 불러오지 못해 만들지 않습니다. {address}");
#endif
            return InvalidInstanceId;
        }

        GameObject spawnedObject = Instantiate(prefab);

        _lastInstanceId++;
        int instanceId = _lastInstanceId;

        //하이어라키에서 어떤 번호인지 알아보기 위한 이름표
        spawnedObject.name = $"{prefab.name}(Id_{instanceId})";

        _spawnedObjectInfos.Add(instanceId, new SpawnedObjectInfo(spawnedObject, address));

        return instanceId;
    }

    //번호의 오브젝트를 없애고 빌려온 프리팹을 반납한다. 없는 번호면 경고만 남긴다
    public void RequestDestroyWorldObject(int instanceId)
    {
        if (_spawnedObjectInfos.TryGetValue(instanceId, out SpawnedObjectInfo spawnedObjectInfo) == false)
        {
#if UNITY_EDITOR
            Debug.LogWarning($"[GameObjectManager] 없는 번호라 없애지 않습니다. {instanceId}");
#endif
            return;
        }

        _spawnedObjectInfos.Remove(instanceId);
        Destroy(spawnedObjectInfo.SpawnedObject);
        ResourceManager.Instance.Release(spawnedObjectInfo.Address);
    }
}
