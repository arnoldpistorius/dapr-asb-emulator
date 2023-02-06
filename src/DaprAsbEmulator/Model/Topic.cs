using DaprAsbEmulator.Application;

namespace DaprAsbEmulator.Model;

public class Topic
{
    public string Name { get; }
    
    public Topic(string name)
    {
        Name = name;
    }
}