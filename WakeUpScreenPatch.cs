using HarmonyLib;
using QSB;
using QSB.HUD;
using QSB.Player.Messages;
using QSBFPS;
using System;
using System.Reflection;
using UnityEngine;

[HarmonyPatch]
public class WakeUpScreenPatch : MonoBehaviour
{
    /*[HarmonyPrefix]
    [HarmonyPatch(typeof(PlayerCameraEffectController), nameof(PlayerCameraEffectController.OnStartOfTimeLoop))]
    public static bool PlayerCameraEffectController_OnStartOfTimeLoop_Prefix(PlayerCameraEffectController __instance)
    {
        if (__instance.gameObject.CompareTag("MainCamera") && LoadManager.GetCurrentScene() != OWScene.EyeOfTheUniverse)
        {
            if (LoadManager.GetPreviousScene() == OWScene.TitleScreen)
            {
                qsbFPS.Instance.SpawnReadyUpCanvas();
                __instance._owCamera.postProcessingSettings.eyeMask.openness = 0f;
                __instance._owCamera.postProcessingSettings.bloom.threshold = 0f;
                __instance._owCamera.postProcessingSettings.eyeMaskEnabled = true;
                //Wake input is going to be manually called
                __instance._waitForWakeInput = false;
                __instance._wakePrompt = new ScreenPrompt(InputLibrary.interact, UITextLibrary.GetString(UITextType.WakeUpPrompt), 0, ScreenPrompt.DisplayState.Normal, false);
                __instance._wakePrompt.SetVisibility(false);
                //Locator.GetPromptManager().AddScreenPrompt(__instance._wakePrompt, PromptPosition.Center, false);
                OWTime.Pause(OWTime.PauseType.Sleeping);
                //Locator.GetPauseCommandListener().AddPauseCommandLock();
                return false;
            }
            __instance.WakeUp();
        }

        return false;
    }*/

    [HarmonyPrefix]
    [HarmonyPatch(typeof(CursorManager), nameof(CursorManager.RefreshCursorState))]
    public static bool CursorManager_RefreshCursorState_Prefix(CursorManager __instance)
    {
        if (qsbFPS.Instance.ignoreLockCursor)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            return false;
        }

        bool flag = false;

        CursorLockMode cursorLockMode = CursorLockMode.Locked;
        if (__instance._isPaused || !__instance._hasFocus || (OWInput.IsInputMode(InputMode.Menu | InputMode.Rebinding | InputMode.KeyboardInput) && !OWInput.IsChangePending() && !OWInput.UsingGamepad()))
        {
            flag = true;
            cursorLockMode = CursorLockMode.None;
        }
        if (Cursor.visible != flag)
        {
            Cursor.visible = flag;
        }
        if (Cursor.lockState != cursorLockMode)
        {
            Cursor.lockState = cursorLockMode;
        }
        return false;
    }

    /*[HarmonyPrefix]
    [HarmonyPatch(typeof(MultiplayerHUDManager), nameof(MultiplayerHUDManager.WriteSystemMessage))]
    public static void MultiplayerHUDManager_WriteSystemMessage_Postfix(MultiplayerHUDManager __instance)
    {
        FieldInfo[] field;
        Type classType = typeof(MultiplayerHUDManager);
        field = classType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance);

        for (int i = 0; i < field.Length; i++)
        {
            if (field[i].Name == "_textChat")
            {
                qsbFPS.Instance.ModHelper.Console.WriteLine("_textChat: " + (Transform)field[i].GetValue(__instance));
            }
        }
    }*/

    [HarmonyPrefix]
    [HarmonyPatch(typeof(MultiplayerHUDManager), nameof(MultiplayerHUDManager.WriteMessage))]
    public static bool CheckTextChat(MultiplayerHUDManager __instance)
    {
        FieldInfo[] field;
        Type classType = typeof(MultiplayerHUDManager);
        field = classType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
        Transform textChat = null;

        for (int i = 0; i < field.Length; i++)
        {
            if (field[i].Name == "_textChat")
            {
                qsbFPS.Instance.ModHelper.Console.WriteLine("_textChat: " + field[i].GetValue(__instance));
                textChat = (Transform)field[i].GetValue(__instance);
            }
        }

        if (textChat == null)
        {
            return false;
        }

        return true;
    }
}