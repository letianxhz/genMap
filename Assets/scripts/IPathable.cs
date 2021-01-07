namespace d3_delaunay_cs
{
    public interface IPathable<T>
    {
        void moveTo(double x, double y);
        void closePath();
        void lineTo(double x, double y);
        T value();
    }
}