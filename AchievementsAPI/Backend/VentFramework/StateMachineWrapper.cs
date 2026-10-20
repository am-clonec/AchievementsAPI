using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes;

namespace AchievementsAPI.Backend;

// code from https://github.com/Lotus-AU/LotusContinued/blob/872ea7c8b5eac37d57aad3ac3b017b80719e139c/src/Utilities/StateMachineWrapper.cs
// which is modified from https://github.com/All-Of-Us-Mods/MiraAPI/blob/master/MiraAPI/Utilities/StateMachineWrapper.cs
public class StateMachineWrapper<T> where T : Il2CppObjectBase
{
    private readonly Il2CppObjectBase _stateMachine;

    // normally it is fields, but IL2CPP turns them into properties
    private readonly PropertyInfo _thisProperty;
    private readonly PropertyInfo _stateProperty;
    private readonly Dictionary<string, PropertyInfo> _propertyCache;

    private T? _parentInstance;

    /// <summary>
    /// Gets the instance of the parent class that owns the state machine.
    /// </summary>
    public T Instance => _parentInstance ??= (T)_thisProperty.GetValue(_stateMachine)!;

    /// <summary>
    /// Initializes a new instance of the <see cref="StateMachineWrapper{T}"/> class.
    /// </summary>
    /// <param name="stateMachine">The state machine instance to wrap.</param>
    public StateMachineWrapper(Il2CppObjectBase stateMachine)
    {
        _stateMachine = stateMachine;

        var type = _stateMachine.GetType();
        _thisProperty = AccessTools.Property(type, "__4__this");
        _stateProperty = AccessTools.Property(type, "__1__state");

        if (_thisProperty == null || _stateProperty == null)
        {
            throw new MissingMemberException($"Could not find required properties in type '{type}'.");
        }

        _propertyCache = [];
    }

    /// <summary>
    /// Gets the current state of the state machine.
    /// </summary>
    /// <returns>The current state as an integer.</returns>
    public int State => (int)_stateProperty.GetValue(_stateMachine)!;

    /// <summary>
    /// Gets a parameter from the state machine by its name.
    /// </summary>
    /// <param name="parameterName">The name of the parameter to retrieve.</param>
    /// <typeparam name="TField">The type of the parameter to retrieve.</typeparam>
    /// <returns>>The value of the specified parameter.</returns>
    /// <exception cref="MissingFieldException">Thrown if the specified parameter does not exist.</exception>
    public TField GetParameter<TField>(string parameterName)
    {
        if (_propertyCache.TryGetValue(parameterName, out var property))
        {
            return (TField)property.GetValue(_stateMachine)!;
        }

        property = AccessTools.Property(_stateMachine.GetType(), parameterName)
                   ?? throw new MissingFieldException($"Could not find parameter '{parameterName}' in state machine of type '{_stateMachine.GetType()}'.");

        _propertyCache[parameterName] = property;
        return (TField)property.GetValue(_stateMachine)!;
    }

    /// <summary>
    /// Sets a parameter in the state machine by its name.
    /// </summary>
    /// <param name="parameterName">The name of the parameter to set.</param>
    /// <param name="value">The value to assign to the parameter.</param>
    /// <typeparam name="TField">The type of the parameter to set.</typeparam>
    /// <exception cref="MissingFieldException">Thrown if the specified parameter does not exist.</exception>
    public void SetParameter<TField>(string parameterName, TField value)
    {
        if (_propertyCache.TryGetValue(parameterName, out var property))
        {
            property.SetValue(_stateMachine, value);
            return;
        }

        property = AccessTools.Property(_stateMachine.GetType(), parameterName)
                   ?? throw new MissingFieldException($"Could not find parameter '{parameterName}' in state machine of type '{_stateMachine.GetType()}'.");

        _propertyCache[parameterName] = property;
        property.SetValue(_stateMachine, value);
    }

    // https://github.com/All-Of-Us-Mods/MiraAPI/blob/eac442443bf85051210c8957605a524fb5136e8b/MiraAPI/Utilities/Helpers.cs#L43
    public static MethodBase? GetStateMachineMoveNext(string methodName)
    {
        var typeName = typeof(T).FullName;
        var showRoleStateMachine =
            typeof(T)
                .GetNestedTypes()
                .FirstOrDefault(x=>x.Name.Contains(methodName));

        if (showRoleStateMachine == null)
        {
            VentLib.Logging.StaticLogger.Exception($"Failed to find {methodName} state machine for {typeName}");
            return null;
        }

        var moveNext = AccessTools.Method(showRoleStateMachine, "MoveNext");
        if (moveNext == null)
        {
            VentLib.Logging.StaticLogger.Exception($"Failed to find MoveNext method for {typeName}.{methodName}");
            return null;
        }

        VentLib.Logging.StaticLogger.Info($"Found {methodName}.MoveNext");
        return moveNext;
    }
}
