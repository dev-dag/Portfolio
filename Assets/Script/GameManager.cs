using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : SingleTon<GameManager>
{
    public DataContainer data;
    public UI_Manager uiManager;

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

        Caching();
    }

    private void Caching()
    {
        // UI 매니저 캐싱
        uiManager = GameObject.FindWithTag("UI_Manager")?.GetComponent<UI_Manager>();

        if (uiManager == null)
        {
            Debug.LogError("UI Manager 참조 캐싱 실패");
        }
    }
}
