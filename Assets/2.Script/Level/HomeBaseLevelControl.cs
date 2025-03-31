using UnityEngine;

public class HomeBaseLevelControl : LevelControl
{
    protected override void Start()
    {
        base.Start();

        Current = this;
    }
}
