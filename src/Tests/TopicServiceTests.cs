using DaprAsbEmulator.Adapter.Memory;
using DaprAsbEmulator.Application;
using DaprAsbEmulator.Application.Exceptions;
using DaprAsbEmulator.Model;
using DaprAsbEmulator.Ports;
using FluentAssertions;

namespace Tests;

public class TopicServiceTests
{
    readonly ITopicRepository topicRepository;
    readonly ITopicService service;
    
    public TopicServiceTests()
    {
        topicRepository = new TopicRepository();
        service = new TopicService(topicRepository);
    }
    
    [Fact]
    public async Task CreateTopic_NewTopic_TopicIsCreated()
    {
        // Arrange
        
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
        await EnsureTopic("a-topic-name");
        
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
        
        // Act/Assert
        await service.Awaiting(x => x.CreateTopic(topicName))
            .Should()
            .NotThrowAsync("The name is valid");
    }

    [Fact]
    public async Task RemoveTopic_ExistingTopic_TopicIsRemoved()
    {
        // Arrange
        await EnsureTopic("a-topic-name");
        
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
        
        // Act/Assert
        await service.Awaiting(x => x.RemoveTopic("a-topic-name"))
            .Should()
            .ThrowExactlyAsync<TopicNotFoundException>();
    }

    [Fact]
    public async Task GetAllTopics_NoTopics_ReturnsEmptyArray()
    {
        // Arrange
        
        // Act
        var allTopics = await service.GetAllTopics();
        
        // Assert
        allTopics.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllTopics_ThreeTopics_ReturnsExactlyThreeTopics()
    {
        // Arrange
        var topic1 = await EnsureTopic("topic1");
        var topic2 = await EnsureTopic("topic2");
        var topic3 = await EnsureTopic("topic3");
        
        // Act
        var allTopics = await service.GetAllTopics();
        
        // Assert
        allTopics.Should().HaveCount(3).And.Contain(new[]
        {
            topic1,
            topic2,
            topic3
        });
    }

    [Fact]
    public async Task GetTopic_TopicExists_ReturnsTheTopic()
    {
        // Arrange
        var topic = await EnsureTopic("a-nice-topic");
        
        // Act
        var getTopic = await service.GetTopic(topic.Name);
        
        // Assert
        getTopic.Should().BeEquivalentTo(topic);
    }

    [Fact]
    public async Task GetTopic_TopicWithDifferentCasing_ReturnsTheTopic()
    {
        // Arrange
        var topic = await EnsureTopic("A-Nice-Topic");
        
        // Act
        var getTopic = await service.GetTopic("a-nice-topic");
        
        // Assert
        getTopic.Should().BeEquivalentTo(topic);
    }

    [Fact]
    public async Task PublishMessage_ExistingTopic_Succeeds()
    {
        // Arrange
        await EnsureTopic("a-topic");
        
        // Act/Assert
        await service.Awaiting(x => x.PublishMessage("a-topic", "message"))
            .Should()
            .NotThrowAsync();
    }

    [Fact]
    public async Task PublishMessage_NonExistingTopic_RaisesException()
    {
        // Arrange
        
        // Act/Assert
        await service.Awaiting(x => x.PublishMessage("a-topic", "message"))
            .Should()
            .ThrowAsync<TopicNotFoundException>();
    }

    [Fact]
    public async Task SubscribeTopic_ExistingTopic_Succeeds()
    {
        // Arrange
        await EnsureTopic("a-topic");
        
        // Act/Assert
        await service.Awaiting(x => x.SubscribeTopic("a-topic", "a-subscription"))
            .Should()
            .NotThrowAsync();
    }
    
    [Fact]
    public async Task SubscribeTopic_ExistingTopic_RaisesException()
    {
        // Arrange
        
        // Act/Assert
        await service.Awaiting(x => x.SubscribeTopic("a-topic", "a-subscription"))
            .Should()
            .ThrowAsync<TopicNotFoundException>();
    }

    [Fact]
    public async Task SubscribeTopic_ExistingSubscription_RaisesException()
    {
        // Arrange
        await EnsureTopicSubscription("a-topic", "a-subscription");
        
        // Act/Assert
        await service.Awaiting(x => x.SubscribeTopic("a-topic", "a-subscription"))
            .Should()
            .ThrowAsync<TopicSubscriptionAlreadyExistsException>();
    }

    async Task<Topic> EnsureTopic(string topicName)
    {
        try
        {
            return await service.CreateTopic(topicName);
        }
        catch (TopicAlreadyExistsException)
        {
            return await service.GetTopic(topicName);
        }
    }

    async Task EnsureTopicSubscription(string topicName, string subscriptionName)
    {
        await EnsureTopic(topicName);
        try
        {
            await service.SubscribeTopic(topicName, subscriptionName);
        }
        catch (TopicSubscriptionAlreadyExistsException)
        {
        }
    }
}