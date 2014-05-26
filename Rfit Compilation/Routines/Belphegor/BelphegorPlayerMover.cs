using System;
using log4net;
using Zeta.Bot;
using Zeta.Bot.Navigation;
using Zeta.Common;
using Zeta.Common.Helpers;
using Zeta.Game;
using Zeta.TreeSharp;

namespace Belphegor
{
    public class BelphegorPlayerMover : IPlayerMover
    {
        private static readonly ILog Log = Logger.GetLoggerInstanceForType();

        #region Implementation of IPlayerMover

        public void MoveTowards(Vector3 to)
        {
            var belphegor = RoutineManager.Current as Belphegor;
            if (belphegor != null)
            {
                Composite movementProvider = belphegor.Movement;
                if (movementProvider != null)
                {
                    if(!movementProvider.IsRunning)
                        movementProvider.Start(to);
                    movementProvider.Tick(to);
                    if(movementProvider.IsRunning)
                        movementProvider.Stop(to);
                    return;
                }
            }
            ZetaDia.Me.Movement.MoveActor(to);
        }

        public void MoveStop()
        {
            ZetaDia.Me.Movement.MoveActor(ZetaDia.Me.Position);
        }

        #endregion

        #region Timers

        private static readonly WaitTimer LagTimer = new WaitTimer(TimeSpan.FromSeconds(1));

        static BelphegorPlayerMover()
        {
            GameEvents.OnGameLeft += ResetTimers;
            GameEvents.OnPlayerDied += ResetTimers;
        }

        private static void ResetTimers(object sender, EventArgs e)
        {
            LagTimer.Stop();
        }

        #endregion
    }
}