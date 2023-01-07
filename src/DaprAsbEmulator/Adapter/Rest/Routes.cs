using System.Web;

namespace DaprAsbEmulator.Adapter.Rest;

public static class Routes
{
    const string S = "/";     
    
    public const string TopicController = "topic";
    public const string CreateTopic = "";
    public const string RemoveTopic = "{" + ParamTopicName + "}";

    public const string ParamTopicName = "topicName";

    static string Trim(string route) => route.Trim('/');
    public static string CreateTopicRoute() => 
        Trim(TopicController + S + CreateTopic);

    public static string CreateRemoveTopicRoute(string topicName) =>
        Trim(TopicController + S + RemoveTopic)
            .Replace("{" + ParamTopicName + "}", HttpUtility.UrlEncode(topicName));
}