using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace Hardmode
{
    [BepInPlugin("org.hobbitjack.plugins.hardmode", "Cold Waters: Hardmode", "1.0.0")]
    [BepInProcess("ColdWaters.exe")]
    public class HardmodePlugin : BaseUnityPlugin
    {
        private void Awake()
        {
            // Plugin startup logic
            Logger.LogInfo("Plugin CW Hardmode is loaded!");
            Harmony.CreateAndPatchAll(typeof(HardmodePlugin));
        }

        // DISABLE 3D MODE
        [HarmonyPatch(typeof(MissionManager), "Battlestations")]
        [HarmonyPostfix]
        static void OpenTacMapOnStart(ref MissionManager __instance)
        {
            __instance.uifunctions.levelloadmanager.tacticalmap.SetTacticalMap();
            UIFunctions.globaluifunctions.HUDholder.SetActive(true);
        }

        [HarmonyPatch(typeof(TacticalMap), "SetTacticalMap")]
        [HarmonyPrefix]
        static bool Disable3D(ref TacticalMap __instance)
        {
            string caller = (new System.Diagnostics.StackTrace()).GetFrame(2).GetMethod().DeclaringType.ToString();
            if (caller == "KeybindManager" || caller == "UIFunctions")
            {
                Traverse.Create(__instance).Field("tacMapEnabled").SetValue(false);
            }
            return true;
        }

        [HarmonyPatch(typeof(TacticalMap), "SetTacticalMap")]
        [HarmonyPostfix]
        static void PreventESCCheating(ref TacticalMap __instance)
        {
            string caller = (new System.Diagnostics.StackTrace()).GetFrame(2).GetMethod().DeclaringType.ToString();
            if (caller == "ManualCameraZoom")
            {
                UIFunctions.globaluifunctions.missionmanager.BringInExitMenu(false);
            }
        }

        [HarmonyPatch(typeof(MissionManager), "DismissExitMenu")]
        [HarmonyPostfix]
        static void BringBackTacMapAfterExit(ref MissionManager __instance)
        {
            __instance.uifunctions.levelloadmanager.tacticalmap.SetTacticalMap();
        }
        //END DISABLE 3D MODE

        //ALWAYS DISABLE AUTOCLASSIFICATON
        [HarmonyPatch(typeof(SensorManager), "ContactClassified")]
        [HarmonyPrefix]
        static bool DisableAutoClassification()
        {
            return false;
        }
        //END ALWAYS DISABLE AUTOCLASSIFICATON

        //DISPLAY TORPEDO TYPE ON CLICK
        [HarmonyPatch(typeof(PlayerFunctions), "MapTorpedoButton")]
        [HarmonyPostfix]
        static void DisplayTorpedoTypeOnClick(ref PlayerFunctions __instance, Transform torpedoPosition)
        {
            Torpedo component = torpedoPosition.gameObject.GetComponent<Torpedo>();
            __instance.PlayerMessage($"Conn, Sonar, torpedo classified as {component.databaseweapondata.weaponName}.", __instance.messageLogColors["Sonar"], "", false);
        }
        //END DISPLAY TORPEDO TYPE ON CLICK

        //TEXT FOR LOUD EXPLOSION ON BEARING
        [HarmonyPatch(typeof(Compartment), "OnTriggerEnter")]
        [HarmonyPostfix]
        static void DisplayLoudExplosionOnHit(ref Compartment __instance)
        {
            string contactname = UIFunctions.globaluifunctions.playerfunctions.GetFullContactName(UIFunctions.globaluifunctions.playerfunctions.sensormanager.tacticalmap.mapContact[__instance.activeVessel.vesselListIndex].contactText.text, __instance.activeVessel.vesselListIndex);
            UIFunctions.globaluifunctions.playerfunctions.PlayerMessage($"Conn, Sonar, loud explosion on the bearing of {contactname}.", UIFunctions.globaluifunctions.playerfunctions.messageLogColors["Sonar"], "", false);
        }
    }
}
