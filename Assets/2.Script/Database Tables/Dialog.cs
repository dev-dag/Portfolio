using UnityEngine;
using SQLite4Unity3d;
using System.Collections.Generic;

namespace Database_Table
{
    [Table("dialog")]
    public class Dialog
    {
        [Column("id"), PrimaryKey]
        public int ID { get; set; }

        [Column("index"), PrimaryKey]
        public int Index { get; set; }

        [Column("dialog_text")]
        public string DialogText { get; set; }
    }

    public class DialogWrapper
    {
        public int ID { get; private set; }
        public Dictionary<int, string> DialogText { get; private set; } // Dictionary<Index, DialogText>

        private DialogWrapper() {}

        public DialogWrapper(int ID)
        {
            this.ID = ID;
            DialogText = new Dictionary<int, string>();
        }
    }
}
