using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using Belphegor.Helpers;
using Zeta.Bot;
using Zeta.Bot.Profile;
using Zeta.Bot.Settings;
using Zeta.Common;
using Zeta.Game;
using Zeta.Game.Internals.Actors;
using Zeta.Game.Internals.SNO;

namespace Belphegor.Utilities
{
    public class BelphegorCombatTargetingProvider : ITargetingProvider
    {
        private static BelphegorCombatTargetingProvider _instance;

        internal static BelphegorCombatTargetingProvider Instance
        {
            get { return _instance ?? (_instance = new BelphegorCombatTargetingProvider()); }
        }

        #region Implementation of ITargetingProvider

        private static readonly Dictionary<int, MonsterType> MonsterTypeCache = new Dictionary<int, MonsterType>();
        private static readonly Dictionary<int, bool> IsEliteCache = new Dictionary<int, bool>();
        private static readonly Dictionary<int, MonsterSize> MonsterSizeCache = new Dictionary<int, MonsterSize>();

        private readonly HashSet<int> _summonerIds = new HashSet<int>
        {
            180734, // Siege_wallMonster_A_captainAmbush
            4303, // GoatMutant_Shaman_A
            4099, // FallenShaman_B
            4100, // FallenShaman_C
            365, // FallenShaman_D
            136090, // crater_HellPortal_Node_Monster
            4153, // FleshPitFlyerSpawner_B
            5387, // SkeletonSummoner_A
            6038, // TriuneSummoner_C
            5840, // trDun_Crypt_Pillar_Spawner
        };

        private readonly HashSet<int> _treasureGoblinIds = new HashSet<int>
        {
            5984, // Goblin A
            5985, // Goblin B
            5987, // Goblin C
            5988 // Goblin D
        };

        public List<DiaObject> GetObjectsByWeight()
        {
            if (ZetaDia.Me == null || !ProfileManager.CurrentProfile.KillMonsters ||
                !CombatTargeting.Instance.AllowedToKillMonsters)
                return new List<DiaObject>();

            var profileBlacklist = new HashSet<int>();

            if (ProfileManager.CurrentProfile.TargetBlacklists != null)
                profileBlacklist =
                    new HashSet<int>(ProfileManager.CurrentProfile.TargetBlacklists.Select(i => i.ActorId));

            List<TargetPriority> profileMultipliers = ProfileManager.CurrentProfile.TargetPriorities;

            List<Score> hostileUnits = ZetaDia.Actors.GetActorsOfType<DiaUnit>().
                Where(u => u.IsValid && u.Distance <= CharacterSettings.Instance.KillRadius &&
                           IsValidActor(u, profileBlacklist) && u.InLineOfSight).
                Select(
                    n => new Score
                    {
                        Obj = n,
                        Weight = 200 - (2*n.Distance)
                    }).ToList();

            foreach (Score s in hostileUnits)
                s.Weight = GetWeightForUnit(s.Obj, profileMultipliers);

            // Order by weight (descending), remove anything with < 0 weight. Then grab the TorObject version of it. :D
            return new List<DiaObject>(hostileUnits.OrderByDescending(s => s.Weight).Select(s => s.Obj).ToList());
        }

        /// <summary> Gets a weight for unit. </summary>
        /// <param name="unit"> The unit. </param>
        /// <param name="priorities"> </param>
        /// <returns> The weight for unit. </returns>
        private double GetWeightForUnit(DiaUnit unit, IEnumerable<TargetPriority> priorities)
        {
            double weight = 0d;

            weight += (int) GetMonsterSize(unit)*20;
            weight -= unit.HitpointsCurrentPct*100;
            weight += IsSummoner(unit) ? 100 : 0;
            weight += IsTreasureGoblin(unit) ? 200 : 0;
            weight += IsElite(unit.CommonData) ? 300 : 0;

            // Heavily score invulnerable units but don't remove them.
            weight -= unit.IsInvulnerable ? 10000 : 0;

            TargetPriority multiplier = priorities.FirstOrDefault(m => m.ActorId == unit.ActorSNO);
            if (multiplier != null)
                weight *= multiplier.Multiplier;

            return weight;
        }


        /// <summary> Query if 'unit' is a valid actor. </summary>
        /// <param name="unit">The unit. </param>
        /// <param name="blacklist"> The blacklist. </param>
        /// <returns> true if a valid actor, false if not. </returns>
        public bool IsValidActor(DiaUnit unit, HashSet<int> blacklist)
        {
            if (unit.ACDGuid == -1)
                return false;

            int actorSNO = unit.ActorSNO;

            // Blacklisted units
            if (blacklist.Contains(actorSNO))
                return false;

            if (!IsValidMonsterType(unit))
                return false;

            if (unit.ZDiff > 20f)
                return false;

            if (!unit.IsAttackable())
                return false;

            Vector3 raycastTo = unit.Position;
            Vector3 raycastFrom = ZetaDia.Me.Position;
            raycastFrom.Z += 2.0f;
            raycastTo.Z += 2.0f;

            return ZetaDia.Physics.Raycast(raycastFrom, raycastTo, NavCellFlags.AllowWalk);
        }

        /// <summary> Gets a monster type for the provided unit. </summary>
        /// <param name="unit"> The unit. </param>
        /// <returns> The monster type. </returns>
        private MonsterType GetMonsterType(DiaUnit unit)
        {
            int actorSNO = unit.ActorSNO;

            MonsterType type;
            if (!MonsterTypeCache.TryGetValue(actorSNO, out type))
            {
                SNORecordMonster monsterInfo = unit.MonsterInfo;
                if (monsterInfo != null)
                    type = monsterInfo.MonsterType;

                MonsterTypeCache.Add(actorSNO, type);
            }

            return type;
        }

        /// <summary> Querys if 'unit' has a valid monster type. </summary>
        /// <param name="u"> The DiaUnit to process. </param>
        /// <returns> true if valid monster type, false if not. </returns>
        private bool IsValidMonsterType(DiaUnit u)
        {
            MonsterType currentMt = GetMonsterType(u);
            return currentMt != MonsterType.Ally &&
                   currentMt != MonsterType.Scenery &&
                   currentMt != MonsterType.Helper &&
                   currentMt != MonsterType.Team;
        }

        /// <summary> Query if 'unit' is elite. </summary>
        /// <param name="unit"> The unit. </param>
        /// <returns> true if elite, false if not. </returns>
        private bool IsElite(ACD unit)
        {
            if (unit.IsValid)
            {
                int key = unit.DynamicId;

                bool isElite;
                if (IsEliteCache.TryGetValue(key, out isElite))
                    return isElite;


                MonsterAffixes affixes = unit.MonsterAffixes;
                IsEliteCache.Add(key,
                    affixes.HasFlag(MonsterAffixes.Elite) || affixes.HasFlag(MonsterAffixes.Rare) ||
                    affixes.HasFlag(MonsterAffixes.Unique)
                    );

                return IsEliteCache[key];
            }

            return false;
        }

        private MonsterSize GetMonsterSize(DiaUnit unit)
        {
            int actorSNO = unit.ActorSNO;
            MonsterSize size;
            if (MonsterSizeCache.TryGetValue(actorSNO, out size))
                return size;

            SNORecordMonster monsterInfo = unit.MonsterInfo;
            if (monsterInfo != null)
                size = monsterInfo.MonsterSize;

            MonsterSizeCache.Add(actorSNO, size);
            return size;
        }

        /// <summary> Query if 'unit' is a treasure goblin. </summary>
        /// <param name="unit"> The DiaUnit to process. </param>
        /// <returns> true if it's a treasure goblin, false if not. </returns>
        private bool IsTreasureGoblin(DiaUnit unit)
        {
            return _treasureGoblinIds.Contains(unit.ActorSNO);
        }

        /// <summary> Query if 'unit' is a summoner. </summary>
        /// <param name="unit"> The unit. </param>
        /// <returns> true if a summoner, false if not. </returns>
        public bool IsSummoner(DiaUnit unit)
        {
            return _summonerIds.Contains(unit.ActorSNO);
        }

        private class Score
        {
            public DiaUnit Obj;
            public double Weight;
        }

        #endregion
    }
}