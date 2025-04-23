using System;

[AttributeUsage(AttributeTargets.Method)]
public class AllowedStates : Attribute
{
    public GameState[] States { get; }

    public AllowedStates(params GameState[] states)
    {
        States = states;
    }
}

[AttributeUsage(AttributeTargets.Method)]
public class AllowAllAboveState : Attribute
{
    public GameState MinState { get; }

    public AllowAllAboveState(GameState minState)
    {
        MinState = minState;
    }
}

[AttributeUsage(AttributeTargets.Method)]
public class DissallowedStates : Attribute
{
    public GameState[] States { get; }

    public DissallowedStates(params GameState[] states)
    {
        States = states;
    }
}