﻿using System;
using System.Linq;
using Trinity.Config.Combat;
using Trinity.DbProvider;
using Trinity.Technicals;
using Zeta.Bot.Navigation;
using Zeta.Common.Plugins;
using Zeta.Game;
using Zeta.Game.Internals.Actors;
using Zeta.Game.Internals.Actors.Gizmos;

namespace Trinity
{
    public partial class Trinity : IPlugin
    {
        private static bool RefreshGizmo(bool AddToCache)
        {
            if (!Settings.WorldObject.AllowPlayerResurection && CurrentCacheObject.ActorSNO == DataDictionary.PLAYER_HEADSTONE_SNO)
            {
                c_IgnoreSubStep = "IgnoreHeadstones";
                AddToCache = false;
                return AddToCache;
            }
            // start as true, then set as false as we go. If nothing matches below, it will return true.
            AddToCache = true;

            bool openResplendentChest = CurrentCacheObject.InternalName.ToLower().Contains("chest_rare");

            // Ignore it if it's not in range yet, except health wells and resplendent chests if we're opening chests
            if ((CurrentCacheObject.RadiusDistance > CurrentBotLootRange || CurrentCacheObject.RadiusDistance > 50) && CurrentCacheObject.Type != GObjectType.HealthWell && CurrentCacheObject.Type != GObjectType.Shrine && CurrentCacheObject.RActorGuid != LastTargetRactorGUID)
            {
                AddToCache = false;
                c_IgnoreSubStep = "NotInRange";
            }

            // re-add resplendent chests
            if (openResplendentChest)
            {
                AddToCache = true;
                c_IgnoreSubStep = "";
            }

            try
            {
                CurrentCacheObject.IsBountyObjective = (CurrentCacheObject.CommonData.GetAttribute<int>(ActorAttributeType.BountyObjective) > 0);
            }
            catch (Exception)
            {
                Logger.LogDebug("Error refreshing IsBountyObjective");
            }
            try
            {
                CurrentCacheObject.IsQuestMonster = CurrentCacheObject.Object.CommonData.GetAttribute<int>(ActorAttributeType.QuestMonster) > 1;
            }
            catch (Exception ex)
            {
                Logger.LogDebug(LogCategory.CacheManagement, "Error reading IsQuestMonster for Unit sno:{0} raGuid:{1} name:{2} ex:{3}",
                    CurrentCacheObject.ActorSNO, CurrentCacheObject.RActorGuid, CurrentCacheObject.InternalName, ex.Message);
            }

            try
            {
                CurrentCacheObject.IsMinimapActive = CurrentCacheObject.Object.CommonData.GetAttribute<int>(ActorAttributeType.MinimapActive) > 0;
            }
            catch (Exception ex)
            {
                Logger.LogDebug(LogCategory.CacheManagement, "Error reading MinimapActive for Gizmo sno:{0} raGuid:{1} name:{2} ex:{3}",
                    CurrentCacheObject.ActorSNO, CurrentCacheObject.RActorGuid, CurrentCacheObject.InternalName, ex.Message);
            }

            try
            {
                CurrentCacheObject.IsQuestMonster = CurrentCacheObject.Object.CommonData.GetAttribute<int>(ActorAttributeType.QuestMonster) > 0;
            }
            catch (Exception ex)
            {
                Logger.LogDebug(LogCategory.CacheManagement, "Error reading IsQuestMonster for Gizmo sno:{0} raGuid:{1} name:{2} ex:{3}",
                    CurrentCacheObject.ActorSNO, CurrentCacheObject.RActorGuid, CurrentCacheObject.InternalName, ex.Message);
            }

            if (CurrentCacheObject.Gizmo != null)
            {
                CurrentCacheObject.GizmoType = CurrentCacheObject.Gizmo.ActorInfo.GizmoType;
            }

                        // Anything that's been disabled by a script
            bool isGizmoDisabledByScript = false;
            try
            {
                if (CurrentCacheObject.Object is DiaGizmo)
                {
                    isGizmoDisabledByScript = CurrentCacheObject.Object.CommonData.GetAttribute<int>(ActorAttributeType.GizmoDisabledByScript) > 0;
                }
            }
            catch
            {
                CacheData.NavigationObstacles.Add(new CacheObstacleObject
                {
                    ActorSNO = CurrentCacheObject.ActorSNO,
                    Radius = CurrentCacheObject.Radius,
                    Position = CurrentCacheObject.Position,
                    RActorGUID = CurrentCacheObject.RActorGuid,
                    ObjectType = CurrentCacheObject.Type,
                });

                Logger.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement,
                    "Safely handled exception getting Gizmo-Disabled-By-Script attribute for object {0} [{1}]", CurrentCacheObject.InternalName, CurrentCacheObject.ActorSNO);
                c_IgnoreSubStep = "isGizmoDisabledByScriptException";
                AddToCache = false;
            }
            if (isGizmoDisabledByScript)
            {
                MainGridProvider.AddCellWeightingObstacle(CurrentCacheObject.ActorSNO, CurrentCacheObject.Radius);

                CacheData.NavigationObstacles.Add(new CacheObstacleObject
                {
                    ActorSNO = CurrentCacheObject.ActorSNO,
                    Radius = CurrentCacheObject.Radius,
                    Position = CurrentCacheObject.Position,
                    RActorGUID = CurrentCacheObject.RActorGuid,
                    ObjectType = CurrentCacheObject.Type,
                });

                AddToCache = false;
                c_IgnoreSubStep = "GizmoDisabledByScript";
                return AddToCache;

            }


            // Anything that's Untargetable
            bool untargetable = false;
            try
            {
                if (CurrentCacheObject.Object is DiaGizmo)
                {
                    untargetable = CurrentCacheObject.Object.CommonData.GetAttribute<int>(ActorAttributeType.Untargetable) > 0;
                }
            }
            catch
            {

            }


            // Anything that's Invulnerable
            bool invulnerable = false;
            try
            {
                if (CurrentCacheObject.Object is DiaGizmo)
                {
                    invulnerable = CurrentCacheObject.Object.CommonData.GetAttribute<int>(ActorAttributeType.Invulnerable) > 0;
                }
            }
            catch
            {

            }

            bool noDamage = false;
            try
            {
                if (CurrentCacheObject.Object is DiaGizmo)
                {
                    noDamage = CurrentCacheObject.Object.CommonData.GetAttribute<int>(ActorAttributeType.NoDamage) > 0;
                }
            }
            catch
            {
                CacheData.NavigationObstacles.Add(new CacheObstacleObject
                {
                    ActorSNO = CurrentCacheObject.ActorSNO,
                    Radius = CurrentCacheObject.Radius,
                    Position = CurrentCacheObject.Position,
                    RActorGUID = CurrentCacheObject.RActorGuid,
                    ObjectType = CurrentCacheObject.Type,
                });

                Logger.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement,
                    "Safely handled exception getting NoDamage attribute for object {0} [{1}]", CurrentCacheObject.InternalName, CurrentCacheObject.ActorSNO);
                c_IgnoreSubStep = "NoDamage";
                AddToCache = false;
            }

            double minDistance;
            bool gizmoUsed = false;
            switch (CurrentCacheObject.Type)
            {
                case GObjectType.Door:
                    {
                        AddToCache = true;

                        if (c_diaObject is DiaGizmo && ((DiaGizmo)c_diaObject).HasBeenOperated)
                        {
                            AddToCache = false;
                            c_IgnoreSubStep = "Door has been operated";
                            return AddToCache;
                        }

                        try
                        {
                            string currentAnimation = CurrentCacheObject.CommonData.CurrentAnimation.ToString().ToLower();
                            gizmoUsed = currentAnimation.EndsWith("open") || currentAnimation.EndsWith("opening");

                            // special hax for A3 Iron Gates
                            if (currentAnimation.Contains("irongate") && currentAnimation.Contains("open"))
                                gizmoUsed = false;
                            if (currentAnimation.Contains("irongate") && currentAnimation.Contains("idle"))
                                gizmoUsed = true;
                        }
                        catch { }
                        if (gizmoUsed)
                        {
                            Blacklist3Seconds.Add(CurrentCacheObject.RActorGuid);
                            AddToCache = false;
                            c_IgnoreSubStep = "Door is Open or Opening";
                            return AddToCache;
                        }

                        try
                        {
                            int gizmoState = CurrentCacheObject.CommonData.GetAttribute<int>(ActorAttributeType.GizmoState);
                            if (gizmoState == 1)
                            {
                                AddToCache = false;
                                c_IgnoreSubStep = "GizmoState=1";
                                return AddToCache;
                            }
                        }
                        catch
                        {
                            AddToCache = false;
                            c_IgnoreSubStep = "GizmoStateException";
                            return AddToCache;
                        }

                        if (untargetable)
                        {
                            MainGridProvider.AddCellWeightingObstacle(CurrentCacheObject.ActorSNO, CurrentCacheObject.Radius);
                            CacheData.NavigationObstacles.Add(new CacheObstacleObject
                            {
                                ActorSNO = CurrentCacheObject.ActorSNO,
                                Radius = CurrentCacheObject.Radius,
                                Position = CurrentCacheObject.Position,
                                RActorGUID = CurrentCacheObject.RActorGuid,
                                ObjectType = CurrentCacheObject.Type,
                            });

                            AddToCache = false;
                            c_IgnoreSubStep = "Untargetable";
                            return AddToCache;
                        }


                        if (AddToCache)
                        {
                            try
                            {
                                DiaGizmo door = null;
                                if (c_diaObject is DiaGizmo)
                                {
                                    door = (DiaGizmo)c_diaObject;

                                    if (door != null && door.IsGizmoDisabledByScript)
                                    {
                                        CacheData.NavigationObstacles.Add(new CacheObstacleObject
                                        {
                                            ActorSNO = CurrentCacheObject.ActorSNO,
                                            Radius = CurrentCacheObject.Radius,
                                            Position = CurrentCacheObject.Position,
                                            RActorGUID = CurrentCacheObject.RActorGuid,
                                            ObjectType = CurrentCacheObject.Type,
                                        });

                                        Blacklist3Seconds.Add(CurrentCacheObject.RActorGuid);
                                        AddToCache = false;
                                        c_IgnoreSubStep = "DoorDisabledbyScript";
                                        return AddToCache;
                                    }
                                }
                                else
                                {
                                    AddToCache = false;
                                    c_IgnoreSubStep = "InvalidCastToDoor";
                                }
                            }

                            catch { }
                        }
                    }
                    break;
               case GObjectType.Interactable:
                    {
                        AddToCache = true;
                        
                        if (untargetable)
                        {
                            ((MainGridProvider)MainGridProvider).AddCellWeightingObstacle(CurrentCacheObject.ActorSNO, CurrentCacheObject.Radius);
                            CacheData.NavigationObstacles.Add(new CacheObstacleObject()
                            {
                                ActorSNO = CurrentCacheObject.ActorSNO,
                                Radius = CurrentCacheObject.Radius,
                                Position = CurrentCacheObject.Position,
                                RActorGUID = CurrentCacheObject.RActorGuid,
                                ObjectType = CurrentCacheObject.Type,
                            });

                            AddToCache = false;
                            c_IgnoreSubStep = "Untargetable";
                            return AddToCache;
                        }


                        int endAnimation;
                        if (DataDictionary.InteractEndAnimations.TryGetValue(CurrentCacheObject.ActorSNO, out endAnimation))
                        {
                            if (endAnimation == (int)CurrentCacheObject.Gizmo.CommonData.CurrentAnimation)
                            {
                                AddToCache = false;
                                c_IgnoreSubStep = "EndAnimation";
                                return AddToCache;
                            }
                        }

                        if (CurrentCacheObject.Gizmo != null && CurrentCacheObject.Gizmo is GizmoLootContainer && CurrentCacheObject.Gizmo.HasBeenOperated)
                        {
                            AddToCache = false;
                            c_IgnoreSubStep = "GizmoHasBeenOperated";
                            return AddToCache;
                        }

                        CurrentCacheObject.Radius = c_diaObject.CollisionSphere.Radius;
                    }
                    break;
                case GObjectType.HealthWell:
                    {
                        AddToCache = true;
                        try
                        {
                            gizmoUsed = (CurrentCacheObject.CommonData.GetAttribute<int>(ActorAttributeType.GizmoCharges) <= 0 && CurrentCacheObject.CommonData.GetAttribute<int>(ActorAttributeType.GizmoCharges) > 0);
                        }
                        catch
                        {
                            Logger.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement, "Safely handled exception getting shrine-been-operated attribute for object {0} [{1}]", CurrentCacheObject.InternalName, CurrentCacheObject.ActorSNO);
                            AddToCache = true;
                            //return bWantThis;
                        }
                        try
                        {
                            int gizmoState = CurrentCacheObject.CommonData.GetAttribute<int>(ActorAttributeType.GizmoState);
                            if (gizmoState == 1)
                            {
                                AddToCache = false;
                                c_IgnoreSubStep = "GizmoState=1";
                                return AddToCache;
                            }
                        }
                        catch
                        {
                            AddToCache = false;
                            c_IgnoreSubStep = "GizmoStateException";
                            return AddToCache;
                        }
                        if (gizmoUsed)
                        {
                            c_IgnoreSubStep = "GizmoCharges";
                            AddToCache = false;
                            return AddToCache;
                        }
                        if (untargetable)
                        {
                            MainGridProvider.AddCellWeightingObstacle(CurrentCacheObject.ActorSNO, CurrentCacheObject.Radius);
                            CacheData.NavigationObstacles.Add(new CacheObstacleObject
                            {
                                ActorSNO = CurrentCacheObject.ActorSNO,
                                Radius = CurrentCacheObject.Radius,
                                Position = CurrentCacheObject.Position,
                                RActorGUID = CurrentCacheObject.RActorGuid,
                                ObjectType = CurrentCacheObject.Type,
                            });

                            AddToCache = false;
                            c_IgnoreSubStep = "Untargetable";
                            return AddToCache;
                        }

                    }
                    break;
                case GObjectType.CursedShrine:
                case GObjectType.Shrine:
                    {
                        AddToCache = true;
                        // Shrines
                        // Check if either we want to ignore all shrines
                        if (!Settings.WorldObject.UseShrine)
                        {
                            // We're ignoring all shrines, so blacklist this one
                            c_IgnoreSubStep = "IgnoreAllShrinesSet";
                            AddToCache = false;
                            return AddToCache;
                        }

                        try
                        {
                            int gizmoState = CurrentCacheObject.CommonData.GetAttribute<int>(ActorAttributeType.GizmoState);
                            if (gizmoState == 1)
                            {
                                AddToCache = false;
                                c_IgnoreSubStep = "GizmoState=1";
                                return AddToCache;
                            }
                        }
                        catch
                        {
                            AddToCache = false;
                            c_IgnoreSubStep = "GizmoStateException";
                            return AddToCache;
                        }
                        if (untargetable)
                        {
                            MainGridProvider.AddCellWeightingObstacle(CurrentCacheObject.ActorSNO, CurrentCacheObject.Radius);
                            CacheData.NavigationObstacles.Add(new CacheObstacleObject
                            {
                                ActorSNO = CurrentCacheObject.ActorSNO,
                                Radius = CurrentCacheObject.Radius,
                                Position = CurrentCacheObject.Position,
                                RActorGUID = CurrentCacheObject.RActorGuid,
                                ObjectType = CurrentCacheObject.Type,
                            });

                            AddToCache = false;
                            c_IgnoreSubStep = "Untargetable";
                            return AddToCache;
                        }


                        // Determine what shrine type it is, and blacklist if the user has disabled it
                        switch (CurrentCacheObject.ActorSNO)
                        {
                            case 176077:  //Frenzy Shrine
                                if (!Settings.WorldObject.UseFrenzyShrine)
                                {
                                    Blacklist60Seconds.Add(CurrentCacheObject.RActorGuid);
                                    c_IgnoreSubStep = "IgnoreShrineOption";
                                    AddToCache = false;
                                }
                                if (Player.ActorClass == ActorClass.Monk && Settings.Combat.Monk.TROption.HasFlag(TempestRushOption.MovementOnly) && Hotbar.Contains(SNOPower.Monk_TempestRush))
                                {
                                    // Frenzy shrines are a huge time sink for monks using Tempest Rush to move, we should ignore them.
                                    AddToCache = false;
                                    c_IgnoreSubStep = "IgnoreShrineOption";
                                    return AddToCache;
                                }
                                break;

                            case 176076:  //Fortune Shrine
                                if (!Settings.WorldObject.UseFortuneShrine)
                                {
                                    AddToCache = false;
                                    c_IgnoreSubStep = "IgnoreShrineOption";
                                    return AddToCache;
                                }
                                break;

                            case 176074:  //Protection Shrine
                                if (!Settings.WorldObject.UseProtectionShrine)
                                {
                                    AddToCache = false;
                                    c_IgnoreSubStep = "IgnoreShrineOption";
                                    return AddToCache;
                                }
                                break;

                            case 260330:  //Empowered Shrine
                                if (!Settings.WorldObject.UseEmpoweredShrine)
                                {
                                    AddToCache = false;
                                    c_IgnoreSubStep = "IgnoreShrineOption";
                                    return AddToCache;
                                }
                                break;

                            case 176075:  //Enlightened Shrine
                                if (!Settings.WorldObject.UseEnlightenedShrine)
                                {
                                    AddToCache = false;
                                    c_IgnoreSubStep = "IgnoreShrineOption";
                                    return AddToCache;
                                }
                                break;

                            case 260331:  //Fleeting Shrine
                                if (!Settings.WorldObject.UseFleetingShrine)
                                {
                                    AddToCache = false;
                                    c_IgnoreSubStep = "IgnoreShrineOption";
                                    return AddToCache;
                                }
                                break;
                        }  //end switch

                        // Bag it!
                        CurrentCacheObject.Radius = 4f;
                        break;
                    }
                case GObjectType.Barricade:
                    {
                        AddToCache = true;

                        if (noDamage)
                        {
                            MainGridProvider.AddCellWeightingObstacle(CurrentCacheObject.ActorSNO, CurrentCacheObject.Radius);
                            CacheData.NavigationObstacles.Add(new CacheObstacleObject
                            {
                                ActorSNO = CurrentCacheObject.ActorSNO,
                                Radius = CurrentCacheObject.Radius,
                                Position = CurrentCacheObject.Position,
                                RActorGUID = CurrentCacheObject.RActorGuid,
                                ObjectType = CurrentCacheObject.Type,
                            });

                            AddToCache = false;
                            c_IgnoreSubStep = "NoDamage";
                            return AddToCache;
                        }
                        if (untargetable)
                        {
                            MainGridProvider.AddCellWeightingObstacle(CurrentCacheObject.ActorSNO, CurrentCacheObject.Radius);
                            CacheData.NavigationObstacles.Add(new CacheObstacleObject
                            {
                                ActorSNO = CurrentCacheObject.ActorSNO,
                                Radius = CurrentCacheObject.Radius,
                                Position = CurrentCacheObject.Position,
                                RActorGUID = CurrentCacheObject.RActorGuid,
                                ObjectType = CurrentCacheObject.Type,
                            });

                            AddToCache = false;
                            c_IgnoreSubStep = "Untargetable";
                            return AddToCache;
                        }


                        if (invulnerable)
                        {
                            MainGridProvider.AddCellWeightingObstacle(CurrentCacheObject.ActorSNO, CurrentCacheObject.Radius);
                            CacheData.NavigationObstacles.Add(new CacheObstacleObject
                            {
                                ActorSNO = CurrentCacheObject.ActorSNO,
                                Radius = CurrentCacheObject.Radius,
                                Position = CurrentCacheObject.Position,
                                RActorGUID = CurrentCacheObject.RActorGuid,
                                ObjectType = CurrentCacheObject.Type,
                            });

                            AddToCache = false;
                            c_IgnoreSubStep = "Invulnerable";
                            return AddToCache;
                        }

                        float maxRadiusDistance;

                        if (DataDictionary.DestructableObjectRadius.TryGetValue(CurrentCacheObject.ActorSNO, out maxRadiusDistance))
                        {
                            if (CurrentCacheObject.RadiusDistance < maxRadiusDistance)
                            {
                                AddToCache = true;
                                c_IgnoreSubStep = "";
                            }
                        }

                        if (DateTime.UtcNow.Subtract(PlayerMover.LastGeneratedStuckPosition).TotalSeconds <= 1)
                        {
                            AddToCache = true;
                            c_IgnoreSubStep = "";
                            break;
                        }

                        // Set min distance to user-defined setting
                        minDistance = Settings.WorldObject.DestructibleRange + CurrentCacheObject.Radius;
                        if (_forceCloseRangeTarget)
                            minDistance += 6f;

                        // This object isn't yet in our destructible desire range
                        if (minDistance <= 0 || CurrentCacheObject.RadiusDistance > minDistance)
                        {
                            c_IgnoreSubStep = "NotInBarricadeRange";
                            AddToCache = false;
                            return AddToCache;
                        }

                        break;
                    }
                case GObjectType.Destructible:
                    {
                        AddToCache = true;

                        if (noDamage)
                        {
                            MainGridProvider.AddCellWeightingObstacle(CurrentCacheObject.ActorSNO, CurrentCacheObject.Radius);
                            CacheData.NavigationObstacles.Add(new CacheObstacleObject
                            {
                                ActorSNO = CurrentCacheObject.ActorSNO,
                                Radius = CurrentCacheObject.Radius,
                                Position = CurrentCacheObject.Position,
                                RActorGUID = CurrentCacheObject.RActorGuid,
                                ObjectType = CurrentCacheObject.Type,
                            });

                            AddToCache = false;
                            c_IgnoreSubStep = "NoDamage";
                            return AddToCache;
                        }

                        if (invulnerable)
                        {
                            MainGridProvider.AddCellWeightingObstacle(CurrentCacheObject.ActorSNO, CurrentCacheObject.Radius);
                            CacheData.NavigationObstacles.Add(new CacheObstacleObject
                            {
                                ActorSNO = CurrentCacheObject.ActorSNO,
                                Radius = CurrentCacheObject.Radius,
                                Position = CurrentCacheObject.Position,
                                RActorGUID = CurrentCacheObject.RActorGuid,
                                ObjectType = CurrentCacheObject.Type,
                            });

                            AddToCache = false;
                            c_IgnoreSubStep = "Invulnerable";
                            return AddToCache;
                        }
                        if (untargetable)
                        {
                            MainGridProvider.AddCellWeightingObstacle(CurrentCacheObject.ActorSNO, CurrentCacheObject.Radius);
                            CacheData.NavigationObstacles.Add(new CacheObstacleObject
                            {
                                ActorSNO = CurrentCacheObject.ActorSNO,
                                Radius = CurrentCacheObject.Radius,
                                Position = CurrentCacheObject.Position,
                                RActorGUID = CurrentCacheObject.RActorGuid,
                                ObjectType = CurrentCacheObject.Type,
                            });

                            AddToCache = false;
                            c_IgnoreSubStep = "Untargetable";
                            return AddToCache;
                        }


                        if (Player.ActorClass == ActorClass.Monk && Hotbar.Contains(SNOPower.Monk_TempestRush) && TimeSinceUse(SNOPower.Monk_TempestRush) <= 150)
                        {
                            AddToCache = false;
                            c_IgnoreSubStep = "MonkTR";
                            break;
                        }

                        if (Player.ActorClass == ActorClass.Monk && Hotbar.Contains(SNOPower.Monk_SweepingWind) && GetHasBuff(SNOPower.Monk_SweepingWind))
                        {
                            AddToCache = false;
                            c_IgnoreSubStep = "MonkSW";
                            break;
                        }

                        if (CurrentCacheObject.IsQuestMonster || CurrentCacheObject.IsMinimapActive)
                        {
                            AddToCache = true;
                            c_IgnoreSubStep = "";
                            break;
                        }

                        if (!DataDictionary.ForceDestructibles.Contains(CurrentCacheObject.ActorSNO) && Settings.WorldObject.DestructibleOption == DestructibleIgnoreOption.ForceIgnore)
                        {
                            AddToCache = false;
                            c_IgnoreSubStep = "ForceIgnoreDestructibles";
                            break;
                        }

                        if (DateTime.UtcNow.Subtract(PlayerMover.LastGeneratedStuckPosition).TotalSeconds <= 1)
                        {
                            AddToCache = true;
                            c_IgnoreSubStep = "";
                            break;
                        }

                        // Set min distance to user-defined setting
                        minDistance = Settings.WorldObject.DestructibleRange;
                        if (_forceCloseRangeTarget)
                            minDistance += 6f;

                        // Only break destructables if we're stuck and using IgnoreNonBlocking
                        if (Settings.WorldObject.DestructibleOption == DestructibleIgnoreOption.DestroyAll)
                        {
                            minDistance += 12f;
                            AddToCache = true;
                            c_IgnoreSubStep = "";
                        }

                        float maxRadiusDistance;

                        if (DataDictionary.DestructableObjectRadius.TryGetValue(CurrentCacheObject.ActorSNO, out maxRadiusDistance))
                        {
                            if (CurrentCacheObject.RadiusDistance < maxRadiusDistance)
                            {
                                AddToCache = true;
                                c_IgnoreSubStep = "";
                            }
                        }
                        // Always add large destructibles within ultra close range
                        if (!AddToCache && CurrentCacheObject.Radius >= 10f && CurrentCacheObject.RadiusDistance <= 5.9f)
                        {
                            AddToCache = true;
                            c_IgnoreSubStep = "";
                            break;
                        }

                        // This object isn't yet in our destructible desire range
                        if (AddToCache && (minDistance <= 1 || CurrentCacheObject.RadiusDistance > minDistance) && PlayerMover.GetMovementSpeed() >= 1)
                        {
                            AddToCache = false;
                            c_IgnoreSubStep = "NotInDestructableRange";
                        }
                        if (AddToCache && CurrentCacheObject.RadiusDistance <= 2f && DateTime.UtcNow.Subtract(PlayerMover.LastGeneratedStuckPosition).TotalMilliseconds > 500)
                        {
                            AddToCache = false;
                            c_IgnoreSubStep = "NotStuck2";
                        }

                        // Add if we're standing still and bumping into it
                        if (CurrentCacheObject.RadiusDistance <= 2f && PlayerMover.GetMovementSpeed() <= 0)
                        {
                            AddToCache = true;
                            c_IgnoreSubStep = "";
                        }

                        if (CurrentCacheObject.RActorGuid == LastTargetRactorGUID)
                        {
                            AddToCache = true;
                            c_IgnoreSubStep = "";
                        }

                        break;
                    }
                case GObjectType.CursedChest:
                case GObjectType.Container:
                    {
                        AddToCache = false;

                        bool isRareChest = CurrentCacheObject.InternalName.ToLower().Contains("chest_rare") || DataDictionary.ResplendentChestIds.Contains(CurrentCacheObject.ActorSNO);
                        bool isChest = (!isRareChest && CurrentCacheObject.InternalName.ToLower().Contains("chest")) ||
                            DataDictionary.ContainerWhiteListIds.Contains(CurrentCacheObject.ActorSNO); // We know it's a container but this is not a known rare chest
                        bool isCorpse = CurrentCacheObject.InternalName.ToLower().Contains("corpse");
                        bool isWeaponRack = CurrentCacheObject.InternalName.ToLower().Contains("rack");
                        bool isGroundClicky = CurrentCacheObject.InternalName.ToLower().Contains("ground_clicky");

                        // We want to do some vendoring, so don't open anything new yet
                        if (ForceVendorRunASAP)
                        {
                            AddToCache = false;
                            c_IgnoreSubStep = "ForceVendorRunASAP";
                        }

                        // Already open, blacklist it and don't look at it again
                        bool chestOpen;
                        try
                        {
                            chestOpen = (CurrentCacheObject.CommonData.GetAttribute<int>(ActorAttributeType.ChestOpen) > 0);
                        }
                        catch
                        {
                            Logger.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement, "Safely handled exception getting container-been-opened attribute for object {0} [{1}]", CurrentCacheObject.InternalName, CurrentCacheObject.ActorSNO);
                            c_IgnoreSubStep = "ChestOpenException";
                            AddToCache = false;
                            return AddToCache;
                        }

                        // Check if chest is open
                        if (chestOpen)
                        {
                            // It's already open!
                            AddToCache = false;
                            c_IgnoreSubStep = "AlreadyOpen";
                            return AddToCache;
                        }

                        if (untargetable)
                        {
                            MainGridProvider.AddCellWeightingObstacle(CurrentCacheObject.ActorSNO, CurrentCacheObject.Radius);
                            CacheData.NavigationObstacles.Add(new CacheObstacleObject
                            {
                                ActorSNO = CurrentCacheObject.ActorSNO,
                                Radius = CurrentCacheObject.Radius,
                                Position = CurrentCacheObject.Position,
                                RActorGUID = CurrentCacheObject.RActorGuid,
                                ObjectType = CurrentCacheObject.Type,
                            });

                            AddToCache = false;
                            c_IgnoreSubStep = "Untargetable";
                            return AddToCache;
                        }

                        // Resplendent chests have no range check
                        if (isRareChest && Settings.WorldObject.OpenRareChests)
                        {
                            AddToCache = true;
                            return AddToCache;
                        }

                        // Regular container, check range
                        if (CurrentCacheObject.RadiusDistance <= Settings.WorldObject.ContainerOpenRange)
                        {
                            if (isChest && Settings.WorldObject.OpenContainers)
                                return true;

                            if (isCorpse && Settings.WorldObject.InspectCorpses)
                                return true;

                            if (isGroundClicky && Settings.WorldObject.InspectGroundClicky)
                                return true;

                            if (isWeaponRack && Settings.WorldObject.InspectWeaponRacks)
                                return true;
                        }
                      
                        if (CurrentCacheObject.IsQuestMonster)
                        {
                            AddToCache = true;
                            return AddToCache;
                        }

                        if (Settings.WorldObject.OpenAnyContainer)
                        {
                            AddToCache = true;
                            return AddToCache;
                        }

                        if (!isChest && !isCorpse && !isRareChest)
                        {
                            c_IgnoreSubStep = "InvalidContainer";
                        }
                        else
                        {
                            c_IgnoreSubStep = "IgnoreContainer";
                        }
                        break;
                    }
            }
            return AddToCache;
        }
    }
}
