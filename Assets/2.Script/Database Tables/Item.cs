using UnityEngine;
using SQLite4Unity3d;

namespace Database_Table
{
    [Table("Item")]
    public class Item
    {
        [Column("id"), PrimaryKey]
        public int ID { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("type")]
        public int Type { get; set; }
    }
}
