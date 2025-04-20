using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

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

    /// <summary>
    /// �� ���� �Լ�
    /// </summary>
    /// <param name="ID">�� ID</param>
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

    protected override void Awake()
    {
        base.Awake();

        DontDestroyOnLoad(this.gameObject);

        DB_Connecter dbConnecter = new DB_Connecter();

        data = dbConnecter.ConnectAndLoadDB();

        globalInputActionAsset.Enable(); // ��ǲ Ȱ��ȭ

        questSystem.Init(); // ����Ʈ �ý��� �ʱ�ȭ

        uiManager = GameObject.Instantiate(UI_Prefab).GetComponent<UI_Manager>(); // UI �ν��Ͻ� ���� �� �ʱ�ȭ
        uiManager.Init();

        GameObject.Instantiate(baseHomeLevelPrefab); // �ʱ� �� �ε�
    }
}
