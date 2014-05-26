using System;
using Armory.Hero;
using Zeta.Game;

namespace Armory
{
    internal class Armory
    {
        public static Version PluginVersion = new Version(1, 2, 10);

        private static HeroClass _activeHero;
        public static HeroClass ActiveHero
        {
            get { return _activeHero ?? (_activeHero = Armory.GetActiveHero()); }
        }

        private static HeroClass GetActiveHero()
        {
            if (!Player.IsValid)
                return null;

            switch (ZetaDia.Me.ActorClass)
            {
                case ActorClass.Barbarian:
                    return new Barbarian();

                case ActorClass.Crusader:
                    return new Crusader();

                case ActorClass.DemonHunter:
                    return new DemonHunter();

                case ActorClass.Monk:
                    return new Monk();

                case ActorClass.Witchdoctor:
                    return new WitchDoctor();

                case ActorClass.Wizard:
                    return new Wizard();

                default:
                    return null;
            }
        }
    }
}