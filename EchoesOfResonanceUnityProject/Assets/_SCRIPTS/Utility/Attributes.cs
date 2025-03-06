using System;

[AttributeUsage(AttributeTargets.Method)]
public class AllowedStatesAttribute : Attribute
{
    public GameState[] States { get; }

    public AllowedStatesAttribute(params GameState[] states)
    {
        States = states;
    }
    
}