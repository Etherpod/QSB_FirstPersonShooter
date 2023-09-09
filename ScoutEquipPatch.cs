using HarmonyLib;
using UnityEngine;

namespace qsbFPS;

[HarmonyPatch]
public class ScoutEquipPatch : MonoBehaviour
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(ToolModeSwapper), nameof(ToolModeSwapper.Update))]
    public static bool ToolModeSwapper_Update_Prefix(ToolModeSwapper __instance)
    {
        if (__instance._isSwitchingToolMode && !__instance._equippedTool.IsEquipped())
        {
            __instance._equippedTool = __instance._nextTool;
            __instance._nextTool = null;
            if (__instance._equippedTool != null)
            {
                __instance._equippedTool.EquipTool();
            }
            __instance._currentToolMode = __instance._nextToolMode;
            __instance._nextToolMode = ToolMode.None;
            __instance._isSwitchingToolMode = false;
        }
        InputMode inputMode = InputMode.Character | InputMode.ShipCockpit;
        if (!__instance.IsNomaiTextInFocus())
        {
            __instance._waitForLoseNomaiTextFocus = false;
        }
        if (__instance._shipDestroyed && __instance._currentToolGroup == ToolGroup.Ship)
        {
            return false;
        }
        if (__instance._currentToolMode != ToolMode.None && __instance._currentToolMode != ToolMode.Item && 
            (OWInput.IsNewlyPressed(InputLibrary.cancel, inputMode | InputMode.ScopeZoom) || PlayerState.InConversation()))
        {
            InputLibrary.cancel.ConsumeInput();
            if (__instance.GetAutoEquipTranslator() && __instance._currentToolMode == ToolMode.Translator)
            {
                __instance._waitForLoseNomaiTextFocus = true;
            }
            __instance.UnequipTool();
        }
        else if (__instance.IsNomaiTextInFocus() && __instance._currentToolMode != ToolMode.Translator && 
            ((__instance.GetAutoEquipTranslator() && !__instance._waitForLoseNomaiTextFocus) || 
            OWInput.IsNewlyPressed(InputLibrary.interact, inputMode)))
        {
            __instance.EquipToolMode(ToolMode.Translator);
            if (__instance._firstPersonManipulator.GetFocusedNomaiText() != null && 
                __instance._firstPersonManipulator.GetFocusedNomaiText().CheckTurnOffFlashlight())
            {
                Locator.GetFlashlight().TurnOff(false);
            }
        }
        else if (__instance._currentToolMode == ToolMode.Translator && !__instance.IsNomaiTextInFocus() && __instance.GetAutoEquipTranslator())
        {
            __instance.UnequipTool();
        }


        else if (OWInput.IsNewlyPressed(InputLibrary.probeLaunch, inputMode) && !qsbFPS.Instance.disableProbeLauncher)
        {
            if (__instance._currentToolGroup == ToolGroup.Suit && __instance._itemCarryTool.GetHeldItemType() == ItemType.DreamLantern)
            {
                return false;
            }
            if (((__instance._currentToolMode == ToolMode.None || __instance._currentToolMode == ToolMode.Item) && 
                Locator.GetPlayerSuit().IsWearingSuit(false)) || ((__instance._currentToolMode == ToolMode.None || 
                __instance._currentToolMode == ToolMode.SignalScope) && OWInput.IsInputMode(InputMode.ShipCockpit)))
            {
                __instance.EquipToolMode(ToolMode.Probe);
            }
        }


        else if (OWInput.IsNewlyPressed(InputLibrary.signalscope, inputMode | InputMode.ScopeZoom))
        {
            if (PlayerState.InDreamWorld())
            {
                return false;
            }
            if (__instance._currentToolMode == ToolMode.SignalScope)
            {
                __instance.UnequipTool();
            }
            else
            {
                __instance.EquipToolMode(ToolMode.SignalScope);
            }
        }
        bool flag = __instance._itemCarryTool.UpdateInteract(__instance._firstPersonManipulator, __instance.IsItemToolBlocked());
        if (!__instance._itemCarryTool.IsEquipped() && flag)
        {
            __instance.EquipToolMode(ToolMode.Item);
            return false;
        }
        if (__instance._itemCarryTool.GetHeldItem() != null && __instance._currentToolMode == ToolMode.None && 
            OWInput.IsInputMode(InputMode.Character) && !OWInput.IsChangePending())
        {
            __instance.EquipToolMode(ToolMode.Item);
        }
        return false;
    }
}