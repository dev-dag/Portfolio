using UnityEngine;
using SQLite4Unity3d;

namespace Database_Table
{
    [Table("overhead_dialog")]
    public class OverheadDialog
    {
        [Column("id"), PrimaryKey]
        public int ID { get; set; }

        [Column("dialog_text")]
        public string DialogText { get; set; }
    }
}
