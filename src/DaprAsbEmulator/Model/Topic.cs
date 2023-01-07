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
}