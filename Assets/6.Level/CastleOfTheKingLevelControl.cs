using Unity.Cinemachine;
using UnityEngine;

public class CastleOfTheKingLevelControl : LevelControl
{
    [SerializeField] private NPC npc;
    [SerializeField] private float closeUpLensValue = 5f;

    public void DungeonClear()
    {
        Player.Current.BlockInput = true;

        GameManager.Instance.gameUI.ShowUI_ForCinematic(false);

        Progress();
    }

    private async Awaitable Progress()
    {
        await Awaitable.WaitForSecondsAsync(3f); // 1초 대기

        Player.Current.transform.rotation = Quaternion.Euler(0f, 180f, 0f); // 플레이어 회전

        npc.gameObject.SetActive(true); // NPC를 활용한 연출 시작

        var cam = GameObject.FindWithTag("MainCamera"); // 시네머신 카메라 팔로우 바인딩
        var cineCam = cam.GetComponent<CinemachineCamera>();
        cineCam.Lens = new LensSettings()
        {
            Dutch = cineCam.Lens.Dutch,
            FarClipPlane = cineCam.Lens.FarClipPlane,
            FieldOfView = cineCam.Lens.FieldOfView,
            ModeOverride = cineCam.Lens.ModeOverride,
            NearClipPlane = cineCam.Lens.NearClipPlane,
            PhysicalProperties = cineCam.Lens.PhysicalProperties,
            OrthographicSize = closeUpLensValue,
        }; 

        cineCam.Target = new CameraTarget()
        {
            TrackingTarget = npc.transform
        };

        // 홈화면으로 이동
    }
}
