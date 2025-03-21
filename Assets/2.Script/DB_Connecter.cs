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
        dataContainer = new DataContainer();

        // Dialog 로드
        {
            var result = connection.Query<Database_Table.Dialog>("select * from dialog");

            foreach (var table in result)
            {
                if (dataContainer.dialog.ContainsKey(table.ID) == false)
                {
                    dataContainer.dialog.Add(table.ID, new List<Database_Table.Dialog>());
                }

                dataContainer.dialog[table.ID].Add(table);
            }
        }

        // Overhead Dialog 로드
        {
            var result = connection.Query<Database_Table.OverheadDialog>("select * from overhead_dialog");

            foreach (var table in result)
            {
                dataContainer.overheadDialog.Add(table.ID, table);
            }
        }
    }
}

public class DataContainer
{
    public Dictionary<int, List<Database_Table.Dialog>> dialog = new Dictionary<int, List<Database_Table.Dialog>>(); // Dictionary<Dialog.ID, Dialog>
    public Dictionary<int, OverheadDialog> overheadDialog = new Dictionary<int, OverheadDialog>(); // Dictionary<OverheadDialog.ID, OverheadDialog>
}