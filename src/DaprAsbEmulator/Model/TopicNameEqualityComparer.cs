namespace DaprAsbEmulator.Model;

public class TopicNameEqualityComparer : IEqualityComparer<string>
{
    static TopicNameEqualityComparer? instance;

    public static TopicNameEqualityComparer Instance => instance ??= new();

    // Force to use Instance
    TopicNameEqualityComparer() {}

    public bool Equals(string? x, string? y)
    {
        return x == y ||
               (x != null && x.Equals(y, StringComparison.OrdinalIgnoreCase));
    }

    public int GetHashCode(string name)
    {
        return HashCode.Combine(name);
    }
}