using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Quest NPC Data", menuName = "Scriptable Object/Quest NPC Data")]
public class QuestNPC_Data : NPC_Data
{
    public List<int> questID_List;
}
