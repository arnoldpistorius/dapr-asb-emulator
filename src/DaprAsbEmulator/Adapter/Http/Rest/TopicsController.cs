using DaprAsbEmulator.Model;
using DaprAsbEmulator.Ports;
using DaprAsbEmulator.Ports.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace DaprAsbEmulator.Adapter.Http.Rest;

[Route("/topics")]
public class TopicsController : ControllerBase
{
    readonly ITopicService topicService;

    public TopicsController(ITopicService topicService)
    {
        this.topicService = topicService;
    }
    
    // GET /topics returns all topics as RestTopic[]
    [HttpGet]
    public async Task<IActionResult> GetTopics()
    {
        var topics = await topicService.GetTopics();
        var restTopics = topics.Select(topic => new RestTopic { Name = topic.Name }).ToArray();
        return Ok(restTopics);
    }
    
    // POST /topics creates a new topic
    [HttpPost]
    public async Task<IActionResult> CreateTopic([FromBody] RestCreateTopicRequest request)
    {
        try
        {
            var topic = await topicService.CreateTopic(request.Name);
            return Ok(new RestTopic { Name = topic.Name });
        }
        // Catch TopicAlreadyExistsException and return 409 Conflict
        catch (TopicAlreadyExistsException)
        {
            return Conflict();
        }
        // Catch all other exceptions and return 500 Internal Server Error
        catch
        {
            return StatusCode(500);
        }
    }
    
    // POST /topics/{topicName}/publish publishes a message to a topic
    [HttpPost("{topicName}/publish")]
    public async Task<IActionResult> PublishMessage(string topicName, [FromBody] RestPublishMessageRequest request)
    {
        try
        {
            await topicService.PublishMessage(topicName, request.Message);
            return Ok();
        }
        // Catch TopicNotFoundException and return 404 Not Found
        catch (TopicNotFoundException)
        {
            return NotFound();
        }
        // Catch all other exceptions and return 500 Internal Server Error
        catch
        {
            return StatusCode(500);
        }
    }
    
    // POST /topics/{topicName}/subscriptions subscribes to a topic
    [HttpPost("{topicName}/subscriptions")]
    public async Task<IActionResult> Subscribe(string topicName, [FromBody] RestSubscribeRequest request)
    {
        try
        {
            var topicSubscription = await topicService.Subscribe(topicName, request.SubscriptionName);
            return Ok(new RestTopicSubscription { TopicName = topicSubscription.TopicName, SubscriptionName = topicSubscription.SubscriptionName });
        }
        // Catch TopicNotFoundException and return 404 Not Found
        catch (TopicNotFoundException)
        {
            return NotFound();
        }
        // Catch TopicSubscriptionAlreadyExistsException and return 409 Conflict
        catch (TopicSubscriptionAlreadyExistsException)
        {
            return Conflict();
        }
        // Catch all other exceptions and return 500 Internal Server Error
        catch
        {
            return StatusCode(500);
        }
    }
    
    // GET /topics/{topicName}/subscriptions/peek/{subscriptionName} peeks a message from a subscription
    [HttpGet("{topicName}/subscriptions/peek/{subscriptionName}")]
    public async Task<IActionResult> PeekMessage(string topicName, string subscriptionName)
    {
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5)); 
            var message = await topicService.Peek(topicName, subscriptionName, cts.Token);
            return Ok(new RestMessage { Message = message.Value, MessageId = message.Id });
        }
        // Catch TopicNotFoundException and return 404 Not Found
        catch (TopicNotFoundException)
        {
            return NotFound();
        }
        // Catch TopicSubscriptionNotFoundException and return 404 Not Found
        catch (TopicSubscriptionNotFoundException)
        {
            return NotFound();
        }
        // Catch OperationCanceledException and return 204 No Content
        catch (OperationCanceledException)
        {
            return NoContent();
        }
        // Catch all other exceptions and return 500 Internal Server Error
        catch
        {
            return StatusCode(500);
        }
    }
    
    // POST /topics/{topicName}/subscriptions/succeed/{subscriptionName} succeeds a message from a subscription
    [HttpPost("{topicName}/subscriptions/succeed/{subscriptionName}/{messageId}")]
    public async Task<IActionResult> SucceedMessage(string topicName, string subscriptionName, Guid messageId)
    {
        try
        {
            var message = new Message(messageId, string.Empty);
            await topicService.SucceedMessage(topicName, subscriptionName, message);
            return Ok();
        }
        // Catch TopicNotFoundException and return 404 Not Found
        catch (TopicNotFoundException)
        {
            return NotFound();
        }
        // Catch TopicSubscriptionNotFoundException and return 404 Not Found
        catch (TopicSubscriptionNotFoundException)
        {
            return NotFound();
        }
        // Catch all other exceptions and return 500 Internal Server Error
        catch
        {
            return StatusCode(500);
        }
    }
    
    // POST /topics/{topicName}/subscriptions/fail/{subscriptionName}/{messageId} fails a message from a subscription
    [HttpPost("{topicName}/subscriptions/fail/{subscriptionName}/{messageId}")]
    public async Task<IActionResult> FailMessage(string topicName, string subscriptionName, Guid messageId)
    {
        try
        {
            var message = new Message(messageId, string.Empty);
            await topicService.FailMessage(topicName, subscriptionName, message);
            return Ok();
        }
        // Catch TopicNotFoundException and return 404 Not Found
        catch (TopicNotFoundException)
        {
            return NotFound();
        }
        // Catch TopicSubscriptionNotFoundException and return 404 Not Found
        catch (TopicSubscriptionNotFoundException)
        {
            return NotFound();
        }
        // Catch all other exceptions and return 500 Internal Server Error
        catch
        {
            return StatusCode(500);
        }
    }
}

public class RestTopic
{
    // Copy of DaprAsbEmulator.Model.Topic
    public string Name { get; set; } = null!;
}

public class RestCreateTopicRequest
{
    // Copy of RestTopic
    public string Name { get; set; } = null!;
}

// Model RestPublishMessageRequest with  a message
public class RestPublishMessageRequest
{
    public string Message { get; set; } = null!;
}

// Model RestSubscribeRequest with a subscription name
public class RestSubscribeRequest
{
    public string SubscriptionName { get; set; } = null!;
}

public class RestTopicSubscription
{
    // Copy of DaprAsbEmulator.Model.TopicSubscription
    public string TopicName { get; set; } = null!;
    public string SubscriptionName { get; set; } = null!;
}

public class RestMessage
{
    // Copy of DaprAsbEmulator.Model.Message
    public Guid MessageId { get; set; }
    public string Message { get; set; } = null!;
}