﻿using System;
using System.Collections.Generic;
using System.Linq;
using Trinity.Combat.Abilities;
using Trinity.Config.Combat;
using Trinity.Technicals;
using Zeta.Common;
using Zeta.Common.Plugins;
using Zeta.Game;
using Zeta.Game.Internals.Actors;
using Zeta.Game.Internals.SNO;
using Logger = Trinity.Technicals.Logger;

namespace Trinity
{
    public partial class Trinity : IPlugin
    {
        internal sealed class KitePosition
        {
            public DateTime PositionFoundTime { get; set; }
            public Vector3 Position { get; set; }
            public float Distance { get; set; }
            public KitePosition() { }
        }
        internal static KitePosition LastKitePosition = null;

        private static void RefreshSetKiting(ref Vector3 vKitePointAvoid, bool NeedToKite)
        {
            using (new PerformanceLogger("RefreshDiaObjectCache.Kiting"))
            {

                bool TryToKite = false;

                List<TrinityCacheObject> kiteMonsterList = new List<TrinityCacheObject>();

                if (CurrentTarget != null && CurrentTarget.IsUnit)
                {
                    switch (CombatBase.PlayerKiteMode)
                    {
                        case KiteMode.Never:
                            break;
                        case KiteMode.Elites:
                            kiteMonsterList = (from m in ObjectCache
                                               where m.IsUnit &&
                                               m.RadiusDistance > 0 &&
                                               m.RadiusDistance <= CombatBase.PlayerKiteDistance &&
                                               m.IsBossOrEliteRareUnique
                                               select m).ToList();
                            break;
                        case KiteMode.Bosses:
                            kiteMonsterList = (from m in ObjectCache
                                               where m.IsUnit &&
                                               m.RadiusDistance > 0 &&
                                               m.RadiusDistance <= CombatBase.PlayerKiteDistance &&
                                               m.IsBoss
                                               select m).ToList();
                            break;
                        case KiteMode.Always:
                            kiteMonsterList = (from m in ObjectCache
                                               where m.IsUnit &&
                                               m.Weight > 0 &&
                                               m.RadiusDistance > 0 &&
                                               m.RadiusDistance <= CombatBase.PlayerKiteDistance &&
                                               (m.IsBossOrEliteRareUnique ||
                                                ((m.HitPointsPct >= .15 || m.MonsterSize != MonsterSize.Swarm) && !m.IsBossOrEliteRareUnique))
                                               select m).ToList();
                            break;
                    }
                }
                if (kiteMonsterList.Any())
                {
                    TryToKite = true;
                    vKitePointAvoid = Player.Position;
                }

                if (CombatBase.PlayerKiteDistance > 0 && kiteMonsterList.Count() > 0 && IsWizardShouldKite())
                {
                    TryToKite = true;
                    vKitePointAvoid = Player.Position;
                }

                // Avoid Death
                if (Settings.Combat.Misc.AvoidDeath &&
                    Player.CurrentHealthPct <= PlayerEmergencyHealthPotionLimit && // health is lower than potion limit
                    !SNOPowerUseTimer(SNOPower.DrinkHealthPotion) && // we can't use a potion anymore
                    TargetUtil.AnyMobsInRange(90f, false))
                {
                    Logger.LogNormal("Attempting to avoid death!");
                    NeedToKite = true;

                    kiteMonsterList = (from m in ObjectCache
                                       where m.IsUnit
                                       select m).ToList();
                }

                // Note that if treasure goblin level is set to kamikaze, even avoidance moves are disabled to reach the goblin!
                bool shouldKamikazeTreasureGoblins = (!AnyTreasureGoblinsPresent || Settings.Combat.Misc.GoblinPriority <= GoblinPriority.Prioritize);

                double msCancelledEmergency = DateTime.UtcNow.Subtract(timeCancelledEmergencyMove).TotalMilliseconds;
                bool shouldEmergencyMove = msCancelledEmergency >= cancelledEmergencyMoveForMilliseconds && NeedToKite;

                double msCancelledKite = DateTime.UtcNow.Subtract(timeCancelledKiteMove).TotalMilliseconds;
                bool shouldKite = msCancelledKite >= cancelledKiteMoveForMilliseconds && TryToKite;

                if (shouldKamikazeTreasureGoblins && (shouldEmergencyMove || shouldKite))
                {
                    Vector3 vAnySafePoint = NavHelper.FindSafeZone(false, 1, vKitePointAvoid, true, kiteMonsterList, shouldEmergencyMove);

                    if (LastKitePosition == null)
                    {
                        LastKitePosition = new KitePosition()
                        {
                            PositionFoundTime = DateTime.UtcNow,
                            Position = vAnySafePoint,
                            Distance = vAnySafePoint.Distance(Player.Position)
                        };
                    }

                    if (vAnySafePoint != Vector3.Zero && vAnySafePoint.Distance(Player.Position) >= 1)
                    {

                        if ((DateTime.UtcNow.Subtract(LastKitePosition.PositionFoundTime).TotalMilliseconds > 3000 && LastKitePosition.Position == vAnySafePoint) ||
                            (CurrentTarget != null && DateTime.UtcNow.Subtract(lastGlobalCooldownUse).TotalMilliseconds > 1500 && TryToKite))
                        {
                            timeCancelledKiteMove = DateTime.UtcNow;
                            cancelledKiteMoveForMilliseconds = 1500;
                            Logger.Log(TrinityLogLevel.Verbose, LogCategory.Movement, "Kite movement failed, cancelling for {0:0}ms", cancelledKiteMoveForMilliseconds);
                            return;
                        }
                        else
                        {
                            LastKitePosition = new KitePosition()
                            {
                                PositionFoundTime = DateTime.UtcNow,
                                Position = vAnySafePoint,
                                Distance = vAnySafePoint.Distance(Player.Position)
                            };
                        }

                        if (Settings.Advanced.LogCategories.HasFlag(LogCategory.Movement))
                        {
                            Logger.Log(TrinityLogLevel.Verbose, LogCategory.Movement, "Kiting to: {0} Distance: {1:0} Direction: {2:0}, Health%={3:0.00}, KiteDistance: {4:0}, Nearby Monsters: {5:0} NeedToKite: {6} TryToKite: {7}",
                                vAnySafePoint, vAnySafePoint.Distance(Player.Position), MathUtil.GetHeading(MathUtil.FindDirectionDegree(Me.Position, vAnySafePoint)),
                                Player.CurrentHealthPct, CombatBase.PlayerKiteDistance, kiteMonsterList.Count(),
                                NeedToKite, TryToKite);
                        }
                        CurrentTarget = new TrinityCacheObject()
                        {
                            Position = vAnySafePoint,
                            Type = GObjectType.Avoidance,
                            Weight = 90000,
                            Distance = Vector3.Distance(Player.Position, vAnySafePoint),
                            Radius = 2f,
                            InternalName = "KitePoint"
                        };
                    }
                }
                else if (!shouldEmergencyMove && NeedToKite)
                {
                    Logger.Log(TrinityLogLevel.Verbose, LogCategory.Movement, "Emergency movement cancelled for {0:0}ms", DateTime.UtcNow.Subtract(timeCancelledEmergencyMove).TotalMilliseconds - cancelledKiteMoveForMilliseconds);
                }
                else if (!shouldKite && TryToKite)
                {
                    Logger.Log(TrinityLogLevel.Verbose, LogCategory.Movement, "Kite movement cancelled for {0:0}ms", DateTime.UtcNow.Subtract(timeCancelledKiteMove).TotalMilliseconds - cancelledKiteMoveForMilliseconds);
                }
            }
        }

        private static bool IsWizardShouldKite()
        {
            if (Player.ActorClass == ActorClass.Wizard)
            {
                if (Settings.Combat.Wizard.KiteOption == WizardKiteOption.Anytime)
                    return true;

                if (GetHasBuff(SNOPower.Wizard_Archon) && Settings.Combat.Wizard.KiteOption == WizardKiteOption.ArchonOnly)
                    return true;
                if (!GetHasBuff(SNOPower.Wizard_Archon) && Settings.Combat.Wizard.KiteOption == WizardKiteOption.NormalOnly)
                    return true;

                return false;

            }
            else
                return false;
        }
    }
}
