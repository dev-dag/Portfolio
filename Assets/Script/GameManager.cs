using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : SingleTon<GameManager>
{
    public InputActionAsset globalInputActionAsset;

    [Space(20f)]
    public DataContainer data;
    [HideInInspector] public UI_Manager uiManager;

    protected override void Awake()
    {
        base.Awake();

        DontDestroyOnLoad(this.gameObject);

        DB_Connecter dbConnecter = new DB_Connecter();

        data = dbConnecter.ConnectAndLoadDB();

        LoadLevelScene();
    }

    private async Awaitable LoadLevelScene()
    {
        await SceneManager.LoadSceneAsync(1);

        globalInputActionAsset.Enable(); // ��ǲ Ȱ��ȭ

        Caching();
    }

    private void Caching()
    {
        // UI �Ŵ��� ĳ��
        uiManager = GameObject.FindWithTag("UI_Manager")?.GetComponent<UI_Manager>();

        if (uiManager == null)
        {
            Debug.LogError("UI Manager ���� ĳ�� ����");
        }
    }
}
