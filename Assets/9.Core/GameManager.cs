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
    public AudioSystem audioSystem;

    [Space(20f)]
    [HideInInspector] public GameUI gameUI;
    [HideInInspector] public GlobalUI globalUI;

    [Space(20f), Header("Level Prefabs")]
    [SerializeField] private GameObject player_Prefab;
    [SerializeField] private GameObject gameUI_Prefab;
    [SerializeField] private GameObject globalUI_Prefab;
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

        Player.Current.Init();
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

        globalUI.Fade.FadeOut(1f, async () =>
        {
            await SceneManager.UnloadSceneAsync(0); // 이전 씬 언로드

            await task.Task; // Play 씬이 로드 안된 경우 대기

            if (gameUI == null)
            {
                gameUI = GameObject.Instantiate(gameUI_Prefab).GetComponent<GameUI>(); // 게임 UI 생성
            }
            gameUI.Init();

            questSystem.Init(); // 퀘스트 시스템 초기화
            Player.Current.Init();

            GameObject.Instantiate(baseHomeLevelPrefab); // 초기 맵 로드

            globalInputActionAsset.Enable(); // 인풋 활성화

            globalUI.Fade.FadeIn(1f);
        });
    }

    protected override void Awake()
    {
        base.Awake();

        DontDestroyOnLoad(this.gameObject);

        Init();
    }

    private void Init()
    {
        DB_Connecter dbConnecter = new DB_Connecter();
        ReferenceData = dbConnecter.ConnectAndLoadDB();
        cachingAwaiter = MakeCache();

        globalInputActionAsset.Enable(); // 인풋 활성화

        InstanceData = new InstanceData(new Dictionary<int, int>()
        {
            { 0, 1 },
            { 1, 1 },
            { 2, 1 },
            { 3, 3 }
        }
        , 1, 3, -1, -1);

        globalUI = GameObject.Instantiate(globalUI_Prefab).GetComponent<GlobalUI>(); // 글로벌 UI 생성
        DontDestroyOnLoad(globalUI.gameObject); // UI 오브젝트 파괴 방지

        GameObject.Instantiate(player_Prefab); // 플레이어 오브젝트 생성

        audioSystem.Init(); // 오디오 시스템 초기화

        Addressables.LoadAssetAsync<AudioClip>("02 Through The Lands - Atmospheres Part I").Completed += (result) =>
        {
            if (result.Status != UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
            {
                return;
            }

            if (SceneManager.GetActiveScene().buildIndex == 0)
            {
                audioSystem.GetBGM_Player().Play(result.Result);
            }
        };
    }
}
