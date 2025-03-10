public class Transition : ITransition {
    public IState To { get; }
    public IPredicate Condition { get; }

    public Transition(IState _to, IPredicate _condition) {
        To = _to;
        Condition = _condition;
    }
}