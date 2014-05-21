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
    class Checks
    {

        public static void CheckHealth()
        {
            //See if I am low health based on LowHealth set in config options
            try
            {
                if (Plugin.lowHealthEnabled)
                {
                    if (ZetaDia.Me.HitpointsCurrentPct < ((double)Plugin.lowHealthPerc / 100))
                    {
                        Helpers.SetTrue("Low Health");
                        Plugin._lowHealthDuration.Reset();
                    }
                    if (!Plugin._lowHealthDuration.IsFinished)
                        Helpers.SetTrue("Low Health");
                }
                //Check for Critical HP levels if DH chestpiece not on cooldown
                if (Plugin.criticalHealthEnabled)
                {
                    if ((ZetaDia.Me.HitpointsCurrentPct < .25) && (Plugin._beckonTimer.IsFinished) && (ZetaDia.Me.ActorClass == ActorClass.DemonHunter))
                    {
                        Helpers.SetTrue("Critical Health");
                        Plugin._beckonTimer.Reset();
                    }
                }
            }
            catch (Exception e)
            {
                Plugin.WriteToLog("Error determining players current health!", e);
            }

        }

        public static void CheckShiMizu()
        {
            try
            {
                if (ZetaDia.Me.HitpointsCurrentPct < ((double)Plugin.shiMizuPerc / 100))
                {
                    Helpers.SetTrue("Shi Mizu's Hoari");
                }
            }
            catch (Exception e)
            {
                Plugin.WriteToLog("Error determining players current health!", e);
            }
        }

        public static void CheckShrines()
        {

            //Look for shrines within 30 
            try
            {
                int shrines = ZetaDia.Actors.GetActorsOfType<DiaObject>(true, false).Count(IsValidShrine);
                if (shrines > 0)
                    Helpers.SetTrue("Shrine");
            }
            catch (Exception e)
            {
                Plugin.WriteToLog("Error looking for Shrines!", e);
            }
            
        }

        public static void CheckWells()
        {
            //look for wells within 30 
            try
            {
                int wells = ZetaDia.Actors.GetActorsOfType<DiaObject>(true, false).Count(IsValidWell);
                if (wells > 0)
                    Helpers.SetTrue("Well");
            }
            catch (Exception e)
            {
                Plugin.WriteToLog("Error looking for Shrines!", e);
            }
        }
        public static void CheckChests()
        {
            if (ZetaDia.Me.GetAllBuffs().Count(a => a.SNOId == 318881) == 1)
            {
                Helpers.SetTrue("Chest");
                return;
            }
            //look for chests within 30 
            try
            {
                int chests = ZetaDia.Actors.GetActorsOfType<DiaObject>(true, false).Count(IsValidChest);
                if (chests > 0)
                    Helpers.SetTrue("Chest");
            }
            catch (Exception e)
            {
                Plugin.WriteToLog("Error looking for Chests!", e);
            }
        }

        public static int CountHostile()
        {
            //Try to count the hostile enemies within 12f (using new IsHostile condition
            try
            {
                return ZetaDia.Actors.GetActorsOfType<DiaUnit>(true, false).Count(IsValidHosileUnit);
            }
            catch (Exception e)
            {
                Plugin.WriteToLog("Error counting hostile enemies within 12f!", e);
                return 0;
            }
        }

        public static void CheckMagicFind(int enemyCount)
        {
            if (enemyCount != 0)
            {
                int elites = 0;
                int MF = 0;
                int MF2 = 0;
                try
                {
                    MF = ZetaDia.Actors.GetActorsOfType<DiaUnit>(true, false).Count(IsValidMFRareUnit);
                    if (MF > 0)
                        Helpers.SetTrue("Magic Find");
                }
                catch (Exception e)
                {
                    Plugin.WriteToLog("Error counting Uniques for Magic Find!", e);
                }

                try
                {
                    elites = ZetaDia.Actors.GetActorsOfType<DiaUnit>(true, false).Count(IsValidElite);
                    if (elites > 0)
                    {
                        MF2 = ZetaDia.Actors.GetActorsOfType<DiaUnit>(true, false).Count(IsValidMFEliteUnit);
                        if (MF2 == elites)
                            Helpers.SetTrue("Magic Find");
                    }
                }
                catch (Exception e)
                {
                    Plugin.WriteToLog("Error counting Rares for Magic Find!", e);
                }
                //
                if (MF != 0)
                    Helpers.SetTrue("Magic Find");

            }

        }
        public static void Check3Enemies(int enemyCount)
        {
            try
            {
                if ((ZetaDia.Me.GetAllBuffs().Count(a => a.SNOId == 364342) == 1 && enemyCount > 0) || enemyCount >= 3)
                    Helpers.SetTrue("3Enemies");
            }
            catch (Exception e)
            {
                Plugin.WriteToLog("Error checking for 3Enemies!", e);
            }
        }

        public static void Check5Enemies(int enemyCount)
        {
            try
            {

                if ((enemyCount >= 5) && (Plugin._archewTimer.IsFinished))
                {
                    Helpers.SetTrue("5Enemies");
                    Plugin._archewTimer.Reset();
                }
            }
            catch (Exception e)
            {
                Plugin.WriteToLog("Error checking for 5Enemies!", e);
            }
           
        }

        public static void CheckArcane()
        {
            try
            {
                int c = ZetaDia.Actors.GetActorsOfType<ACD>(true, false).Count(a => a != null && a.Distance < 55f && ((a.ActorSNO == 219702) || (a.ActorSNO == 221225) || (a.MonsterAffixes.HasFlag(MonsterAffixes.ArcaneEnchanted))));
                if (c != 0)
                    Helpers.SetTrue("Arcane");
            }
            catch (Exception e)
            {
                Plugin.WriteToLog("Error checking for Arcane!", e);
            }
        }

        public static void CheckPoison()
        {
            try
            {
            int c = ZetaDia.Actors.GetActorsOfType<ACD>(true, false).Count(a => a != null && a.Distance < 55f && ((a.ActorSNO == 93837) || (a.ActorSNO == 108869) || (a.ActorSNO == 84608) || (a.ActorSNO == 6578) || (a.MonsterAffixes.HasFlag(MonsterAffixes.Plagued)) || (a.MonsterAffixes.HasFlag(MonsterAffixes.PoisonEnchanted)) || (a.MonsterAffixes.HasFlag(MonsterAffixes.Plagued))));
            if (c != 0)
                Helpers.SetTrue("Poison");
            }
            catch (Exception e)
            {
                Plugin.WriteToLog("Error checking for Poison!", e);
            }
        }

        public static void CheckFire()
        {
            try
            {
            int c = ZetaDia.Actors.GetActorsOfType<ACD>(true, false).Count(a => a != null && a.Distance < 55f && ((a.ActorSNO == 95868) || (a.ActorSNO == 365810) || (a.ActorSNO == 4804) || (a.MonsterAffixes.HasFlag(MonsterAffixes.FireChains)) || (a.MonsterAffixes.HasFlag(MonsterAffixes.Molten)) || (a.MonsterAffixes.HasFlag(MonsterAffixes.Mortar))));
            if (c != 0)
                Helpers.SetTrue("Fire");
            }
            catch (Exception e)
            {
                Plugin.WriteToLog("Error checking for Fire!", e);
            }
        }

        public static void CheckCold()
        {
            try
            {
            int c = ZetaDia.Actors.GetActorsOfType<ACD>(true, false).Count(a => a != null && a.Distance < 55f && ((a.ActorSNO == 349774) || (a.ActorSNO == 223675) || (a.MonsterAffixes.HasFlag(MonsterAffixes.Frozen)) || (a.MonsterAffixes.HasFlag(MonsterAffixes.FrozenPulse))));
            if (c != 0)
                Helpers.SetTrue("Cold");
            }
            catch (Exception e)
            {
                Plugin.WriteToLog("Error checking for Cold!", e);
            }
        }

        public static void CheckLightning()
        {
            try
            {
            int c = ZetaDia.Actors.GetActorsOfType<ACD>(true, false).Count(a => a != null && a.Distance < 55f && ((a.ActorSNO == 4394) || (a.MonsterAffixes.HasFlag(MonsterAffixes.Electrified)) || (a.MonsterAffixes.HasFlag(MonsterAffixes.Orbiter)) || (a.MonsterAffixes.HasFlag(MonsterAffixes.Thunderstorm))));
            if (c != 0)
                Helpers.SetTrue("Lightning");
            }
            catch (Exception e)
            {
                Plugin.WriteToLog("Error checking for Lightning!", e);
            }
        }

        public static void CheckElite()
        {
            try
            {
            int c = ZetaDia.Actors.GetActorsOfType<ACD>(true, false).Count(a => a != null && a.Distance < 55f && ((a.IsElite) || (a.IsRare) || (a.IsUnique)));
            if (c != 0)
                Helpers.SetTrue("Elite");
            }
            catch (Exception e)
            {
                Plugin.WriteToLog("Error checking for Elite!", e);
            }
        }

        public static bool CheckInvuln()
        {
            if (Plugin.immmuneEnabled)
            {
                try
                {
                    if (ZetaDia.Me.IsInvulnerable)
                        return true;
                }
                catch (Exception e)
                {
                    Plugin.WriteToLog("Error checking for Invuln!", e);
                }
            }
            return false;
        }

        public static void CheckBarricade()
        {
            try
            {
                int c = ZetaDia.Actors.GetActorsOfType<DiaObject>(true, false).Count(a => a != null && a.Distance < Plugin.barricadeDistance && ((a.ActorInfo.GizmoType == GizmoType.DestroyableObject) || a.ActorInfo.GizmoType == GizmoType.BreakableChest) || (a.ActorInfo.GizmoType == GizmoType.BreakableDoor));
                if (c != 0)
                    Helpers.SetTrue("Barricade");
            }
            catch (Exception e)
            {
                Plugin.WriteToLog("Error checking for Barricade!", e);
            }
        }

        public static void CheckPuzzleRing()
        {
            try
            {
                int whiteItems = ZetaDia.Actors.GetActorsOfType<DiaItem>(true, false).Count(IsValidWhiteItem);
                if (whiteItems > 0)
                    Helpers.SetTrue("Puzzle Ring");
            }
            catch (Exception e)
            {
                Plugin.WriteToLog("Error looking for White Items!", e);
            }
        }

        private static bool IsValidMFEliteUnit(DiaUnit unit)
        {
            try
            {
                if (unit != null && unit.IsValid && unit.CommonData != null && unit.CommonData.IsValid && unit.IsHostile && unit.Distance < 70f && unit.CommonData.IsElite && unit.HitpointsCurrentPct < ((double)Plugin.MagicFindPerc / 100))
                    return true;
                else
                    return false;
            }
            catch
            {
                return false;
            }

        }
        private static bool IsValidElite(DiaUnit unit)
        {
            try
            {
                if (unit != null && unit.IsValid && unit.CommonData != null && unit.CommonData.IsValid && unit.IsHostile && unit.Distance < 70f && unit.CommonData.IsElite)
                    return true;
                else
                    return false;
            }
            catch
            {
                return false;
            }

        }
        private static bool IsValidMFRareUnit(DiaUnit unit)
        {
            try
            {
                if (unit != null && unit.IsValid && unit.CommonData != null && unit.CommonData.IsValid && unit.IsHostile && unit.Distance < 70f && unit.HitpointsCurrentPct < ((double)Plugin.MagicFindPerc / 100) && ((unit.CommonData.IsRare) || (unit.CommonData.IsUnique) || (unit.ActorSNO == 5984) || (unit.ActorSNO == 5985) || (unit.ActorSNO == 5987) || (unit.ActorSNO == 5987)))
                    return true;
                else
                    return false;
            }
            catch
            {
                return false;
            }

        }
        private static bool IsValidHosileUnit(DiaUnit unit)
        {
            try
            {
                if (unit != null && unit.IsAlive && unit.IsAttackable && unit.Distance <= 12f && unit.IsHostile)
                    return true;
                else
                    return false;
            }
            catch
            {
                return false;
            }

        }
        private static bool IsValidShrine(DiaObject unit)
        {
            
            try
            {
                if ((unit.ActorInfo.GizmoType == GizmoType.PowerUp) && (unit.IsValid))
                {
                    try
                    {
                        int gizmoState = unit.CommonData.GetAttribute<int>(ActorAttributeType.GizmoState);
                        if ((gizmoState == -1) && (unit.Distance < 30f))
                            return true;
                    }
                    catch
                    {
                        return false;
                    }
                }
            }
            catch
            {
                return false;
            }
            return false;
        }
        private static bool IsValidWell(DiaObject unit)
        {
            try
            {
                if (unit.ActorInfo.GizmoType == GizmoType.HealingWell)
                {
                    try
                    {
                        int gizmoState = unit.CommonData.GetAttribute<int>(ActorAttributeType.GizmoState);
                        if ((gizmoState == 0) && (unit.Distance < 30f))
                        {
                            return true;
                        }
                    }
                    catch
                    {
                        return false;
                    }
                }
            }
            catch
            {
                return false;
            }
            return false;
        }
        private static bool IsValidChest(DiaObject unit)
        {
            try
            {
                if (unit.ActorInfo.GizmoType == GizmoType.Chest && unit.Distance < 30f)
                {
                    try
                    {
                        int gizmoState = unit.CommonData.GetAttribute<int>(ActorAttributeType.GizmoHasBeenOperated);
                        if ((gizmoState == 0))
                        {
                            return true;
                        }
                    }
                    catch
                    {
                        return false;
                    }
                }
            }
            catch
            {
                return false;
            }
            return false;
        }
        private static bool IsValidWhiteItem(DiaItem unit)
        {
            try
            {
                if (unit != null && unit.IsValid && unit.CommonData != null && unit.CommonData.IsValid && unit.ActorType == ActorType.Item && unit.CommonData.ItemQualityLevel == ItemQuality.Normal && (unit.CommonData.ItemBaseType == ItemBaseType.Armor || unit.CommonData.ItemBaseType == ItemBaseType.Jewelry || unit.CommonData.ItemBaseType == ItemBaseType.Weapon) && unit.Distance < 30f)
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
            return false;
        }
        public static void CheckArea()
        {
            //initialize needed variable
            CheckHealth();
            int enemyCount = CountHostile();
            if (Plugin.shiMizuEnabled)
                CheckShiMizu();
            if (Plugin.puzzleRingEnabled)
                CheckPuzzleRing();
            if (Plugin.barricadeEnabled)
                CheckBarricade();
            if (Plugin.shrineEnabled)
                CheckShrines();
            if (Plugin.wellEnabled)
                CheckWells();
            if (Plugin.chestEnabled)
                CheckChests();
            if (Plugin.magicFindEnabled)
               CheckMagicFind(enemyCount);
            if (!CheckInvuln())
            {
                if (Plugin.threeEnemiesEnabled)
                    Check3Enemies(enemyCount);
                if (Plugin.fiveEnemiesEnabled)
                    Check5Enemies(enemyCount);
                if (Plugin.arcaneEnabled)
                    CheckArcane();
                if (Plugin.poisonEnabled)
                    CheckPoison();
                if (Plugin.fireEnabled)
                    CheckFire();
                if (Plugin.coldEnabled)
                    CheckCold();
                if (Plugin.lightningEnabled)
                    CheckLightning();
            }
            if (Plugin.eliteEnabled)
                CheckElite();
            Plugin.AnalyzeResults();
        }
    }
}

