using System;
using System.Collections.Generic;
using System.Linq;
using Belphegor.Settings;
using Belphegor.Utilities;
using Zeta.Bot;
using Zeta.Common;
using Zeta.Game;
using Zeta.Game.Internals.Actors;

namespace Belphegor.Helpers
{
    internal class CombatContext
    {
        private readonly Func<CombatContext, DiaUnit> _targetReceiver;
        private PointCollisionTree _aoePositions;
        private PositionCache _avoidanceCollisionPositions;
        private IEnumerable<DiaUnit> _cacheDiaUnits;
        private double? _currentHealthPercentage;
        private DiaObject _currentLootTarget;
        private DiaUnit _currentTarget;
        private bool? _currentTargetElite;
        private bool? _isCollidingWithAoe;
        private bool? _isPlayerFearedStunnedForzenOrBlind;
        private bool? _isPlayerIncapacited;
        private Vector3? _playerPosition;
        private float? _targetDistance;
        private int? _targetGuid;
        private Vector3? _targetPosition;
        private PositionCache _unitPositions;
        private CachedValue<Vector3> _whirlwindCache;

        internal CombatContext(Func<CombatContext, DiaUnit> targetReceiver)
        {
            // Blargh
            _targetReceiver = targetReceiver;
        }

        internal IEnumerable<DiaUnit> CachedUnits
        {
            get { return _cacheDiaUnits ?? (_cacheDiaUnits = ZetaDia.Actors.GetActorsOfType<DiaUnit>(true)); }
        }

        internal DiaUnit CurrentTarget
        {
            get { return _currentTarget ?? (_currentTarget = _targetReceiver(this)); }
        }

        internal bool CurrentTargetIsElite
        {
            get { return _currentTargetElite ?? (_currentTargetElite = CurrentTarget.IsElite()).Value; }
        }

        internal DiaObject CurrentLootTarget
        {
            get { return _currentLootTarget ?? (_currentLootTarget = LootTargeting.Instance.FirstObject); }
        }

        internal Vector3 PlayerPosition
        {
            get { return (_playerPosition ?? (_playerPosition = ZetaDia.Me.Position)).Value; }
        }

        internal PointCollisionTree AoePositions
        {
            get
            {
                if (_aoePositions == null)
                {
                    _aoePositions = new PointCollisionTree();
                    IEnumerable<PointCollisionTree.PointNode> nodes =
                        CachedUnits
                            .Where(u => u.IsValid && u.IsAoe() && u.GetTriggerHealthPct() >= CurrentHealthPercentage)
                            .Select(u => new PointCollisionTree.PointNode(u.Position, u.GetCollisionRadius()));
                    nodes.ForEach(p => _aoePositions.AddPoint(p));
                }
                return _aoePositions;
            }
        }

        internal PositionCache UnitPositions
        {
            get
            {
                return _unitPositions ?? (_unitPositions =
                    new PositionCache(
                        CachedUnits
                            .Where(
                                u =>
                                    u.IsValid && u.IsAttackable() && u.IsACDBased &&
                                    !u.IsDead)
                            .Select(
                                u =>
                                    new PositionCache.CachedPosition
                                    {
                                        Position = u.Position,
                                        Radius = u.CollisionSphere.Radius
                                    })));
            }
        }

        internal PositionCache AvoidanceCollisionPositions
        {
            get
            {
                return _avoidanceCollisionPositions ?? (_avoidanceCollisionPositions =
                    new PositionCache(
                        UnitPositions.CachedPositions.Where(p => p.Position.DistanceSqr(PlayerPosition) > 2.5*2.5)));
            }
        }

        internal double CurrentHealthPercentage
        {
            get
            {
                return _currentHealthPercentage ?? (_currentHealthPercentage = ZetaDia.Me.HitpointsCurrentPct).Value;
            }
        }

        internal bool IsCollidingWithAoe
        {
            get
            {
                return (_isCollidingWithAoe ?? (_isCollidingWithAoe = PlayerPosition.IsCollidingWithAoe(this))).Value;
            }
        }

        internal Vector3 TargetPosition
        {
            get
            {
                return
                    (_targetPosition ??
                     (_targetPosition =
                         CurrentTarget != null
                             ? CurrentTarget.Position
                             : CurrentLootTarget != null ? CurrentLootTarget.Position : Vector3.Zero)).Value;
            }
        }

        internal int TargetGuid
        {
            get { return (_targetGuid ?? (_targetGuid = CurrentTarget != null ? CurrentTarget.ACDGuid : -1)).Value; }
        }

        internal float TargetDistance
        {
            get
            {
                return
                    (_targetDistance ??
                     (_targetDistance =
                         CurrentTarget != null
                             ? CurrentTarget.Distance - CurrentTarget.GetMonsterDistanceModifier()
                             : CurrentLootTarget != null ? CurrentLootTarget.Distance : float.MaxValue)).Value;
            }
        }

        internal bool IsPlayerIncapacited
        {
            get { return (_isPlayerIncapacited ?? (_isPlayerIncapacited = Unit.IsMeIncapacited)).Value; }
        }

        internal bool IsPlayerFearedStunnedFrozenOrBlind
        {
            get
            {
                return
                    (_isPlayerFearedStunnedForzenOrBlind ??
                     (_isPlayerFearedStunnedForzenOrBlind = Unit.IsMeFearedStunnedFrozenOrBlind)).Value;
            }
        }

        /// <summary> Gets the whirlwind target position. </summary>
        /// <value> The whirl wind target position. </value>
        internal Vector3 WhirlWindTargetPosition
        {
            get
            {
                CachedValue<Vector3> cachedValue = _whirlwindCache ??
                                                   (_whirlwindCache =
                                                       new CachedValue<Vector3>(GetWhirlwindPosition,
                                                           TimeSpan.FromMilliseconds(100)));
                return cachedValue.Value;
            }
        }

        private Vector3 GetWhirlwindPosition()
        {
            Vector3 pos = Clusters.GetBestPositionForClusters(this,
                BelphegorSettings.Instance.Barbarian.WhirlwindClusterRange);
            if (pos == Vector3.Zero)
                pos = TargetPosition;

            float dir = 0.0f;
            switch (new Random().Next(1, 5))
            {
                case 1:
                    dir = MathEx.ToRadians(0f);
                    break;
                case 2:
                    dir = MathEx.ToRadians(90f);
                    break;
                case 3:
                    dir = MathEx.ToRadians(180f);
                    break;
                case 4:
                    dir = MathEx.ToRadians(270f);
                    break;
            }

            Vector3 wwPoint = MathEx.GetPointAt(pos, 2f, dir);
            return wwPoint;
        }
    }
}