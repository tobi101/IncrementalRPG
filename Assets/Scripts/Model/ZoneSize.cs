namespace Model
{
    public struct ZoneSize
    {
        public float RadiusX;
        public float RadiusY;
        
        public static ZoneSize Default => new ZoneSize { RadiusX = 0.6f, RadiusY = 0.33f };
    }
}