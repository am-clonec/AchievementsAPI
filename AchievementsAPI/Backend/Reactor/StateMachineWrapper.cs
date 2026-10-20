using System.Reflection;
using Il2CppInterop.Runtime.InteropTypes;

namespace AchievementsAPI.Backend;

public class StateMachineWrapper<T> where T : Il2CppObjectBase
{
    private readonly Reactor.Utilities.Il2CppStateMachineWrapper<T> _stateMachineWrapper;

    public T Instance => _stateMachineWrapper.Instance;

    public int State
    {
        get => _stateMachineWrapper.State;
        set => _stateMachineWrapper.State = value;
    }
    
    public StateMachineWrapper(Il2CppObjectBase stateMachine)
    {
        _stateMachineWrapper = new Reactor.Utilities.Il2CppStateMachineWrapper<T>(stateMachine);
    }
    
    public TField GetParameter<TField>(string parameterName)
    {
        return _stateMachineWrapper.GetParameter<TField>(parameterName);
    }
    
    public void SetParameter<TField>(string parameterName, TField value)
    {
        _stateMachineWrapper.SetParameter(parameterName, value);
    }
    
    public static MethodBase? GetStateMachineMoveNext(string methodName)
    {
        return Reactor.Utilities.Il2CppStateMachineWrapper<T>.GetStateMachineMoveNext(methodName);
    }
}