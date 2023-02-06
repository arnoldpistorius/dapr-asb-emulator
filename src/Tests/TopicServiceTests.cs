using System.Reactive.Disposables;
using DaprAsbEmulator.Adapter.Memory;
using DaprAsbEmulator.Application;
using DaprAsbEmulator.Model;
using DaprAsbEmulator.Ports;
using DaprAsbEmulator.Ports.Exceptions;
using FluentAssertions;
using FluentAssertions.Extensions;
using Microsoft.Extensions.Options;
using Moq;

namespace Tests;

public class TopicServiceTests
{
    TopicService Service { get; }
    Mock<ITopicSubscriptionEvents> TopicSubscriptionEvents { get; } = new();

    Mock<IOptions<TopicRepositorySettings>> Settings { get; } = new();

    
    public TopicServiceTests()
    {
        Settings.Setup(x => x.Value).Returns(new TopicRepositorySettings
        {
            MaxDeliveryAttempts = 10
        });
        
        var repository = new InMemoryTopicRepository(Settings.Object);
        Service = new TopicService(repository, repository, TopicSubscriptionEvents.Object, new ValidatorService());
    }
    
    public static IEnumerable<object[]> ValidTopicNames()
    {
        yield return Name("SomeNiceName");
        yield return Name("SomeNiceNameWithD1g1t5");
        yield return Name("nice-name-with-dashes");
        yield return Name("nice/name/with/slashes");
        
        object[] Name(string name) =>
            new object[] { name };
    }

    public static IEnumerable<object[]> InvalidTopicNames()
    {
        yield return Name("@$#$%$%");
        yield return Name("/test");
        yield return Name(".HowAreYou");
        yield return Name("test/test/");
        
        object[] Name(string name) =>
            new object[] { name };
    }

    [Theory, MemberData(nameof(ValidTopicNames))]
    public async Task CreateTopic_ValidTopics_SuccessfullyCreated(string topicName)
    {
        Func<Task> createTopic = () => Service.CreateTopic(topicName);
        
        await createTopic.Should().NotThrowAsync();
    }
    
    [Theory, MemberData(nameof(InvalidTopicNames))]
    public async Task CreateTopic_InvalidTopics_RaisesArgumentException(string topicName)
    {
        Func<Task> createTopic = () => Service.CreateTopic(topicName);
        
        await createTopic.Should().ThrowExactlyAsync<ArgumentException>().WithParameterName("topicName");
    }

    [Fact]
    public async Task CreateTopic_Duplicate_RaisesException()
    {
        Func<Task> createTopic = () => Service.CreateTopic("a-topic-name");

        await createTopic.Should().NotThrowAsync();
        await createTopic
            .Should()
            .ThrowExactlyAsync<TopicAlreadyExistsException>()
            .Where(x => x.TopicName == "a-topic-name");
    }

    [Fact]
    public async Task PublishMessage_ValidMessage_Succeeds()
    {
        var topic = await Service.CreateTopic("aTopic");

        Func<Task> publishMessage = () => Service.PublishMessage(topic.Name, "some serialized message");

        await publishMessage.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Subscribe_ValidTopic_Succeeds()
    {
        var topicName = "aTopic";
        var subscriptionName = "aSubscription";
        var topic = await Service.CreateTopic(topicName);

        Func<Task> subscribe = () => Service.Subscribe(topic.Name, subscriptionName);

        await subscribe.Should().NotThrowAsync();
    }
    
    [Fact]
    public async Task Subscribe_InvalidTopic_Fails()
    {
        var topicName = "aTopic";
        var subscriptionName = "aSubscription";

        Func<Task> subscribe = () => Service.Subscribe(topicName, subscriptionName);

        await subscribe.Should().ThrowAsync<TopicNotFoundException>().Where(x => x.TopicName == topicName);
    }

    [Fact]
    public async Task Peek_ValidTopicSubscription_Succeeds()
    {
        var topicName = "aTopic";
        var subscriptionName = "aSubscription";
        await Service.CreateTopic(topicName);
        await Service.Subscribe(topicName, subscriptionName);
        using var tokenDisposable = CancellationTokenUtility.CreateDisposableCancellationToken(out _, out var token);
        
        Func<Task> peek = () => Service.Peek(topicName, subscriptionName, token);

        await peek.Should().NotCompleteWithinAsync(10.Milliseconds());
    }

    [Fact(Timeout = 1000)]
    public async Task Peek_InvalidTopic_RaisesException()
    {
        var topicName = "aTopic";
        var subscriptionName = "aSubscription";
        using var tokenDisposable = CancellationTokenUtility.CreateDisposableCancellationToken(out _, out var token);

        Func<Task> peek = () => Service.Peek(topicName, subscriptionName, token);

        await peek
            .Should()
            .ThrowExactlyAsync<TopicNotFoundException>()
            .Where(x => x.TopicName == topicName);
    }
    
    [Fact(Timeout = 1000)]
    public async Task Peek_InvalidSubscription_RaisesException()
    {
        var topicName = "aTopic";
        var subscriptionName = "aSubscription";
        var topic = await Service.CreateTopic(topicName);
        using var tokenDisposable = CancellationTokenUtility.CreateDisposableCancellationToken(out _, out var token);

        Func<Task> peek = () => Service.Peek(topic.Name, subscriptionName, token);

        await peek
            .Should()
            .ThrowExactlyAsync<TopicSubscriptionNotFoundException>()
            .Where(x => x.TopicName == topic.Name && x.SubscriptionName == subscriptionName);
    }

    [Fact(Timeout = 1000)]
    public async Task Peek_PublishedMessage_ReturnsMessage()
    {
        var topicName = "aTopic";
        var subscriptionName = "aSubscription";
        var message = "Hello World!";
        var topic = await Service.CreateTopic(topicName);
        var subscription = await Service.Subscribe(topic.Name, subscriptionName);
        using var tokenDisposable = CancellationTokenUtility.CreateDisposableCancellationToken(out _, out var token);
        await Service.PublishMessage(topic.Name, message);
        
        var peekMessage = await Service.Peek(topic.Name, subscription.SubscriptionName, token);

        peekMessage.Value.Should().BeEquivalentTo(message);
    }
    
    [Fact(Timeout = 1000)]
    public async Task Peek_PublishedMessageSecondTime_DoesntReturnMessage()
    {
        var topicName = "aTopic";
        var subscriptionName = "aSubscription";
        var message = "Hello World!";
        var topic = await Service.CreateTopic(topicName);
        var subscription = await Service.Subscribe(topic.Name, subscriptionName);
        using var tokenDisposable = CancellationTokenUtility.CreateDisposableCancellationToken(out _, out var token);
        await Service.PublishMessage(topic.Name, message);
        await Service.Peek(topic.Name, subscription.SubscriptionName, token);

        await Service.Awaiting(x => x.Peek(topic.Name, subscription.SubscriptionName, token))
            .Should()
            .NotCompleteWithinAsync(10.Milliseconds());
    }

    [Fact(Timeout = 1000)]
    public async Task FailMessage_PeekedMessage_MessageCanBePeekedAgain()
    {
        var topicName = "aTopic";
        var subscriptionName = "aSubscription";
        var message = "Hello World!";
        var topic = await Service.CreateTopic(topicName);
        var subscription = await Service.Subscribe(topic.Name, subscriptionName);
        using var tokenDisposable = CancellationTokenUtility.CreateDisposableCancellationToken(out _, out var token);
        await Service.PublishMessage(topic.Name, message);
        var peekedMessage = await Service.Peek(topic.Name, subscription.SubscriptionName, token);

        await Service.FailMessage(topic.Name, subscription.SubscriptionName, peekedMessage);

        var peekedMessage2 = await Service.Peek(topic.Name, subscription.SubscriptionName, token);
        peekedMessage2.Id.Should().Be(peekedMessage.Id);
        peekedMessage2.Value.Should().BeEquivalentTo(message);
    }

    [Fact(Timeout = 1000)]
    public async Task SucceedMessage_PeekedMessage_Succeeds()
    {
        var topicName = "aTopic";
        var subscriptionName = "aSubscription";
        var message = "Hello World!";
        var topic = await Service.CreateTopic(topicName);
        var subscription = await Service.Subscribe(topic.Name, subscriptionName);
        using var tokenDisposable = CancellationTokenUtility.CreateDisposableCancellationToken(out _, out var token);
        await Service.PublishMessage(topic.Name, message);
        var peekedMessage = await Service.Peek(topic.Name, subscription.SubscriptionName, token);

        await Service.Awaiting(x => x.SucceedMessage(topic.Name, subscription.SubscriptionName, peekedMessage))
            .Should()
            .NotThrowAsync();
    }

    [Fact]
    public async Task FailMessage_ForMaxDeliveryAttempts_RaisesDeadLetter()
    {
        var topicName = "aTopic";
        var subscriptionName = "aSubscription";
        var message = "Hello World!";
        var topic = await Service.CreateTopic(topicName);
        var subscription = await Service.Subscribe(topic.Name, subscriptionName);
        using var tokenDisposable = CancellationTokenUtility.CreateDisposableCancellationToken(out _, out var token);
        await Service.PublishMessage(topic.Name, message);

        Message? peekedMessage = null;

        for (int i = 0; i < Settings.Object.Value.MaxDeliveryAttempts; i++)
        {
            var newPeekedMessage = await Service.Peek(topic.Name, subscription.SubscriptionName, token);
            if (peekedMessage != null)
            {
                newPeekedMessage.Id.Should().Be(peekedMessage.Id);
            }

            peekedMessage = newPeekedMessage;
            await Service.FailMessage(topic.Name, subscription.SubscriptionName, peekedMessage);
        }

        peekedMessage.Should().NotBeNull();
        TopicSubscriptionEvents.Verify(x => x.OnDeadLetterMessage(peekedMessage!), Times.Once);
    }
}

public static class CancellationTokenUtility
{
    public static IDisposable CreateDisposableCancellationToken(out CancellationTokenSource cancellationTokenSource,
        out CancellationToken cancellationToken)
    {
        cancellationTokenSource = new CancellationTokenSource();
        cancellationToken = cancellationTokenSource.Token;
        return Disposable.Create(cancellationTokenSource, cts =>
        {
            cts.Cancel();
            cts.Dispose();
        });
    }
}