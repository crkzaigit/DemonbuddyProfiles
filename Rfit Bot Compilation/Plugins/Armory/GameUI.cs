using Zeta.Game.Internals;

namespace Armory
{
    class GameUI
    {
        private const ulong StashTab1Hash = 0x276522EDF3238841;

        public static UIElement StashTab1
        {
            get
            {
                return UIElement.FromHash(StashTab1Hash);
            }
        }

        public static bool ElementIsValid(UIElement element)
        {
            if (element == null)
                return false;
            if (!element.IsValid)
                return false;

            return true;
        }

        public static bool ElementIsVisible(UIElement element)
        {
            if (!ElementIsValid(element))
                return false;

            return element.IsVisible;
        }

        public static bool ElementIsEnabled(UIElement element)
        {
            if (!ElementIsVisible(element))
                return false;

            return element.IsEnabled;
        }
    }
}
