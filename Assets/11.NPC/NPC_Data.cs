using UnityEngine;

[CreateAssetMenu(fileName = "NPC Data", menuName = "Scriptable Object/NPC Data")]
public class NPC_Data : ScriptableObject
{
    public int dialogID = -1;
    public int overheadDialogID = -1;

    public Vector2 overheadUI_Offset = new Vector2(0f, 1f);
    public float overheadUI_Distance = 8f;
    public float gKeyIconDistance = 2f;
}
