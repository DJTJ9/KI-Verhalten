using System;
using System.Collections.Generic;

public class StateMachine {
    StateNode current;
    Dictionary<Type, StateNode> nodes = new();
    HashSet<ITransition> anyTransitions = new();

    public void Update() {
        var transition = GetTransition();
        if (transition != null)
            ChangeState(transition.To);

        current.State?.Update();
    }

    public void FixedUpdate() {
        current.State?.FixedUpdate();
    }

    public void SetState(IState _state) {
        current = nodes[_state.GetType()];
        current.State?.OnEnter();
    }

    void ChangeState(IState _state) {
        if (_state == current.State) return;

        var previousState = current.State;
        var nextState = nodes[_state.GetType()].State;

        previousState?.OnExit();
        nextState?.OnEnter();
        current = nodes[_state.GetType()];
    }

    ITransition GetTransition() {
        foreach (var transition in anyTransitions)
            if (transition.Condition.Evaluate())
                return transition;

        foreach (var transition in current.Transitions)
            if (transition.Condition.Evaluate())
                return transition;

        return null;
    }

    public void AddTransition(IState _from, IState _to, IPredicate _condition) {
        GetOrAddNode(_from).AddTransition(GetOrAddNode(_to).State, _condition);
    }

    public void AddAnyTransition(IState _to, IPredicate _condition) {
        anyTransitions.Add(new Transition(GetOrAddNode(_to).State, _condition));
    }

    StateNode GetOrAddNode(IState _state) {
        var node = nodes.GetValueOrDefault(_state.GetType());

        if (node == null) {
            node = new StateNode(_state);
            nodes.Add(_state.GetType(), node);
        }

        return node;
    }

    class StateNode {
        public IState State { get; }
        public HashSet<ITransition> Transitions { get; }

        public StateNode(IState _state) {
            State = _state;
            Transitions = new HashSet<ITransition>();
        }

        public void AddTransition(IState _to, IPredicate _condtion) {
            Transitions.Add(new Transition(_to, _condtion));
        }
    }
}
