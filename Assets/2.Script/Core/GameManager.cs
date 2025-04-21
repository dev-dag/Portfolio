using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.U2D;

public class GameManager : SingleTon<GameManager>
{
    public const string INTERACTABLE_OBJECT_LAYER_NAME = "InteractableObject";
    public const string PLAYER_SIDE_LAYER_NAME = "PlayerSide";
    public const string MONSTER_SIDE_LAYER_NAME = "MonsterSide";
    public const string PLATFORM_LAYER_NAME = "Platform";

    [Space(20f)]
    public InputActionAsset globalInputActionAsset;
    public QuestSystem questSystem;
    public CombatSystem combatSystem;

    [Space(20f)]
    public DataContainer data;
    [HideInInspector] public UI_Manager uiManager;

    [Space(20f), Header("Level Prefabs")]
    [SerializeField] private GameObject UI_Prefab;
    [SerializeField] private GameObject baseHomeLevelPrefab;
    [SerializeField] private GameObject dungeonLevelPrefab;

    private SpriteAtlas itemIconAtlas;

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

    protected override void Awake()
    {
        base.Awake();

        DontDestroyOnLoad(this.gameObject);

        Init();
    }

    private async void Init()
    {
        DB_Connecter dbConnecter = new DB_Connecter();

        await LoadItemIcon();

        data = dbConnecter.ConnectAndLoadDB();

        globalInputActionAsset.Enable(); // 인풋 활성화

        questSystem.Init(); // 퀘스트 시스템 초기화

        uiManager = GameObject.Instantiate(UI_Prefab).GetComponent<UI_Manager>(); // UI 인스턴스 생성 및 초기화
        uiManager.Init();

        GameObject.Instantiate(baseHomeLevelPrefab); // 초기 맵 로드
    }

    private async Awaitable LoadItemIcon()
    {
        var handle = Addressables.LoadAssetAsync<SpriteAtlas>("Sprite Atlas/Item Icon");
        await handle.Task;
        
        if (handle.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
        {
            itemIconAtlas = handle.Result;
        }
    }
}
