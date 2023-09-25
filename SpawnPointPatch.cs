using HarmonyLib;
using OWML.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace QSBFPS;

[HarmonyPatch]
public class SpawnPointPatch : MonoBehaviour
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(PlayerSpawner), nameof(PlayerSpawner.SpawnPlayer))]
    public static bool PlayerSpawner_SpawnPlayer_Prefix(PlayerSpawner __instance)
    {
        SpawnLocation _spawnLocation = SpawnLocation.TimberHearth;

        if (qsbFPS.Instance.customSpawn)
        {
            if (!EnumUtils.IsDefined<SpawnLocation>(17))
            {
                _spawnLocation = EnumUtils.Create<SpawnLocation>(17, "PVPArena");
            }
            else
            {
                _spawnLocation = (SpawnLocation)17;
            }
        }

        if (PlayerData.GetWarpedToTheEye())
        {
            Debug.Log("Abort player spawn. Vessel will handle it.");
            return false;
        }
        if (__instance._initialSpawnPoint == null)
        {
            if (LoadManager.GetCurrentScene() == OWScene.SolarSystem || SceneManager.GetActiveScene().name.Contains("TimberHearth") || 
                SceneManager.GetActiveScene().name.Contains("TH_BeautifulCorner") || SceneManager.GetActiveScene().name.Contains("Test"))
            {
                __instance._initialSpawnPoint = __instance.GetSpawnPoint(_spawnLocation);
            }
            if (!__instance._initialSpawnPoint)
            {
                for (int i = 0; i < __instance._spawnList.Length; i++)
                {
                    if (__instance._spawnList[i].GetSpawnLocation() != SpawnLocation.Ship && !__instance._spawnList[i].IsShipSpawn())
                    {
                        __instance._initialSpawnPoint = __instance._spawnList[i];
                        break;
                    }
                }
            }
        }
        if (__instance._initialSpawnPoint != null)
        {
            if (LoadManager.GetCurrentScene() == OWScene.SolarSystem)
            {
                __instance._cameraController.SetDegreesY(80f);
            }
            OWRigidbody attachedOWRigidbody = __instance._initialSpawnPoint.gameObject.GetAttachedOWRigidbody(false);
            __instance._playerBody.transform.position = __instance._initialSpawnPoint.transform.position;
            __instance._playerBody.transform.rotation = __instance._initialSpawnPoint.transform.rotation;
            __instance._playerBody.GetRequiredComponent<MatchInitialMotion>().SetBodyToMatch(attachedOWRigidbody);
            if (!Physics.autoSyncTransforms)
            {
                Physics.SyncTransforms();
            }
            __instance._finishSpawnNextUpdate = true;
            qsbFPS.Instance.ModHelper.Console.WriteLine("SPAWN PLAYER");
            qsbFPS.Instance.ModHelper.Console.WriteLine($"Player location: {__instance._playerBody.transform.position}");
            return false;
        }
        Debug.LogError("Spawn point is null!");

        return false;
    }
}