using UnityEngine;
using SQLite4Unity3d;
using Database_Table;
using System.Collections.Generic;

public class DB_Connecter
{
    public DataContainer dataContainer;

    private SQLiteConnection connection;

    public DataContainer ConnectAndLoadDB()
    {
        dataContainer = new DataContainer();
        connection = new SQLiteConnection(Application.streamingAssetsPath + "\\BaseData.db");

        Application.quitting += () => connection.Close();

        LoadTables();

        return dataContainer;
    }

    /// <summary>
    /// DB 테이블 로드
    /// </summary>
    private void LoadTables()
    {
        dataContainer.dialog = connection.Query<Database_Table.Dialog>("select * from dialog");
    }
}

public class DataContainer
{
    public List<Database_Table.Dialog> dialog;
}