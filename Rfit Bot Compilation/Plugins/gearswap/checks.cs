using System;
using System.Linq;
using System.Collections.Generic;
using Zeta;
using Zeta.Common;
using Zeta.Common.Plugins;
using Zeta.Game;
using Zeta.Game.Internals.SNO;
using Zeta.Game.Internals.Actors;
using Zeta.XmlEngine;
using Zeta.TreeSharp;
using Zeta.Bot.Profile;
using Zeta.Bot;
using Zeta.Common.Helpers;
using Zeta.Common.Xml;

namespace GearSwap
{
    partial class gearSwap : IPlugin
    {

        public static void checkHealth()
        {
            //See if I am low health based on LowHealth set in config options
            try
            {
                if (lowHealthEnabled)
                {
                    if (ZetaDia.Me.HitpointsCurrentPct < ((double)lowHealthPerc / 100))
                    {
                        setTrue("Low Health");
                    }
                }
                //Check for Critical HP levels if DH chestpiece not on cooldown
                if (criticalHealthEnabled)
                {
                    if ((ZetaDia.Me.HitpointsCurrentPct < .25) && (_beckonTimer.IsFinished) && (ZetaDia.Me.ActorClass == ActorClass.DemonHunter))
                    {
                        setTrue("Critical Health");
                        _beckonTimer.Reset();
                    }
                }
            }
            catch (Exception e)
            {
                writeToLog("Error determining players current health!", e);
            }

        }

        public static void checkShrinesWells()
        {
            try
            {
                if (shrineEnabled || wellEnabled)
                {
                    //See if you have Harringtons Buff if so do not dequip belt or you lose buff
                    if (ZetaDia.Me.GetAllBuffs().Count(a => a.SNOId == 318881) == 1)
                        setTrue("Chest");
                    foreach (DiaObject obj in ZetaDia.Actors.GetActorsOfType<DiaObject>(true, false))
                    {

                        if (shrineEnabled)
                        {
                            if ((obj.ActorInfo.GizmoType == GizmoType.PowerUp) && (obj.IsValid))
                            {
                                try
                                {
                                    int gizmoState = obj.CommonData.GetAttribute<int>(ActorAttributeType.GizmoState);
                                    if ((gizmoState == -1) && (obj.Distance < 30f))
                                        setTrue("Shrine");
                                }
                                catch
                                {
                                    writeToLog("Failed to get state");
                                }
                            }
                        }
                        if (wellEnabled)
                        {
                            if (obj.ActorInfo.GizmoType == GizmoType.HealingWell)
                            {
                                try
                                {
                                    int gizmoState = obj.CommonData.GetAttribute<int>(ActorAttributeType.GizmoState);
                                    if ((gizmoState == 0) && (obj.Distance < 30f))
                                    {
                                        setTrue("Well");
                                    }
                                }
                                catch
                                {
                                    writeToLog("Failed to get state");
                                }
                            }
                        }
                        if (chestEnabled)
                        {
                            if (obj.ActorInfo.GizmoType == GizmoType.Chest && obj.Distance < 30f)
                            {
                                try
                                {
                                    int gizmoState = obj.CommonData.GetAttribute<int>(ActorAttributeType.GizmoHasBeenOperated);
                                    if ((gizmoState == 0))
                                    {
                                        setTrue("Chest");
                                    }
                                }
                                catch
                                {
                                    writeToLog("Failed to get state");
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                writeToLog("Failed to check for shrines/wells!", e);
            }
        }

        public static void checkShrines()
        {

            //Look for shrines within 30 (not working - currently not used)
            try
            {
                int shrines = ZetaDia.Actors.GetActorsOfType<DiaUnit>(true, false).Count(a => a != null && a.ActorInfo != null && a.CommonData != null &&  a.ActorInfo.GizmoType == GizmoType.PowerUp && a.Distance <= 30f && (a.CommonData.GetAttribute<int>(ActorAttributeType.GizmoState) == -1));
                if (shrines > 0)
                    setTrue("Shrine");
            }
            catch (Exception e)
            {
                writeToLog("Error looking for Shrines!", e);
            }
            
        }

        public static void checkWells()
        {
            //look for wells within 30 (not working - currently not used)
            try
            {
                int wells = ZetaDia.Actors.GetActorsOfType<DiaUnit>(true, false).Count(a => a != null && a.ActorInfo != null && a.CommonData != null && a.ActorInfo.GizmoType == GizmoType.HealingWell && a.Distance <= 30f && a.CommonData.GetAttribute<int>(ActorAttributeType.GizmoState) == 0);
                if (wells > 0)
                    setTrue("Well");
            }
            catch (Exception e)
            {
                writeToLog("Error looking for Shrines!", e);
            }
        }

        public static int countHostile()
        {
            //Try to count the hostile enemies within 12f (using new IsHostile condition
            try
            {
                return ZetaDia.Actors.GetActorsOfType<DiaUnit>(true, false).Count(a => a != null && a.Distance <= 12f && a.IsHostile);
            }
            catch (Exception e)
            {
                writeToLog("Error counting hostile enemies within 12f!", e);
                return 0;
            }
        }

        public static void checkMagicFind(int enemyCount)
        {
            if (enemyCount != 0)
            {
                int elites = 0;
                int MF = 0;
                try
                {

                    //count Rares/Uniques/Goblins (Goblin SNO: 5984, 5985, 5987, 5988) that are low health based on config menu value
                    foreach (DiaUnit obj in ZetaDia.Actors.GetActorsOfType<DiaUnit>(true, false))
                    {
                        if (obj != null)
                        {
                            if (obj.HitpointsCurrentPct < ((double)MagicFindPerc / 100))
                            {
                                if ((obj.CommonData.IsRare) || (obj.CommonData.IsUnique) || (obj.ActorSNO == 5984) || (obj.ActorSNO == 5985) || (obj.ActorSNO == 5987) || (obj.ActorSNO == 5987))
                                {
                                    setTrue("Magic Find");
                                    return; // Just need 1 to make this true

                                }
                            }
                        }
                    }
                    //MF = ZetaDia.Actors.GetActorsOfType<DiaUnit>(true, false).Count(a => a.IsValid && a != null && a.CommonData != null && a.HitpointsCurrentPct < ((double)MagicFindPerc / 100) && ((a.CommonData.IsRare) || (a.CommonData.IsUnique) || (a.ActorSNO == 5984) || (a.ActorSNO == 5985) || (a.ActorSNO == 5987) || (a.ActorSNO == 5987)));
                }
                catch (Exception e)
                {
                    writeToLog("Error counting Uniques for Magic Find!", e);
                }

                try
                {
                    //count Elites that are low health based on config menu value
                    foreach (DiaUnit obj in ZetaDia.Actors.GetActorsOfType<DiaUnit>(true, false))
                    {
                        if (obj != null)
                        {
                            if (obj.CommonData.IsElite)
                            {
                                elites++;
                                if (elites > 1) //If more than 1 elite do not set true
                                {
                                    return;
                                }
                                if (obj.HitpointsCurrentPct < ((double)MagicFindPerc / 100))
                                {
                                    if (obj.CommonData.IsElite)
                                        MF++;
                                }
                            }
                        }
                    }
                    //MF2 = ZetaDia.Actors.GetActorsOfType<DiaUnit>(true, false).Count(a => a.IsValid && a != null && a.CommonData != null && a.CommonData.IsElite && a.HitpointsCurrentPct < ((double)MagicFindPerc / 100));
                    //elites = ZetaDia.Actors.GetActorsOfType<DiaUnit>(true, false).Where(a => a.CommonData.IsElite).Count();
                }
                catch (Exception e)
                {
                    writeToLog("Error counting Rares for Magic Find!", e);
                }
                //
                if (MF != 0)
                    setTrue("Magic Find");

            }
        }

        public static void check3Enemies(int enemyCount)
        {
            try
            {
                if ((enemyCount >= 3) && (_poxTimer.IsFinished))
                {
                    setTrue("3Enemies");
                    _poxTimer.Reset();

                }
            }
            catch (Exception e)
            {
                writeToLog("Error checking for 3Enemies!", e);
            }
        }

        public static void check5Enemies(int enemyCount)
        {
            try
            {

                if ((enemyCount >= 5) && (_archewTimer.IsFinished))
                {
                    setTrue("5Enemies");
                    _archewTimer.Reset();
                }
            }
            catch (Exception e)
            {
                writeToLog("Error checking for 5Enemies!", e);
            }
           
        }

        public static void checkArcane()
        {
            try
            {
                int c = ZetaDia.Actors.GetActorsOfType<ACD>(true, false).Count(a => a != null && a.Distance < 55f && ((a.ActorSNO == 219702) || (a.ActorSNO == 221225) || (a.MonsterAffixes.HasFlag(MonsterAffixes.ArcaneEnchanted))));
                if (c != 0)
                    setTrue("Arcane");
            }
            catch (Exception e)
            {
                writeToLog("Error checking for Arcane!", e);
            }
        }

        public static void checkPoison()
        {
            try
            {
            int c = ZetaDia.Actors.GetActorsOfType<ACD>(true, false).Count(a => a != null && a.Distance < 55f && ((a.ActorSNO == 93837) || (a.ActorSNO == 108869) || (a.ActorSNO == 84608) || (a.ActorSNO == 6578) || (a.MonsterAffixes.HasFlag(MonsterAffixes.Plagued)) || (a.MonsterAffixes.HasFlag(MonsterAffixes.PoisonEnchanted)) || (a.MonsterAffixes.HasFlag(MonsterAffixes.Plagued))));
            if (c != 0)
                setTrue("Poison");
            }
            catch (Exception e)
            {
                writeToLog("Error checking for Poison!", e);
            }
        }

        public static void checkFire()
        {
            try
            {
            int c = ZetaDia.Actors.GetActorsOfType<ACD>(true, false).Count(a => a != null && a.Distance < 55f && ((a.ActorSNO == 95868) || (a.ActorSNO == 365810) || (a.ActorSNO == 4804) || (a.MonsterAffixes.HasFlag(MonsterAffixes.FireChains)) || (a.MonsterAffixes.HasFlag(MonsterAffixes.Molten)) || (a.MonsterAffixes.HasFlag(MonsterAffixes.Mortar))));
            if (c != 0)
                setTrue("Fire");
            }
            catch (Exception e)
            {
                writeToLog("Error checking for Fire!", e);
            }
        }

        public static void checkCold()
        {
            try
            {
            int c = ZetaDia.Actors.GetActorsOfType<ACD>(true, false).Count(a => a != null && a.Distance < 55f && ((a.ActorSNO == 349774) || (a.ActorSNO == 223675) || (a.MonsterAffixes.HasFlag(MonsterAffixes.Frozen)) || (a.MonsterAffixes.HasFlag(MonsterAffixes.FrozenPulse))));
            if (c != 0)
                setTrue("Cold");
            }
            catch (Exception e)
            {
                writeToLog("Error checking for Cold!", e);
            }
        }

        public static void checkLightning()
        {
            try
            {
            int c = ZetaDia.Actors.GetActorsOfType<ACD>(true, false).Count(a => a != null && a.Distance < 55f && ((a.ActorSNO == 4394) || (a.MonsterAffixes.HasFlag(MonsterAffixes.Electrified)) || (a.MonsterAffixes.HasFlag(MonsterAffixes.Orbiter)) || (a.MonsterAffixes.HasFlag(MonsterAffixes.Thunderstorm))));
            if (c != 0)
                setTrue("Lightning");
            }
            catch (Exception e)
            {
                writeToLog("Error checking for Lightning!", e);
            }
        }

        public static void checkElite()
        {
            try
            {
            int c = ZetaDia.Actors.GetActorsOfType<ACD>(true, false).Count(a => a != null && a.Distance < 55f && ((a.IsElite) || (a.IsRare) || (a.IsUnique)));
            if (c != 0)
                setTrue("Elite");
            }
            catch (Exception e)
            {
                writeToLog("Error checking for Elite!", e);
            }
        }

        public static bool checkInvuln()
        {
            if (immmuneEnabled)
            {
                try
                {
                    if (ZetaDia.Me.IsInvulnerable)
                        return true;
                }
                catch (Exception e)
                {
                    writeToLog("Error checking for Invuln!", e);
                }
            }
            return false;
        }

        public static void checkBarricade()
        {
            try
            {
                int c = ZetaDia.Actors.GetActorsOfType<DiaObject>(true, false).Count(a => a != null && a.Distance < barricadeDistance && ((a.ActorInfo.GizmoType == GizmoType.DestroyableObject) || a.ActorInfo.GizmoType == GizmoType.BreakableChest) || (a.ActorInfo.GizmoType == GizmoType.BreakableDoor));
                if (c != 0)
                    setTrue("Barricade");
            }
            catch (Exception e)
            {
                writeToLog("Error checking for Barricade!", e);
            }
        }

        public static void checkSnapshot()
        {
            PowerManager.CanCastFlags castFlag = PowerManager.CanCastFlags.None;
            bool offCooldown = PowerManager.CanCast(SNOPower.Barbarian_CallOfTheAncients, out castFlag);
            if (ZetaDia.Me.GetAllBuffs().Count(a => a.SNOId == 80049) != 1 && offCooldown)
            {
                setTrue("Snapshot");
                
            }
            //for (int i = 0; i< ZetaDia.Me.GetAllBuffs().Count(); i++)
            //    writeToLog("Buff: " + ZetaDia.Me.GetAllBuffs().ElementAt(i).SNOId + " : " + ZetaDia.Me.GetAllBuffs().ElementAt(i).InternalName);
        }

        public static void checkArea()
        {
            //initialize needed variable
            int enemyCount = countHostile();
            if (barricadeEnabled)
                checkBarricade();
            checkHealth();
            checkShrinesWells();
            if (magicFindEnabled)
               checkMagicFind(enemyCount);
            if (!checkInvuln())
            {
                if (threeEnemiesEnabled)
                    check3Enemies(enemyCount);
                if (fiveEnemiesEnabled)
                    check5Enemies(enemyCount);
                if (arcaneEnabled)
                    checkArcane();
                if (poisonEnabled)
                    checkPoison();
                if (fireEnabled)
                    checkFire();
                if (coldEnabled)
                    checkCold();
                if (lightningEnabled)
                    checkLightning();
            }
            if (eliteEnabled)
                checkElite();
            analyzeResults();
        }
    }
}

