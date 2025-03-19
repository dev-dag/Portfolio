using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : SingleTon<GameManager>
{
    public DataContainer data;

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
    }
}
