using System.Text.RegularExpressions;
using DaprAsbEmulator.Ports;

namespace DaprAsbEmulator.Application;

public sealed class ValidatorService : IValidatorService
{
    public Task<bool> IsValidTopicName(string topicName) =>
        Task.FromResult(TopicNameValidator.Validate(topicName));
}

file static class TopicNameValidator
{
    static readonly Regex NameValidationRegex = new("^[A-Za-z0-9]$|^[A-Za-z0-9][\\w-\\.\\/\\~]*[A-Za-z0-9]$", RegexOptions.Compiled);
    const int NameMinLength = 1;
    const int NameMaxLength = 260;

    public static bool Validate(string name)
    {
        return name.Length is >= NameMinLength and <= NameMaxLength &&
               NameValidationRegex.IsMatch(name);
    }
}