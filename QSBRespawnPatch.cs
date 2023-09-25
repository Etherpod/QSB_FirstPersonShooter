using HarmonyLib;
using UnityEngine;
using QSB.RespawnSync;
using QSB.Patches;
using QSB.DeathSync;
using System.Linq;

namespace QSBFPS;

[HarmonyPatch]
public class QSBRespawnPatch : MonoBehaviour
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(RespawnManager), nameof(RespawnManager.Respawn))]
    public static bool RespawnManager_Respawn_Prefix()
    {
        var mapController = FindObjectOfType<MapController>();
        QSBPatchManager.DoUnpatchType(QSBPatchTypes.RespawnTime);

        var playerSpawner = FindObjectOfType<PlayerSpawner>();
        playerSpawner.DebugWarp(playerSpawner.GetSpawnPoint((SpawnLocation)17));

        mapController.ExitMapView();

        var cameraEffectController = Locator.GetPlayerCamera().GetComponent<PlayerCameraEffectController>();
        cameraEffectController.OpenEyes(1f);

        OWInput.ChangeInputMode(InputMode.Character);

        Locator.GetPlayerSuit().SuitUp(false, false, true);

        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(RespawnManager), nameof(RespawnManager.TriggerRespawnMap))]
    public static bool RespawnManager_TriggerRespawnMap_Prefix()
    {
        qsbFPS.Instance.StartCoroutine("RespawnDelay");
        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(RespawnOnDeath), "OnGUI")]
    public static bool RespawnOnDeath_OnGUI_Prefix(RespawnOnDeath __instance)
    {
        GUIStyle _deadTextStyle = new();
        _deadTextStyle.font = (Font)Resources.Load(@"fonts\english - latin\SpaceMono-Regular_Dynamic");
        _deadTextStyle.alignment = TextAnchor.MiddleCenter;
        _deadTextStyle.normal.textColor = Color.white;
        _deadTextStyle.fontSize = 20;

        if (QSB.Player.TransformSync.PlayerTransformSync.LocalInstance == null || QSB.ShipSync.ShipManager.Instance.ShipCockpitUI == null)
        {
            return false;
        }

        if (QSB.Player.QSBPlayerManager.LocalPlayer.IsDead)
        {
            GUI.contentColor = Color.white;

            var width = 200;
            var height = 100;

            // it is good day to be not dead

            var secondText = QSB.ShipSync.ShipManager.Instance.IsShipWrecked
                ? string.Format(QSB.Localization.QSBLocalization.Current.WaitingForAllToDie, QSB.Player.QSBPlayerManager.PlayerList.Count(x => !x.IsDead))
                : "Waiting for round to end..."/*QSB.Localization.QSBLocalization.Current.WaitingForRespawn*/;

            GUI.Label(
                new Rect((Screen.width / 2) - (width / 2), (Screen.height / 2) - (height / 2) + (height * 2), width, height),
                $"{QSB.Localization.QSBLocalization.Current.YouAreDead}\n{secondText}",
                _deadTextStyle);
        }

        return false;
    }
}