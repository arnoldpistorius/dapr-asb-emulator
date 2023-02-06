using DaprAsbEmulator.Application;
using FluentAssertions;

namespace Tests;

public class ValidatorServiceTests
{
    readonly ValidatorService service;

    public ValidatorServiceTests()
    {
        service = new ValidatorService();
    }
    
    [Fact]
    public async Task IsValidTopicName_ValidTopicName_ReturnsTrue()
    {
        // Arrange
        var topicName = "some-serious-topic";

        // Act
        var isValidTopicNameResult = await service.IsValidTopicName(topicName);

        // Assert
        isValidTopicNameResult.Should().BeTrue();
    }

    [Fact]
    public async Task IsValidTopicName_InvalidTopicName_ReturnsFalse()
    {
        // Arrange
        var topicName = "some-serious-invalid-topic/";

        // Act
        var isValidTopicNameResult = await service.IsValidTopicName(topicName);

        // Assert
        isValidTopicNameResult.Should().BeFalse();
    }
}