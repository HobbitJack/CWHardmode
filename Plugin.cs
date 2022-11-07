using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace Hardmode
{
    [BepInPlugin("org.hobbitjack.plugins.hardmode", "Cold Waters: Hardmode", "1.0.1")]
    [BepInProcess("ColdWaters.exe")]
    public class HardmodePlugin : BaseUnityPlugin
    {

        //static GameObject[] mirrorcontacts { get; set; }
        //static Vector3[] mirrorcontactpositions { get; set; }
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

        //DRAW TA MIRROR CONTACTS
        /*[HarmonyPatch(typeof(TacticalMap), "TacticalMapInit")]
        [HarmonyPostfix]
        static void InitializeMirrorContacts()
        {

            HardmodePlugin.mirrorcontacts = new GameObject[GameDataManager.enemyNumberofShips];
            HardmodePlugin.mirrorcontactpositions = new Vector3[GameDataManager.enemyNumberofShips];
        }

        [HarmonyPatch(typeof(SensorManager), "CalculatePlayerTMA")]
        [HarmonyPostfix]
        static void DrawTAMirrorContacts(ref SensorManager __instance, int enemyIndex, bool detectedByPlayerWithActive)
        {
            if (!(UIFunctions.globaluifunctions.playerfunctions.playerVessel.databaseshipdata.towedSonarID == -1) && UIFunctions.globaluifunctions.playerfunctions.damagecontrol.CheckSubsystem("TOWED", false))
            {
                if (GameDataManager.enemyvesselsonlevel[enemyIndex].acoustics.playerHasDetectedWith[0] && !GameDataManager.enemyvesselsonlevel[enemyIndex].acoustics.playerHasDetectedWith[1] && !GameDataManager.enemyvesselsonlevel[enemyIndex].acoustics.playerHasDetectedWith[2] && !GameDataManager.enemyvesselsonlevel[enemyIndex].acoustics.playerHasDetectedWith[3])
                {
                    int passiveSonar = int.Parse($"{Traverse.Create(UIFunctions.globaluifunctions.playerfunctions.sensormanager).Method("GetSonarReadingValues", GameDataManager.enemyvesselsonlevel[enemyIndex], GameDataManager.enemyvesselsonlevel[enemyIndex].vesselai.sensordata.playerSignatureData, true).GetValue()}".Split('\n')[1]);
                    if (!detectedByPlayerWithActive && passiveSonar <= UIFunctions.globaluifunctions.playerfunctions.sensormanager.detectionThresholds.y)
                    {
                        float enemyBearing = UIFunctions.globaluifunctions.GetBearingToTransform(UIFunctions.globaluifunctions.playerfunctions.playerVessel.transform, GameDataManager.enemyvesselsonlevel[enemyIndex].transform);
                        float bearingDiff = 180f - (360f % (UIFunctions.globaluifunctions.playerfunctions.playerVessel.CurrentHeading - enemyBearing));
                        float mirrorTABearing = (UIFunctions.globaluifunctions.playerfunctions.playerVessel.CurrentHeading + 180f) % 360f;
                        float mirrorContactBearing = enemyBearing > mirrorTABearing ? (bearingDiff - mirrorTABearing) % 360f : (bearingDiff + mirrorTABearing) % 360f;
                        GameDataManager.playervesselsonlevel[0].acoustics.sensorNavigator.transform.LookAt(GameDataManager.enemyvesselsonlevel[enemyIndex].transform.position);
                        GameDataManager.playervesselsonlevel[0].acoustics.sensorNavigator.transform.Translate(Vector3.forward * __instance.solutionRangeErrors[enemyIndex] * GameDataManager.inverseYardsScale * __instance.rangeToContacts[enemyIndex]);
                        GameDataManager.playervesselsonlevel[0].acoustics.sensorNavigator.transform.localPosition = Vector3.zero;
                        if (UIFunctions.globaluifunctions.playerfunctions.sensormanager.solutionQualityOfContacts[enemyIndex] > UIFunctions.globaluifunctions.playerfunctions.sensormanager.tacticalmap.qualityToDrawTails)
                        {
                            UIFunctions.globaluifunctions.playerfunctions.sensormanager.solutionQualityOfContacts[enemyIndex] = UIFunctions.globaluifunctions.playerfunctions.sensormanager.tacticalmap.qualityToDrawTails - 1f;
                        }
                    }
                }
            }
        }
        static void RefreshContact(TacticalMap tacticalMap, Vessel activeVessel)
        {
            int i = activeVessel.vesselListIndex;
            if (activeVessel.isSinking || activeVessel.isCapsizing)
            {
                return;
            }
            else
            {
                if (tacticalMap.sensormanager.detectedByPlayer[i])
                {
                    HardmodePlugin.mirrorcontacts[i].transform.localPosition = new Vector3(tacticalMap.sensormanager.enemyPositions[i].x * tacticalMap.zoomFactor, tacticalMap.sensormanager.enemyPositions[i].z * tacticalMap.zoomFactor, -5f);
                    Quaternion rotation = Quaternion.identity;
                    if (tacticalMap.sensormanager.solutionQualityOfContacts[i] < tacticalMap.qualityToCourse)
                    {
                        if (tacticalMap.sensormanager.identifiedByPlayer[i])
                        {
                            tacticalMap.mapContact[i].shipDisplayIcon.sprite = tacticalMap.sensormanager.sonarPaintImages[tacticalMap.sensormanager.shipTypes[i]];
                        }
                        else
                        {
                            tacticalMap.mapContact[i].shipDisplayIcon.sprite = tacticalMap.sensormanager.sonarPaintImages[0];
                        }
                    }
                    else
                    {
                        Vector3 eulerAngles = activeVessel.transform.eulerAngles;
                        rotation = Quaternion.Euler(0f, 180f, eulerAngles.y);
                        tacticalMap.mapContact[i].shipDisplayIcon.sprite = tacticalMap.sensormanager.sonarPaintImages[5];
                    }
                    tacticalMap.mapContact[i].shipDisplayIcon.transform.rotation = rotation;
                }
            }
        }
        //END DRAW TA MIRROR CONTACTS
        */
    }
}
