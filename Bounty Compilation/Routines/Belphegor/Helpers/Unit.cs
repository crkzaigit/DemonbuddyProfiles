using System;
using System.Collections.Generic;
using System.Linq;
using Zeta.Bot;
using Zeta.Common;
using Zeta.Game;
using Zeta.Game.Internals.Actors;
using Zeta.Game.Internals.SNO;

namespace Belphegor.Helpers
{
    internal static class Unit
    {
        private static int _pulsesSinceLastReset;

        /// <summary>
        ///     List of all known treasure goblins
        /// </summary>
        private static readonly HashSet<int> TreasureGoblin = new HashSet<int>
        {
            5984, //Goblin A
            5985, //Goblin B
            5987, //Goblin C
            5988 //Goblin D
        };

        private static readonly Dictionary<int, bool> IsEliteCache = new Dictionary<int, bool>();
        private static readonly Dictionary<int, MonsterSize> MonsterSizeCache = new Dictionary<int, MonsterSize>();
        private static readonly Dictionary<int, float> MonsterDistanceModifierCache = new Dictionary<int, float>();

        static Unit()
        {
            Pulsator.OnPulse += Pulsator_OnPulse;
        }

        /// <summary>
        ///     Checks if we are Blind, Feared, Frozen, Stunned or Rooted
        /// </summary>
        public static bool IsMeIncapacited
        {
            get
            {
                DiaActivePlayer me = ZetaDia.Me;
                return me.IsFeared || me.IsStunned || me.IsFrozen || me.IsBlind || me.IsRooted;
            }
        }

        /// <summary>
        ///     Checks if we are Blind, Feared, Frozen or Stunned
        /// </summary>
        public static bool IsMeFearedStunnedFrozenOrBlind
        {
            get
            {
                DiaActivePlayer me = ZetaDia.Me;
                return me.IsFeared || me.IsStunned || me.IsFrozen || me.IsBlind;
            }
        }

        /// <summary> Query if 'acd' is valid a ACD. </summary>
        /// <param name="unit"> The unit. </param>
        /// <returns> true if valid acd, false if not. </returns>
        public static bool IsAttackable(this DiaUnit unit)
        {
            ACD acd = unit.CommonData;
            if (!unit.IsValid || acd == null || unit.ACDGuid == -1)
                return false;

            if (unit.IsFriendly)
                return false;

            AnimationState animationState = GetAnimationStateForACD(acd);
            if (animationState == AnimationState.Dead)
                return false;

            bool isAttackble = !unit.IsUntargetable &&
                               !unit.IsSlowdownImmune &&
                               !unit.IsStunImmune &&
                               !unit.IsUninterruptible &&
                               !unit.IsRootImmune &&
                               !unit.IsBurrowed &&
                               !unit.IsHidden &&
                               !unit.IsDead;

            return isAttackble;
        }


        private static AnimationState GetAnimationStateForACD(ACD acd)
        {
            if (acd != null && acd.AnimationInfo != null)
                return acd.AnimationInfo.State;

            return AnimationState.Invalid;
        }

        private static void Pulsator_OnPulse(object sender, EventArgs e)
        {
            if (_pulsesSinceLastReset == 100)
            {
                _pulsesSinceLastReset = 0;
                IsEliteCache.Clear();
            }
            else
            {
                _pulsesSinceLastReset++;
            }
        }

        /// <summary>
        ///     Checks if the mob is Elite or Rare
        /// </summary>
        /// <param name="unit">DiaUnit</param>
        /// <returns>True if Current unit is Elite</returns>
        public static bool IsElite(this DiaUnit unit)
        {
            ACD commonData = unit.CommonData;
            if (!unit.IsValid || commonData == null) return false;
            int key = commonData.DynamicId;

            if (IsEliteCache.ContainsKey(key)) return IsEliteCache[key];
            MonsterAffixes affixes = commonData.MonsterAffixes;
            IsEliteCache.Add(key,
                affixes.HasFlag(MonsterAffixes.Elite) || affixes.HasFlag(MonsterAffixes.Rare) ||
                affixes.HasFlag(MonsterAffixes.Unique) || TreasureGoblin.Contains(unit.ActorSNO));
            return IsEliteCache[key];
        }

        /// <summary>
        ///     Checks if the mob is Elite or Rare && with range
        /// </summary>
        /// <param name="unit">DiaUnit</param>
        /// <param name="range">Range from us to unit</param>
        /// <returns>True if current uint is Elite && unit is in range</returns>
        public static bool IsElite(this DiaUnit unit, float range)
        {
            Vector3 myLoc = ZetaDia.Me.Position;
            return unit.IsElite() && unit.Position.DistanceSqr(myLoc) <= range*range;
        }


        public static bool IsEliteInRange(float range, CombatContext context)
        {
            return context.CachedUnits.Any(u => (u).IsElite(range));
        }

        public static MonsterSize GetMonsterSize(this DiaUnit u)
        {
            int sno = u.ActorSNO;
            if (!MonsterSizeCache.ContainsKey(sno))
                MonsterSizeCache.Add(sno, u.MonsterInfo.MonsterSize);
            return MonsterSizeCache[sno];
        }

        public static float GetMonsterDistanceModifier(this DiaUnit u)
        {
            int sno = u.ActorSNO;
            if (!MonsterDistanceModifierCache.ContainsKey(sno))
                MonsterDistanceModifierCache.Add(sno, u.CollisionSphere.Radius);
            return MonsterDistanceModifierCache[sno];
        }
    }
}