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
    /// DB ���̺� �ε�
    /// </summary>
    private void LoadTables()
    {
        dataContainer = new DataContainer();

        // Dialog �ε�
        {
            var result = connection.Query<Database_Table.Dialog>("select * from dialog");

            foreach (var table in result)
            {
                if (dataContainer.dialog.ContainsKey(table.ID) == false)
                {
                    dataContainer.dialog.Add(table.ID, new DialogWrapper(table.ID));
                }

                dataContainer.dialog[table.ID].DialogText.Add(table.Index, table.DialogText);
            }
        }

        // Overhead Dialog �ε�
        {
            var result = connection.Query<Database_Table.OverheadDialog>("select * from overhead_dialog");

            foreach (var table in result)
            {
                dataContainer.overheadDialog.Add(table.ID, table);
            }
        }

        // Quest �ε�
        {
            var result = connection.Query<Database_Table.Quest>("select * from quest");

            foreach (var table in result)
            {
                dataContainer.quest.Add(table.ID, table);
            }
        }
    }
}

public class DataContainer
{
    public Dictionary<int, DialogWrapper> dialog = new Dictionary<int, DialogWrapper>(); // Dictionary<Dialog.ID, DialogWrapper>
    public Dictionary<int, Database_Table.OverheadDialog> overheadDialog = new Dictionary<int, Database_Table.OverheadDialog>(); // Dictionary<OverheadDialog.ID, OverheadDialog>
    public Dictionary<int, Database_Table.Quest> quest = new Dictionary<int, Database_Table.Quest>(); // Dictionary<Quest.ID, Quest>
}