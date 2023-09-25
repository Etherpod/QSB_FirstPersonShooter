using HarmonyLib;
using QSB.Player.TransformSync;
using QSB.PlayerBodySetup.Remote;
using System;
using System.Reflection;
using UnityEngine;

namespace QSBFPS;

[HarmonyPatch]
public class PlayerObjectInterceptPatch : MonoBehaviour
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(PlayerTransformSync), "InitRemoteTransform")]
    public static bool PlayerTransformSync_InitRemoteTransform_Prefix(PlayerTransformSync __instance, ref Transform __result)
    {
        FieldInfo[] field;
        Type classType = typeof(PlayerTransformSync);
        field = classType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance);

        Transform _visibleCameraRoot;
        Transform _visibleRoastingSystem; ;
        Transform _visibleStickPivot;
        Transform _visibleStickTip;

        Transform playerTransform = RemotePlayerCreation.CreatePlayer(
            __instance.Player,
            out _visibleCameraRoot,
            out _visibleRoastingSystem,
            out _visibleStickPivot,
            out _visibleStickTip);

        for (int i = 0; i < field.Length; i++)
        {
            if (field[i].Name == "_visibleCameraRoot")
            {
                field[i].SetValue(__instance, _visibleCameraRoot);
            }
            else if (field[i].Name == "_visibleRoastingSystem")
            {
                field[i].SetValue(__instance, _visibleRoastingSystem);
            }
            else if (field[i].Name == "_visibleStickPivot")
            {
                field[i].SetValue(__instance, _visibleStickPivot);
            }
            else if (field[i].Name == "_visibleStickTip")
            {
                field[i].SetValue(__instance, _visibleStickTip);
            }
        }

        qsbFPS.Instance.idToGameObjects.Add(__instance.Player.PlayerId, playerTransform.gameObject);
        qsbFPS.Instance.ModHelper.Console.WriteLine($"ID-Object pair: {__instance.Player.PlayerId}, {playerTransform.gameObject}");

        __result = playerTransform;
        return false;
    }
}