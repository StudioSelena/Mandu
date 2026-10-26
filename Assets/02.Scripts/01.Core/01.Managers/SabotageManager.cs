//스테이지 상태에 맞춰 방해요소를 열고, 시작하고, 멈추고, 닫는 매니저
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

public class SabotageManager : SingletonBase<SabotageManager>
{
    private const string DancingManduPoolAddress = "DancingManduPool"; //춤추는 만두 풀 프리팹 어드레서블 주소

    private string _currentSabotageId; //다시하기로 같은 방해요소인지 비교하고, 닫을 때 종류를 조회한다
    private ISabotage _currentSabotage;
    private int _currentInstanceId = GameObjectManager.InvalidInstanceId; //월드 방해요소를 닫을 때 쓰는 번호. 만들때만 생기는 값이라 받아 적어 둠
    private CancellationTokenSource _prepareCancelTokenSource; //생성을 기다리는 준비가 있을 때 새 준비가 오면 이전 것을 취소하는 데 쓴다

    private void Start()
    {
        if (Instance != this)
        {
            return;
        }

        StageManager.Instance.StageStateChangedEvent += HandleStageStateChanged;
    }

    private void OnDestroy()
    {
        if (Instance != this)
        {
            return;
        }

        CancelPrepareToken();

        if (StageManager.Instance == null)
        {
            return;
        }

        StageManager.Instance.StageStateChangedEvent -= HandleStageStateChanged;
    }

    private void HandleStageStateChanged(StageState stageState)
    {
        switch (stageState)
        {
            case StageState.Ready:
                //삶는 도중 스테이지가 바뀌면 멈추지 않은 방해요소가 남아 있으므로 준비 전에 먼저 멈춘다
                StopCurrentSabotage();
                PrepareCurrentSabotage().Forget();
                break;
            
            case StageState.Boiling:
                StartCurrentSabotage();
                break;

            case StageState.JudgmentComplete:
                StopCurrentSabotage();
                break;
        }
    }

    //새 스테이지의 방해요소를 준비한다. 같은 종류면 재사용하고, 다르면 이전 것을 닫고 새로 연다
    private async UniTaskVoid PrepareCurrentSabotage()
    {
        string newSabotageId = StageManager.Instance.CurrentSabotageId;

        //생성을 기다리는 이전 준비가 있으면 취소한다. 토큰은 지역 변수로 받아 await 뒤에도 내 것을 확인한다
        CancellationToken cancelToken = RenewPrepareCancelToken();

        //1.다시하기처럼 같은 방해요소면 다시 준비만 한다
        if (_currentSabotage != null && newSabotageId == _currentSabotageId)
        {
            _currentSabotage.PrepareSabotage(newSabotageId);
            return;
        }

        //2.새 Id가 비었는지 보기 전에 이전것부터 닫는다. 순서가 바뀌면 이전 방해요소가 화면에 남는다
        CloseCurrentSabotage();
        
        //3. 방해요소 없는 스테이지
        if (string.IsNullOrEmpty(newSabotageId))
        {
            return;
        }

        //4.새로 열고 준비한다
        ISabotage openedSabotage = await OpenSabotage(newSabotageId, cancelToken);
        if (openedSabotage == null)
        {
            return;
        }

        _currentSabotageId = newSabotageId;
        _currentSabotage = openedSabotage;
        _currentSabotage.PrepareSabotage(newSabotageId);
    }

    //종류에 맞는 곳에서 방해요소를 연다. 방해요소 종류가 늘면 여기와 CloseCurrentSabotage에 case를 더한다
    private async UniTask<ISabotage> OpenSabotage(string sabotageId, CancellationToken cancelToken)
    {
        SabotageData sabotageData = GameDataManager.Instance.GetSabotageData(sabotageId);
        if (sabotageData == null)
        {
            return null;
        }

        ISabotage openedSabotage = null;

        switch (sabotageData.Type)
        {
            case SabotageType.FakeNumber:
                UIBase openedUI = UIManager.Instance.OpenUI(UIRootType.Content, UIType.FakeNumberSabotageUI, false);
                openedSabotage = openedUI as ISabotage;
                break;

            case SabotageType.DancingMandu:
                openedSabotage = await OpenWorldSabotage(DancingManduPoolAddress, cancelToken);
                break;
        }

        //취소된 준비는 일부러 null을 돌려주므로 에러가 아니다
        if (openedSabotage == null && !cancelToken.IsCancellationRequested)
        {
#if UNITY_EDITOR
            Debug.LogError($"[SabotageManager] {sabotageId}를 열지 못했습니다. 종류 {sabotageData.Type}");
#endif
        }

        return openedSabotage;
    }

    //월드 방해요소를 만들고 번호를 받아 적는다. 기다리는 사이 새 준비가 시작됐으면 방금 만든 것을 업애고 null
    private async UniTask<ISabotage> OpenWorldSabotage(string address, CancellationToken cancelToken)
    {
        int instanceId = await GameObjectManager.Instance.RequestSpawnWorldObject(address);
        if (instanceId == GameObjectManager.InvalidInstanceId)
        {
            return null;
        }

        //await만 기다렸을 뿐 생성은 끝까지 진행됐다. 낡은 준비가 만든 것은 아무도 번호를 모르니 여기서 직접 없앤다.
        if (cancelToken.IsCancellationRequested)
        {
            GameObjectManager.Instance.RequestDestroyWorldObject(instanceId);
            return null;
        }

        GameObject spawnedObject = GameObjectManager.Instance.GetWorldObject(instanceId);
        ISabotage openedSabotage = spawnedObject.GetComponent<ISabotage>();
        if (openedSabotage == null)
        {
            GameObjectManager.Instance.RequestDestroyWorldObject(instanceId);
            return null;
        }

        _currentInstanceId = instanceId;
        return openedSabotage;
    }

    //지금 열린 방해요소를 종류에 맞는 곳에서 닫고 기억을 비운다
    private void CloseCurrentSabotage()
    {
        if (_currentSabotage == null)
        {
            return;
        }

        SabotageData sabotageData = GameDataManager.Instance.GetSabotageData(_currentSabotageId);
        if (sabotageData != null)
        {
            switch (sabotageData.Type)
            {
                case SabotageType.FakeNumber:
                    UIManager.Instance.CloseUI(UIType.FakeNumberSabotageUI);
                    break;

                case SabotageType.DancingMandu:
                    GameObjectManager.Instance.RequestDestroyWorldObject(_currentInstanceId);
                    break;
            }
        }

        _currentSabotageId = null;
        _currentSabotage = null;
        _currentInstanceId = GameObjectManager.InvalidInstanceId;
    }

    private void StartCurrentSabotage()
    {
        if (_currentSabotage == null)
        {
            return;
        }

        _currentSabotage.StartSabotage();
    }

    private void StopCurrentSabotage()
    {
        if (_currentSabotage == null)
        {
            return;
        }

        _currentSabotage.StopSabotage();
    }

    //이전 준비 토큰을 취소하고 새 토큰을 만들어 돌려준다
    private CancellationToken RenewPrepareCancelToken()
    {
        CancelPrepareToken();
        _prepareCancelTokenSource = new CancellationTokenSource();
        return _prepareCancelTokenSource.Token;
    }

    private void CancelPrepareToken()
    {
        if (_prepareCancelTokenSource == null)
        {
            return;
        }

        _prepareCancelTokenSource.Cancel();
        _prepareCancelTokenSource.Dispose();
        _prepareCancelTokenSource = null;
    }
}