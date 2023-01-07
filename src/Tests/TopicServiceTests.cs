using DaprAsbEmulator.Adapter.Memory;
using DaprAsbEmulator.Application;
using DaprAsbEmulator.Application.Exceptions;
using DaprAsbEmulator.Model;
using DaprAsbEmulator.Ports;
using FluentAssertions;

namespace Tests;

public class TopicServiceTests
{
    [Fact]
    public async Task CreateTopic_NewTopic_TopicIsCreated()
    {
        // Arrange
        ITopicRepository topicRepository = new TopicRepository();
        ITopicService service = new TopicService(topicRepository);
        
        // Act
        var createdTopic = await service.CreateTopic("a-topic-name");

        // Assert
        var expectedTopic = new Topic("a-topic-name");
        createdTopic.Should().BeEquivalentTo(expectedTopic);
        
        var repositoryTopic = await topicRepository.GetTopic("a-topic-name");
        repositoryTopic.Should().BeEquivalentTo(expectedTopic);
    }

    [Fact]
    public async Task CreateTopic_ExistingTopic_TopicIsNotCreated()
    {
        // Arrange
        ITopicRepository topicRepository = new TopicRepository();
        ITopicService service = new TopicService(topicRepository);
        _ = await service.CreateTopic("a-topic-name");
        
        // Act/Assert
        await service.Awaiting(x => x.CreateTopic("a-topic-name"))
            .Should()
            .ThrowExactlyAsync<TopicAlreadyExistsException>("The topic is already created");
    }

    [Theory]
    [InlineData("foo@bar.com")]
    [InlineData("")]
    [InlineData("ATopicNameOfExactly261Characters0000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000")]
    [InlineData("Some#HashTag")]
    [InlineData("TopicNameWith$DollarSign")]
    [InlineData("TopicNameWith%Percentage")]
    [InlineData("I'mNotGoingToTestEveryPossibleCharacterIfYouDon'tMind")]
    [InlineData(".topicHasToStartWithLetterOrNumber")]
    [InlineData("-topicHasToStartWithLetterOrNumber")]
    [InlineData("/topicHasToStartWithLetterOrNumber")]
    [InlineData("_topicHasToStartWithLetterOrNumber")]
    [InlineData("topicHasToEndWithLetterOrNumber.")]
    [InlineData("topicHasToEndWithLetterOrNumber-")]
    [InlineData("topicHasToEndWithLetterOrNumber_")]
    [InlineData("topicHasToEndWithLetterOrNumber/")]
    [InlineData(".")]
    [InlineData("_")]
    [InlineData("-")]
    [InlineData("/")]
    public async Task CreateTopic_InvalidTopicName_TopicIsNotCreated(string topicName)
    {
        // Arrange
        ITopicRepository topicRepository = new TopicRepository();
        ITopicService service = new TopicService(topicRepository);
        
        // Act/Assert
        await service.Awaiting(x => x.CreateTopic(topicName))
            .Should()
            .ThrowExactlyAsync<TopicNameValidationFailedException>("The topic name is invalid");

        await topicRepository.Awaiting(x => x.GetTopic(topicName))
            .Should()
            .ThrowExactlyAsync<TopicNotFoundException>("The topic should not be created: name is invalid");
    }

    [Theory]
    [InlineData("a")]
    [InlineData("A")]
    [InlineData("ATopicNameOfExactly260Characters000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000")]
    [InlineData("you/can/use/slashes")]
    [InlineData("you-can-use-hyphens")]
    [InlineData("you.can.use.periods")]
    [InlineData("you_can_use_underscores")]
    [InlineData("you/can-use_it.all")]
    [InlineData("1NumberAtStartIsFine")]
    [InlineData("AllCharactersAllowed-abcdefghijklmnopqrstuvwxyz/ABCDEFGHIJKLMNOPQRSTUVWXYZ_0123456789")]
    public async Task CreateTopic_ValidTopicName_TopicIsCreated(string topicName)
    {
        // Arrange
        ITopicRepository topicRepository = new TopicRepository();
        ITopicService service = new TopicService(topicRepository);
        
        // Act/Assert
        await service.Awaiting(x => x.CreateTopic(topicName))
            .Should()
            .NotThrowAsync("The name is valid");
    }

    [Fact]
    public async Task RemoveTopic_ExistingTopic_TopicIsRemoved()
    {
        // Arrange
        ITopicRepository topicRepository = new TopicRepository();
        ITopicService service = new TopicService(topicRepository);
        _ = await service.CreateTopic("a-topic-name");
        
        // Act
        await service.RemoveTopic("a-topic-name");
        
        // Assert
        await topicRepository.Awaiting(x => x.GetTopic("a-topic-name"))
            .Should()
            .ThrowExactlyAsync<TopicNotFoundException>("The topic was removed");
    }

    [Fact]
    public async Task RemoveTopic_NonExistingTopic_ExceptionIsRaised()
    {
        // Arrange
        ITopicRepository topicRepository = new TopicRepository();
        ITopicService service = new TopicService(topicRepository);
        
        // Act/Assert
        await service.Awaiting(x => x.RemoveTopic("a-topic-name"))
            .Should()
            .ThrowExactlyAsync<TopicNotFoundException>();
    }
}