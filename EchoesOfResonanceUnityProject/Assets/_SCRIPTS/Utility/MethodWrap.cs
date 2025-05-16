using System;
using UnityEngine;
using System.Linq;
using System.Reflection;

public static class MethodGate
{
    public static Action Wrap(Broadcaster b, string methodName)
    {
        return () =>
        {
            if (TryGetMethod(b, methodName, out var method, out var target) && !ShouldBlock(method))
                ((Action)method.CreateDelegate(typeof(Action), target))();
        };
    }

    public static Action<T> Wrap<T>(Broadcaster b, string methodName)
    {
        return (arg) =>
        {
            if (TryGetMethod(b, methodName, out var method, out var target) && !ShouldBlock(method))
                ((Action<T>)method.CreateDelegate(typeof(Action<T>), target))(arg);
        };
    }

    public static Action<T1, T2> Wrap<T1, T2>(Broadcaster b, string methodName)
    {
        return (arg1, arg2) =>
        {
            if (TryGetMethod(b, methodName, out var method, out var target) && !ShouldBlock(method))
                ((Action<T1, T2>)method.CreateDelegate(typeof(Action<T1, T2>), target))(arg1, arg2);
        };
    }

    private static bool TryGetMethod(Broadcaster b, string methodName, out MethodInfo method, out object target)
    {
        var type = b.GetType();
        method = type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        if (method == null || method.DeclaringType == typeof(Broadcaster))
        {
            target = null;
            return false;
        }

        target = b;
        return true;
    }

    private static bool ShouldBlock(MethodInfo method)
    {
        var current = GameManager.root.State;

        var allowed = (AllowedStates)Attribute.GetCustomAttribute(method, typeof(AllowedStates));
        var disallowed = (DissallowedStates)Attribute.GetCustomAttribute(method, typeof(DissallowedStates));
        var above = (AllowAllAboveState)Attribute.GetCustomAttribute(method, typeof(AllowAllAboveState));

        if (allowed != null && !allowed.States.Contains(current)) return true;
        if (disallowed != null && disallowed.States.Contains(current)) return true;
        if (above != null && current < above.MinState) return true;

        return false;
    }
}
