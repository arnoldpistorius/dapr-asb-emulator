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
    
    [HttpPost(Routes.CreateTopic)]
    public async Task<IActionResult> CreateTopic([FromBody] Topic topic)
    {
        try
        {
            var createdTopic = await topicService.CreateTopic(topic.Name);
            return Ok(new Topic(createdTopic.Name));
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
}