using System.Net;
using System.Net.Http.Json;
using DaprAsbEmulator.Adapter.Rest;
using DaprAsbEmulator.Adapter.Rest.Model;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Tests.IntegrationTests;

public class TopicControllerTests : IAsyncDisposable
{
    readonly WebApplicationFactory<Program> appFactory;
    readonly HttpClient client;

    public TopicControllerTests()
    {
        appFactory = new WebApplicationFactory<Program>();
        client = appFactory.CreateDefaultClient();
    }
    
    [Fact]
    public async Task CreateTopic_ValidNameAndNotExists_ReturnsTopic()
    {
        // Arrange
        var createTopic = new Topic("A-Topic-Name");
        
        // Act
        var response = await client.PostAsJsonAsync(Routes.CreateTopicRoute(), createTopic);
        
        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        var content = await response.Content.ReadFromJsonAsync<Topic>();
        content.Should().BeEquivalentTo(createTopic);
    }

    [Fact]
    public async Task CreateTopic_ValidNameButDoesExists_ReturnsConflictStatusCode()
    {
        // Arrange
        var createTopic = new Topic("A-Topic-Name");
        await client.PostAsJsonAsync(Routes.CreateTopicRoute(), createTopic);
        
        // Act
        var response = await client.PostAsJsonAsync(Routes.CreateTopicRoute(), createTopic);
        
        // Assert
        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
    
    [Fact]
    public async Task CreateTopic_InvalidName_ReturnsBadRequestStatusCode()
    {
        // Arrange
        var createTopic = new Topic("/A-Topic-Name");
        
        // Act
        var response = await client.PostAsJsonAsync(Routes.CreateTopicRoute(), createTopic);
        
        // Assert
        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RemoveTopic_TopicExists_ReturnsNoContentStatusCode()
    {
        // Arrange
        var topic = new Topic("this-topic-exists");
        await EnsureTopic(topic);
        
        // Act
        var response = await client.DeleteAsync(Routes.CreateRemoveTopicRoute(topic.Name));
        
        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task RemoveTopic_TopicNotExists_ReturnsNotFouncStatusCode()
    {
        // Arrange
        string topicName = "this-topic-does-not-exist";
        
        // Act
        var response = await client.DeleteAsync(Routes.CreateRemoveTopicRoute(topicName));
        
        // Assert
        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    async Task EnsureTopic(Topic topic)
    {
        var response = await client.PostAsJsonAsync(Routes.CreateTopicRoute(), topic);
        if (response is { IsSuccessStatusCode: false, StatusCode: not HttpStatusCode.Conflict })
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Could not create topic: {error}");
        }
    }

    public ValueTask DisposeAsync()
    {
        return appFactory.DisposeAsync();
    }
}