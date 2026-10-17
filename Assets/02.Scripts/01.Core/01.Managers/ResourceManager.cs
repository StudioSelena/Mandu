//어드레서블 에셋을 주소로 불러오고, 빌려간 수를 세어 마지막 반납 때 메모리에서 해제하는 매니저
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class ResourceManager : SingletonBase<ResourceManager>
{
    //주소별 번호표(핸들). 로드를 신청하는 순간 기록한다
    private Dictionary<string, AsyncOperationHandle> _handles = new Dictionary<string, AsyncOperationHandle>();

    //주소별 빌려간 수. 0이 되면 번호표를 반납한다
    private Dictionary<string, int> _referenceCounts = new Dictionary<string, int>();

    //Sprite[]처럼 배열도 받을 수 있게 T의 제약을 class로 둔다
    public async UniTask<T> LoadAsset<T>(string address) where T : class
    {
        if (_handles.TryGetValue(address, out AsyncOperationHandle handle) == false)
        {
            handle = Addressables.LoadAssetAsync<T>(address);
            _handles.Add(address, handle);
            _referenceCounts.Add(address, 0);
        }

        _referenceCounts[address]++;

#if UNITY_EDITOR
        Debug.Log($"[ResourceManager] 로드 {address}, 빌려간 수 {_referenceCounts[address]}");
#endif

        try
        {
            await handle.ToUniTask();
        }
        catch (System.Exception e)
        {
#if UNITY_EDITOR
            Debug.LogError($"[ResourceManager] 에셋 로드 실패. {address} / {e.Message}");
#endif

            //기다리는 사이 이미 해제된 번호표라면 반납할 것이 없다
            if (handle.IsValid() == true)
            {
                Release(address);
            }

            return null;
        }

        //기다리는 사이 부른 쪽이 먼저 반납해 해제된 경우
        if (handle.IsValid() == false)
        {
            return null;
        }

        return handle.Result as T;
    }

    public void Release(string address)
    {
        if (_referenceCounts.TryGetValue(address, out int referenceCount) == false)
        {
#if UNITY_EDITOR
            Debug.LogWarning($"[ResourceManager] 빌려간 적 없는 주소를 반납하려 합니다. {address}");
#endif
            return;
        }

        referenceCount--;

#if UNITY_EDITOR
        Debug.Log($"[ResourceManager] 반납 {address}, 빌려간 수 {referenceCount}");
#endif

        if (referenceCount > 0)
        {
            _referenceCounts[address] = referenceCount;
            return;
        }

        Addressables.Release(_handles[address]);
        _handles.Remove(address);
        _referenceCounts.Remove(address);
    }
}
