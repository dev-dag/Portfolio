using UnityEngine;
using SQLite4Unity3d;

namespace Database_Table
{
    [Table("dialog")]
    public class Dialog
    {
        [Column("id")]
        public int ID { get; set; }

        [Column("index")]
        public int Index { get; set; }

        [Column("dialog_text")]
        public string DialogText { get; set; }
    }
}
