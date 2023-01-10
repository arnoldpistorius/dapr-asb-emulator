using System.Net;
using DaprAsbEmulator.Adapter.Rest.Model;
using DaprAsbEmulator.Application.Exceptions;
using DaprAsbEmulator.Ports;
using Microsoft.AspNetCore.Mvc;

namespace DaprAsbEmulator.Adapter.Rest;

[Route(Routes.TopicController)]
public class TopicController : ControllerBase
{
    readonly ITopicService topicService;

    public TopicController(ITopicService topicService)
    {
        this.topicService = topicService;
    }

    [HttpGet(Routes.GetAllTopics)]
    public async Task<IActionResult> GetAllTopics()
    {
        try
        {
            var topics = await topicService.GetAllTopics();
            return Ok(topics.Select(Topic.FromDomainTopic));
        }
        catch
        {
            return Problem(statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    [HttpGet(Routes.GetTopic)]
    public async Task<IActionResult> GetTopic([FromRoute(Name = Routes.ParamTopicName)] string topicName)
    {
        try
        {
            var topic = await topicService.GetTopic(topicName);
            return Ok(Topic.FromDomainTopic(topic));
        }
        catch (TopicNotFoundException exception)
        {
            return NotFound(exception.Message);
        }
        catch
        {
            return Problem(statusCode: StatusCodes.Status500InternalServerError);
        }
    }
    
    [HttpPost(Routes.CreateTopic)]
    public async Task<IActionResult> CreateTopic([FromBody] Topic topic)
    {
        try
        {
            var createdTopic = await topicService.CreateTopic(topic.Name);
            return Ok(Topic.FromDomainTopic(createdTopic));
        }
        catch (TopicNameValidationFailedException exception)
        {
            return BadRequest(exception.Message);
        }
        catch (TopicAlreadyExistsException exception)
        {
            return Conflict(exception.Message);
        }
        catch
        {
            return Problem(statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    [HttpDelete(Routes.RemoveTopic)]
    public async Task<IActionResult> RemoveTopic([FromRoute(Name = Routes.ParamTopicName)] string topicName)
    {
        topicName = WebUtility.UrlDecode(topicName);
        try
        {
            await topicService.RemoveTopic(topicName);
            return NoContent();
        }
        catch (TopicNotFoundException exception)
        {
            return NotFound(exception.Message);
        }
        catch
        {
            return Problem(statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    [HttpPost(Routes.CreateTopicSubscription)]
    public async Task<IActionResult> CreateTopicSubscription([FromRoute(Name = Routes.ParamTopicName)] string topicName, [FromBody] CreateTopicRequestBody body)
    {
        topicName = WebUtility.UrlDecode(topicName);
        try
        {
            var domainTopicSubscription = await topicService.SubscribeTopic(topicName, body.SubscriptionName);
            var topicSubscription = TopicSubscription.FromDomainTopicSubscription(domainTopicSubscription);
            return Ok(topicSubscription);
        }
        catch (TopicNotFoundException exception)
        {
            return BadRequest(exception.Message);
        }
        catch (TopicSubscriptionAlreadyExistsException exception)
        {
            return Conflict(exception.Message);
        }
        catch
        {
            return Problem(statusCode: StatusCodes.Status500InternalServerError);
        }
    }
}