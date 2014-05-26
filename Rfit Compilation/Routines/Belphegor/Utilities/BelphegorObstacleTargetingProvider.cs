using System.Collections.Generic;
using System.Linq;
using Zeta.Bot;
using Zeta.Game;
using Zeta.Game.Internals.Actors;
using Zeta.Game.Internals.Actors.Gizmos;
using Zeta.Game.Internals.SNO;

namespace Belphegor.Utilities
{
    /// <summary> Default obstacle targeting provider. </summary>
    public class BelphegorObstacleTargetingProvider : ITargetingProvider
    {
        private static BelphegorObstacleTargetingProvider _instance;

        internal static BelphegorObstacleTargetingProvider Instance
        {
            get { return _instance ?? (_instance = new BelphegorObstacleTargetingProvider()); }
        }

        #region Implementation of ITargetingProvider

        private readonly HashSet<int> _doorBlacklist = new HashSet<int>
        {
            200371, // CaOut_Target_Dummy
            167185, // trOut_Cultists_Summoning_Portal_B
            200832, // a3dun_Keep_Door_Wooden_Charred
            100849, // a1dun_caves_Neph_WaterBridge_A
            201680, // a3dun_crater_st_Demon_ChainPylon_Fire_MistressOfPain
            206461, // trDun_Cave_SwordOfJustice_Shard
            198977, // a3dun_crater_st_Demon_ChainPylon_Fire_Azmodan
            90419, // trOut_NewTristram_Gate_Town
            188577 // a4dun_spire_Sigil_Door_Fate
        };

        private readonly Dictionary<int, float> _longRangeDestructibles = new Dictionary<int, float>
        {
            {211456, 7}, // a3dun_Keep_Barrel_Snow_No_Skirt
            {92529, 20}, // caOut_Breakable_Wagon_b
            {340, 20}, // caOut_Bone_Cairn
            {214396, 20}, // a3Battlefield_Props_burnt_supply_wagon_B_Breakable
            {2972, 10}, // a2dun_Zolt_Breakable_Pillar_A
            {80357, 16}, // trOut_Highlands_LogStack_Trap
            {116508, 10}, // trOut_Log_Highlands
            {113932, 8}, // Trout_Log
            {197514, 18}, // a2dun_Zolt_Random_Breakable_Table_Sand
            {108587, 8}, // a3dun_Keep_Crate_B_Snow
            {108618, 8}, // a3dun_Keep_Barrel_Breakable_Snow
            {108612, 8}, // a3dun_Keep_Crate_E_Snow
            {116409, 18}, // a3dun_Bridge_Munitions_Cart_A
            {121586, 22}, // A3_Battlefield_Wagon_SupplyCart_A_Breakable
            {195101, 10}, // Barricade_Breakable_Snow_A
            {195108, 25}, // Barricade_Doube_Breakable_Snow_A
            {170657, 5}, // A3_Battlefield_Cart_A_Breakable
            {181228, 10}, // caOut_StingingWinds_Barricade_A
            {211959, 25}, // a1dun_Random_Mushroom_Cluster_B
            {210418, 25}, // a1dun_Random_Present_A
            {174496, 4}, // A3_crater_st_DemonCage_A
            {193963, 5}, // a3_Battlefield_Barricade_Breakable_charred
            {159066, 12}, // a3dun_Bridge_Barricade_A
            {160570, 12}, // a3dun_Bridge_Barricade_D
            {55325, 5}, // a3dun_Keep_Door_Destructable
            {5718, 14}, // trDun_Cath_Barricade_A
            {5909, 10}, // trDun_Pew_01
            {5792, 8}, // trDun_Cath_WoodDoor_A_Barricaded
            {108194, 8}, // a2dunSwr_Breakables_Barricade_B
            {129031, 30}, // Catapult_a3dunKeep_WarMachines_Breakable
            {192867, 3.5f}, // a3dun_crater_BonePile
            {155255, 8}, // a3dun_crater_st_Demon_BloodContainer_A
            {54530, 6}, // a3dun_Keep_Crane_Clickable
            {157541, 6}, // trDun_Blacksmith_CellarDoor_Breakable
            {93306, 10}, // caOut_Breakable_Wagon_C
            {210120, 27}, //a4dun_Garden_Corruption_Monster
        };

        private readonly Dictionary<int, float> _longRangeDoors = new Dictionary<int, float>
        {
            {54850, 20f}, // a3dun_Keep_SiegeTowerDoor_A
            {5766, 20f}, // trDun_Cath_Gate_C
            {5767, 20f}, // trDun_Cath_Gate_D
        };

        private readonly HashSet<int> _obstacleBlacklist = new HashSet<int>
        {
            /* Act I */
            5899, // trDun_Magic_Painting_E_NoSpawn
            89503, // trDun_Cath_Braizer_Trap
            5744, // trDun_Cath_Chandelier_Trap
            5717, // trDun_Cath_Bannister_x6

            86400, // trOut_Wilderness_Planter_A
            86428, // trOut_Wilderness_Planter_B
            81699, // trOut_Wagon_Barricade
            86266, // trOut_Wilderness_Gargoyle_A
            86400, // trOut_Wilderness_Planter_A
            6155, // OldTristramTombstoneDestructibleA
            6156, // OldTristramTombstoneDestructibleB
            6157, // OldTristramTombstoneDestructibleC
            6158, // OldTristramTombstoneDestructibleD
            60665, // WoodFenceE_Fields_trOut
            60844, // WoodFenceC_Fields_trOut
            110769, // trDun_Cath_Barrel_Common_NoSkel
            78554, // Hen_House_trOut_Farms

            /* Act III */
            56416, // a3dun_Keep_BarrelRings_A_Breakable
            53957, // a3dun_Keep_BucketMetal_A_Breakable
            141639, // a3dun_Keep_Exploding_Arch_A

            /* Client Effects */
            85816, // healthGlobe_swipe
            159626, // barbarian_frenzyRune_duration_swipe
        };


        /// <summary> Gets the objects by weight. </summary>
        /// <returns> The objects by weight. </returns>
        public List<DiaObject> GetObjectsByWeight()
        {
            if (ZetaDia.Me == null)
                return new List<DiaObject>();

            List<Score> ostacles =
                ZetaDia.Actors.GetActorsOfType<DiaObject>(true).Where(IsValidObstacle).
                    Select(
                        n =>
                        {
                            double weight = -(n.Distance*100) + 500;

                            return
                                new Score
                                {
                                    Obj = n,
                                    Weight = weight
                                };
                        }).ToList();

            // Order by weight (descending), remove anything with < 0 weight. Then grab the TorObject version of it. :D
            return new List<DiaObject>(ostacles.OrderByDescending(s => s.Weight).Select(s => s.Obj).ToList());
        }

        /// <summary> Query if '@object' is valid obstacle. </summary>
        /// <param name="object"> The object. </param>
        /// <returns> true if valid obstacle, false if not. </returns>
        public bool IsValidObstacle(DiaObject @object)
        {
            int actorId = @object.ActorSNO;
            float distance = @object.Distance;

            if (Blacklist.Contains(@object))
                return false;

            if (@object.ActorType == ActorType.ClientEffect)
                return false;

            float baseRange = 5f;
            if (@object.ActorInfo.GizmoType == GizmoType.BreakableDoor)
                baseRange = 10f;

            // This is for destructibles that is rather big and can't go by the default 10 yrds rule.
            float range = baseRange + @object.CollisionSphere.Radius/2;
            if (_longRangeDestructibles.ContainsKey(actorId))
                range = _longRangeDestructibles[actorId];

            if (_longRangeDoors.ContainsKey(actorId))
                range = _longRangeDoors[actorId];

            if (distance > range)
                return false;

            var door = @object as GizmoDoor;
            if (door != null)
            {
                return !_doorBlacklist.Contains(actorId);
            }

            // Destructible loot containers etc.
            var destructible = @object as GizmoDestructible;
            if (destructible != null)
            {
                return !_obstacleBlacklist.Contains(actorId);
            }

            return false;
        }

        #endregion
    }

    internal class Score
    {
        public DiaObject Obj;
        public double Weight;
    }
}