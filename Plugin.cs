using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace Hardmode
{
    [BepInPlugin("org.hobbitjack.plugins.hardmode", "Cold Waters: Hardmode", "1.0.1")]
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
            Torpedo.Torpedoes.Clear();

            __instance.uifunctions.levelloadmanager.tacticalmap.SetTacticalMap();
            UIFunctions.globaluifunctions.HUDholder.SetActive(true);
        }

        [HarmonyPatch(typeof(TacticalMap), "SetTacticalMap")]
        [HarmonyPrefix]
        static bool Disable3D(ref TacticalMap __instance)
        {
            string caller = new System.Diagnostics.StackTrace().GetFrame(2).GetMethod().DeclaringType.ToString();
            if (caller == "KeybindManager" || caller == "UIFunctions")
            {
                Traverse.Create(__instance).Field("tacMapEnabled").SetValue(false);
            }
            return true;
        }

        [HarmonyPatch(typeof(SubmarineFunctions), "LeavePeriscopeView")]
        [HarmonyPostfix]
        static void PreventPeriscopeCheating()
        {

            Traverse.Create(UIFunctions.globaluifunctions.levelloadmanager.tacticalmap).Field("tacMapEnabled").SetValue(false);
            UIFunctions.globaluifunctions.levelloadmanager.tacticalmap.SetTacticalMap();
        }

        [HarmonyPatch(typeof(TacticalMap), "SetTacticalMap")]
        [HarmonyPostfix]
        static void PreventESCCheating(ref TacticalMap __instance)
        {
            string caller = new System.Diagnostics.StackTrace().GetFrame(2).GetMethod().DeclaringType.ToString();
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

        [HarmonyPatch(typeof(DotModOptionsMenuController), "QuitSettings")]
        [HarmonyPostfix]
        static void NoDotModOptionsMenuEscCheating()
        {
            Traverse.Create(UIFunctions.globaluifunctions.levelloadmanager.tacticalmap).Field("tacMapEnabled").SetValue(false);
            UIFunctions.globaluifunctions.levelloadmanager.tacticalmap.SetTacticalMap();
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
            __instance.PlayerMessage($"Conn, sonar: torpedo classified as {component.databaseweapondata.weaponName}.", __instance.messageLogColors["Sonar"], "", false);
        }
        //END DISPLAY TORPEDO TYPE ON CLICK

        //TEXT FOR LOUD EXPLOSION ON BEARING
        [HarmonyPatch(typeof(Compartment), "OnTriggerEnter")]
        [HarmonyPostfix]
        static void DisplayLoudExplosionOnHit(ref Compartment __instance)
        {
            if (!__instance.activeVessel.playercontrolled)
            {
                string contactname = UIFunctions.globaluifunctions.playerfunctions.GetFullContactName(UIFunctions.globaluifunctions.playerfunctions.sensormanager.tacticalmap.mapContact[__instance.activeVessel.vesselListIndex].contactText.text, __instance.activeVessel.vesselListIndex);
                if (UIFunctions.globaluifunctions.playerfunctions.sensormanager.detectedByPlayer[__instance.activeVessel.vesselListIndex] && !__instance.activeVessel.isSinking)
                {
                    UIFunctions.globaluifunctions.playerfunctions.PlayerMessage($"Conn, sonar: loud explosion on the bearing of {contactname}.", UIFunctions.globaluifunctions.playerfunctions.messageLogColors["Sonar"], "", true);
                }
            }
        }
        //END TEXT FOR LOUD EXPLOSION ON BEARING

        //NO TAIL BONUS WHILE TURNING
        [HarmonyPatch(typeof(SensorManager), "GetArrayBonus")]
        [HarmonyPostfix]
        static Vector2 NoTailWhileTurning(ref Vector2 __result, Vessel activeVessel)
        {
            if (activeVessel != null)
            {
                if (activeVessel.vesselmovement.rudderAngle.y != 0)
                {
                    return new Vector2(0, 0);
                }
            }
            return __result;
        }
        //END NO TAIL BONUS WHILE TURNING

        //NO CERTAIN POSITIONS FOR ENEMIES
        [HarmonyPatch(typeof(TacticalMap), nameof(TacticalMap.))]
        [HarmonyPrefix]
        static void DisablePerfectInformation(ref int i)
        {
            if (GameDataManager.playervesselsonlevel[0].submarineFunctions.GetMastIsUp(0) && GameDataManager.enemyvesselsonlevel[i].acoustics.playerHasDetectedWith[0] || GameDataManager.playervesselsonlevel[0].submarineFunctions.GetMastIsUp(2) && GameDataManager.enemyvesselsonlevel[i].acoustics.playerHasDetectedWith[2]) return;
            Debug.Log("HI?");
            if (UIFunctions.globaluifunctions.playerfunctions.sensormanager.solutionQualityOfContacts[i] >= UIFunctions.globaluifunctions.playerfunctions.sensormanager.tacticalmap.qualityToDrawTails) UIFunctions.globaluifunctions.playerfunctions.sensormanager.solutionQualityOfContacts[i] = UIFunctions.globaluifunctions.playerfunctions.sensormanager.tacticalmap.qualityToDrawTails - .1f;
        }
        //END NO CERTAIN POSITIONS FOR ENEMIES
    }
}
