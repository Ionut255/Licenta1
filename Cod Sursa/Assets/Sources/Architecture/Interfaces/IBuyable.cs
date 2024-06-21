namespace Sources.Architecture.Interfaces
{
    public interface IBuyable
    {
        double CostValue { get; }
        IResource CostResource { get; }
        //IResource CostResource1 { get; }
        //IResource CostResource2 { get; }
    }
}