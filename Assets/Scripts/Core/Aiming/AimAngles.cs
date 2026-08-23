namespace Game.Core.Aiming
{
    public readonly struct AimAngles
    {
        public readonly float Yaw;
        public readonly float Elevation;

        public AimAngles(float yaw, float elevation)
        {
            Yaw = yaw;
            Elevation = elevation;
        }
    }
}
