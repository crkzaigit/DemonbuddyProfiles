using System.Linq;
using Belphegor.Settings;
using Belphegor.Utilities;
using Zeta.Common;
using Zeta.Game;
using Zeta.Game.Internals.Actors;

namespace Belphegor.Helpers
{
    internal static class Clusters
    {
        internal static int EvaluateClusterSize(Vector3 inputPoint, PositionCache positions, float clusterRange)
        {
            using (
                new PerformanceLogger(BelphegorSettings.Instance.Debug.IsDebugClusterLoggingActive,
                    "EvaluateClusterSize"))
            {
                return
                    positions.CachedPositions.Count(
                        v => v.Position.DistanceSqr(inputPoint) < clusterRange*clusterRange);
            }
        }

        public static int GetFacingCount(float range, float angle, CombatContext context)
        {
            using (
                new PerformanceLogger(BelphegorSettings.Instance.Debug.IsDebugClusterLoggingActive, "GetFacingCount"))
            {
                if (BelphegorSettings.Instance.EnableClusterCounts)
                    return
                        context.UnitPositions.CachedPositions.Count(
                            p =>
                                ZetaDia.Me.Position.Distance(p.Position) <= range &&
                                ZetaDia.Me.IsFacing(p.Position, angle));
                return -1;
            }
        }

        public static int GetClusterCount(DiaUnit target, CombatContext context, float clusterRange)
        {
            using (
                new PerformanceLogger(BelphegorSettings.Instance.Debug.IsDebugClusterLoggingActive, "GetClusterCount"))
            {
                int count;
                if (BelphegorSettings.Instance.EnableClusterCounts)
                    count = EvaluateClusterSize(target.Position, context.UnitPositions, clusterRange);

                else count = -1;
                return count;
            }
        }

        public static Vector3 GetBestPositionForClusters(CombatContext context, float clusterRange)
        {
            using (
                new PerformanceLogger(BelphegorSettings.Instance.Debug.IsDebugClusterLoggingActive,
                    "GetBestPositionForClusters"))
            {
                return
                    context.UnitPositions.CachedPositions.Select(
                        p =>
                            new
                            {
                                p.Position,
                                Count = EvaluateClusterSize(p.Position, context.UnitPositions, clusterRange)
                            })
                        .OrderByDescending(e => e.Count)
                        .Select(e => e.Position)
                        .FirstOrDefault();
            }
        }

        public static DiaObject GetBestUnitForCluster(CombatContext context,
            float clusterRange)
        {
            using (
                new PerformanceLogger(BelphegorSettings.Instance.Debug.IsDebugClusterLoggingActive,
                    "GetBestUnitForCluster"))
            {
                var firstOrDefault =
                    context.CachedUnits.Where(u => u.IsValid)
                        .Select(
                            u =>
                                new
                                {
                                    Unit = u,
                                    Count = EvaluateClusterSize(u.Position, context.UnitPositions, clusterRange)
                                })
                        .OrderByDescending(e => e.Count)
                        .FirstOrDefault();
                return firstOrDefault != null ? firstOrDefault.Unit : null;
            }
        }
    }
}