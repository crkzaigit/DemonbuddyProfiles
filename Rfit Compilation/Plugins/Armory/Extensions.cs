using Zeta.Game.Internals.Actors;

namespace Armory
{
    public static class Extensions
    {
        public static float GetRadiusDistance(this DiaObject diaObject, float reduction = 0f)
        {
            if (diaObject != null && diaObject.IsValid)
            {
                return diaObject.Distance - diaObject.CollisionSphere.Radius - reduction;
            }
            return reduction;
        }

        public static float GetRadiusDistanceSqr(this DiaObject diaObject, float reduction = 0f)
        {
            if (diaObject != null && diaObject.IsValid)
            {
                var distance = GetRadiusDistance(diaObject, reduction);
                return distance * distance;
            }
            return reduction;
        }
    }
}
