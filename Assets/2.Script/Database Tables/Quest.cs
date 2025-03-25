using UnityEngine;
using SQLite4Unity3d;

namespace Database_Table
{
    [Table("quest")]
    public class Quest
    {
        [Column("id"), PrimaryKey]
        public int ID { get; set; }

        [Column("start_dialog_id")]
        public int StartDialogID { get; set; }

        [Column("process_dialog_id")]
        public int ProcessDialogID { get; set; }

        [Column("complete_dialog_id")]
        public int CompleteDialogID { get; set; }

        [Column("overhead_dialog_id")]
        public int OverheadDialogID { get; set; }

        [Column("quest_class_name")]
        public string QuestClassName { get; set; }

        [Column("reward_id")]
        public int RewardID { get; set; }
    }
}
