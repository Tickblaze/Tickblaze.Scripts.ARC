namespace Tickblaze.Scripts.Arc.Domain;

public interface IBoundable
{
    public int FromBarIndex => Boundary.FromBarIndex;

    public Rectangle Boundary { get; }
}
