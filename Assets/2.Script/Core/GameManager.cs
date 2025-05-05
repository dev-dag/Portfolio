using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.U2D;

public class GameManager : SingleTon<GameManager>
{
    public const float FOLLOW_CAM_ORIGIN_LENS = 8f; // 카메라 기본 렌즈 값
    public const string INTERACTABLE_OBJECT_LAYER_NAME = "InteractableObject";
    public const string PLAYER_LAYER_NAME = "Player";
    public const string MONSTER_LAYER_NAME = "Monster";
    public const string PLATFORM_LAYER_NAME = "Platform";
    public const string PLAYER_EXCLUSIVE_LAYER_NAME = "PlayerExclusive";
    public const string MONSTER_EXCLUSIVE_LAYER_NAME = "MonsterExclusive";

    public DataContainer ReferenceData { get; private set; }
    public InstanceData InstanceData { get; private set; }

    [Space(20f)]
    public InputActionAsset globalInputActionAsset;
    public QuestSystem questSystem;
    public CombatSystem combatSystem;

    [Space(20f)]
    [HideInInspector] public UI_Manager uiManager;

    [Space(20f), Header("Level Prefabs")]
    [SerializeField] private GameObject Player_Prefab;
    [SerializeField] private GameObject UI_Prefab;
    [SerializeField] private GameObject baseHomeLevelPrefab;
    [SerializeField] private GameObject dungeonLevelPrefab;

    private SpriteAtlas itemIconAtlas;

    private Dictionary<int, ItemInfoData> itemInfoDataCache = new Dictionary<int, ItemInfoData>();
    private Awaitable cachingAwaiter;

    /// <summary>
    /// 맵 변경 함수
    /// </summary>
    /// <param name="ID">맵 ID</param>
    public void ChangeMap(int ID)
    {
        switch (ID)
        {
            case 0:
            {
                Destroy(LevelControl.Current.gameObject);
                GameObject.Instantiate(baseHomeLevelPrefab);
                break;
            }
            case 1:
            {
                Destroy(LevelControl.Current.gameObject);
                GameObject.Instantiate(dungeonLevelPrefab);
                break;
            }
        }
    }

    /// <summary>
    /// 아이콘 이미지를 반환하는 함수
    /// </summary>
    public Sprite GetIconSprite(int id)
    {
        return itemIconAtlas.GetSprite(id.ToString());
    }

    public async Awaitable MakeCache()
    {
        // 아이템 인포 캐시
        Task itemInfoTask = null;
        {
            itemInfoDataCache.Clear();

            var handle = Addressables.LoadAssetsAsync<ItemInfoData>("Item Info Data");
            itemInfoTask = handle.Task;

            handle.Completed += (result) =>
            {
                if (result.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
                {
                    foreach (ItemInfoData infoData in result.Result)
                    {
                        itemInfoDataCache.Add(infoData.id, infoData);
                    }
                }
            };
        }

        // 아이콘 스프라이트 아틀라스 캐시
        Task iconSpriteAtlasTask = null;
        {
            var handle = Addressables.LoadAssetAsync<SpriteAtlas>("Sprite Atlas/Item Icon");
            iconSpriteAtlasTask = handle.Task;

            handle.Completed += (result) =>
            {
                if (handle.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
                {
                    itemIconAtlas = handle.Result;
                }
            };
        }

        await itemInfoTask;
        await iconSpriteAtlasTask;
    }

    public T LoadItemInfo<T>(int itemID) where T : ItemInfoData
    {
        if (itemInfoDataCache.ContainsKey(itemID))
        {
            return (T)itemInfoDataCache[itemID];
        }
        else
        {
            EDebug.LogError("데이터를 찾지 못함.");
            return null;
        }
    }

    public async void StartGame()
    {
        await cachingAwaiter;

        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(1, LoadSceneMode.Additive);
        var task = new TaskCompletionSource<object>();
        asyncOperation.completed += _ => task.SetResult(null);

        EventSystem.current.enabled = false; // 현재 씬 이벤트 시스템 비활성화

        uiManager.FadeOut(1f, async () =>
        {
            await SceneManager.UnloadSceneAsync(0); // 이전 씬 언로드

            await task.Task; // 게임 레퍼런스 데이터 캐싱이 아직 안된 경우 대기

            questSystem.Init(); // 퀘스트 시스템 초기화
            uiManager.Init();
            Player.Current.Init();

            GameObject.Instantiate(baseHomeLevelPrefab); // 초기 맵 로드

            globalInputActionAsset.Enable(); // 인풋 활성화

            uiManager.FadeIn(1f);
        });
    }

    protected override void Awake()
    {
        base.Awake();

        DontDestroyOnLoad(this.gameObject);

        Init();
    }

    private async void Init()
    {
        DB_Connecter dbConnecter = new DB_Connecter();

        cachingAwaiter = MakeCache();

        ReferenceData = dbConnecter.ConnectAndLoadDB();

        globalInputActionAsset.Enable(); // 인풋 활성화

                InstanceData = new InstanceData(new Dictionary<int, int>()
        {
            { 0, 1 },
            { 1, 1 },
            { 2, 1 },
            { 3, 3 }
        }
        , 1, 3, -1, -1);

        uiManager = GameObject.Instantiate(UI_Prefab).GetComponent<UI_Manager>(); // UI 인스턴스 생성 및 초기화
        DontDestroyOnLoad(uiManager.gameObject); // UI 오브젝트 파괴 방지

        GameObject.Instantiate(Player_Prefab); // 플레이어 오브젝트 생성
    }
}
