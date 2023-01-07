using System.Text.RegularExpressions;

namespace DaprAsbEmulator.Model;

public record Topic(string Name)
{
    // Source: Azure Portal name tooltip when creating a topic
    static readonly Regex NameValidationRegex = new("^[A-Za-z0-9]$|^[A-Za-z0-9][\\w-\\.\\/\\~]*[A-Za-z0-9]$", RegexOptions.Compiled);
    const int NameMinLength = 1;
    const int NameMaxLength = 260;
    
    public bool IsValidName() =>
        Name.Length is >= NameMinLength and <= NameMaxLength &&
        NameValidationRegex.IsMatch(Name);

    public virtual bool Equals(Topic? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return NameEqualityComparer.Instance.Equals(Name, other.Name);
    }
    
    public override int GetHashCode()
    {
        return Name.ToLowerInvariant().GetHashCode();
    }

    public class NameEqualityComparer : EqualityComparer<string>
    {
        static NameEqualityComparer? instance;
        public static NameEqualityComparer Instance => instance ??= new(); 
        
        public override bool Equals(string? x, string? y)
        {
            if (x == y)
            {
                return true;
            }

            if (x == null || y == null)
            {
                return false;
            }

            if (x.Equals(y, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return false;
        }

        public override int GetHashCode(string obj)
        {
            throw new NotImplementedException();
        }
    }
}