using System.Reactive.Linq;
using DaprAsbEmulator.Adapter.Grpc;
using FluentAssertions;
using FluentAssertions.Extensions;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Mvc.Testing;
using Nito.Disposables;

namespace Tests.Integration;

public class TopicGrpcTests
{
    Topics.TopicsClient client;

    public TopicGrpcTests()
    {
        var factory = new WebApplicationFactory<Program>();
        var grpcOptions = new GrpcChannelOptions
        {
            HttpHandler = factory.Server.CreateHandler()
        };
        var channel = GrpcChannel.ForAddress(factory.Server.BaseAddress, grpcOptions);
        client = new(channel);
    }

    [Fact]
    public async Task CreateTopic_NonExistingTopic_CreatesTopic()
    {
        // Arrange
        var topicName = "test-topic";

        // Act
        var response = await client.CreateTopicAsync(new()
        {
            TopicName = topicName
        });

        // Assert
        response.TopicName.Should().BeEquivalentTo(topicName);
    }

    [Fact]
    public async Task CreateTopic_ExistingTopic_ThrowsException()
    {
        // Arrange
        var topicName = "test-topic";
        await client.CreateTopicAsync(new()
        {
            TopicName = topicName
        });

        // Act
        Func<Task> act = async () => await client.CreateTopicAsync(new()
        {
            TopicName = topicName
        });

        // Assert
        await act.Should().ThrowAsync<RpcException>();
    }

    [Fact]
    public async Task SubscribeTopic_NonExistingSubscription_SubscribesTopic()
    {
        // Arrange
        var topicName = "test-topic";
        var subscriptionName = "test-subscription";
        await client.CreateTopicAsync(new()
        {
            TopicName = topicName
        });

        // Act
        var response = await client.SubscribeTopicAsync(new()
        {
            TopicName = topicName,
            SubscriptionName = subscriptionName
        });

        // Assert
        response.TopicName.Should().BeEquivalentTo(topicName);
        response.SubscriptionName.Should().BeEquivalentTo(subscriptionName);
    }

    [Fact]
    public async Task SubscribeTopic_ExistingSubscription_ThrowsException()
    {
        // Arrange
        var topicName = "test-topic";
        var subscriptionName = "test-subscription";
        await client.CreateTopicAsync(new()
        {
            TopicName = topicName
        });
        await client.SubscribeTopicAsync(new()
        {
            TopicName = topicName,
            SubscriptionName = subscriptionName
        });

        // Act
        Func<Task> act = async () => await client.SubscribeTopicAsync(new()
        {
            TopicName = topicName,
            SubscriptionName = subscriptionName
        });

        // Assert
        await act.Should().ThrowAsync<RpcException>();
    }

    [Fact]
    public async Task Publish_ExistingTopic_MessageIsPublished()
    {
        // Arrange
        var topicName = "test-topic";
        var subscriptionName = "test-subscription";
        var testMessage = "This is a test message.";

        await client.CreateTopicAsync(new()
        {
            TopicName = topicName
        });
        await client.SubscribeTopicAsync(new()
        {
            TopicName = topicName,
            SubscriptionName = subscriptionName
        });

        // Act
        var response = await client.PublishMessageAsync(new()
        {
            TopicName = topicName,
            Message = testMessage
        });

        // Assert
        response.Should().NotBeNull();
    }

    [Fact]
    public async Task Publish_NonExistingTopic_ThrowsException()
    {
        // Arrange
        var topicName = "test-topic";
        var testMessage = "This is a test message.";

        // Act
        Func<Task> act = async () => await client.PublishMessageAsync(new()
        {
            TopicName = topicName,
            Message = testMessage
        });

        // Assert
        await act.Should().ThrowAsync<RpcException>();
    }

    [Fact]
    public async Task Peek_NoMessagePublished_NoMessageReceived()
    {
        // Arrange
        var topicName = "test-topic";
        var subscriptionName = "test-subscription";

        await client.CreateTopicAsync(new()
        {
            TopicName = topicName
        });
        await client.SubscribeTopicAsync(new()
        {
            TopicName = topicName,
            SubscriptionName = subscriptionName
        });

        using var cancellationTokenSource = new CancellationTokenSource(1.Seconds());
        // Act
        var stream = client.PeekMessage(new PeekMessageRequest()
        {
            SubscriptionName = subscriptionName,
            TopicName = topicName
        });

        int count = 0;

        try
        {
            await foreach (var message in stream.ResponseStream.ReadAllAsync(cancellationTokenSource.Token))
            {
                count++;
            }
        }
        catch (RpcException e) when(e.StatusCode == StatusCode.Cancelled)
        {
            // doe
        }

        // Assert
        count.Should().Be(0);
    }

    [Fact]
    public async Task Peek_MessagePublished_MessageReceived()
    {
        // Arrange
        var topicName = "test-topic";
        var subscriptionName = "test-subscription";
        var testMessage = "This is a test message.";

        await client.CreateTopicAsync(new()
        {
            TopicName = topicName
        });
        await client.SubscribeTopicAsync(new()
        {
            TopicName = topicName,
            SubscriptionName = subscriptionName
        });
        await client.PublishMessageAsync(new()
        {
            TopicName = topicName,
            Message = testMessage
        });

        using var cancellationTokenSource = new CancellationTokenSource(1.Seconds());
        // Act
        var stream = client.PeekMessage(new PeekMessageRequest()
        {
            SubscriptionName = subscriptionName,
            TopicName = topicName
        });

        int count = 0;

        try
        {
            await foreach (var message in stream.ResponseStream.ReadAllAsync(cancellationTokenSource.Token))
            {
                count++;
            }
        }
        catch (RpcException e) when(e.StatusCode == StatusCode.Cancelled)
        {
            // doe
        }

        // Assert
        count.Should().Be(1);
    }

    [Fact]
    public async Task Peek_TwoMessagesPublished_TwoMessagesReceived()
    {
        // Arrange
        var topicName = "test-topic";
        var subscriptionName = "test-subscription";
        var testMessage = "This is a test message.";

        await client.CreateTopicAsync(new()
        {
            TopicName = topicName
        });
        await client.SubscribeTopicAsync(new()
        {
            TopicName = topicName,
            SubscriptionName = subscriptionName
        });
        await client.PublishMessageAsync(new()
        {
            TopicName = topicName,
            Message = testMessage
        });
        await client.PublishMessageAsync(new()
        {
            TopicName = topicName,
            Message = testMessage
        });

        using var cancellationTokenSource = new CancellationTokenSource(1.Seconds());
        // Act
        var stream = client.PeekMessage(new PeekMessageRequest()
        {
            SubscriptionName = subscriptionName,
            TopicName = topicName
        });

        int count = 0;

        try
        {
            await foreach (var message in stream.ResponseStream.ReadAllAsync(cancellationTokenSource.Token))
            {
                count++;
            }
        }
        catch (RpcException e) when(e.StatusCode == StatusCode.Cancelled)
        {
            // doe
        }

        // Assert
        count.Should().Be(2);
    }
}