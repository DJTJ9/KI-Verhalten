using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BehaviourTree : Node
{
    readonly IPolicy policy;
        
    public BehaviourTree(string name, IPolicy policy = null) : base(name) {
        this.policy = policy ?? Policies.RunForever;
    }

    public override Status Process() {
        Status status = children[currentChild].Process();
        if (policy.ShouldReturn(status)) {
            return status;
        }
            
        currentChild = (currentChild + 1) % children.Count;
        return Status.Running;
    }
}

 public class UntilFail : Node {
        public UntilFail(string name) : base(name) { }
        
        public override Status Process() {
            if (children[0].Process() == Status.Failure) {
                Reset();
                return Status.Failure;
            }

            return Status.Running;
        }
    }
    
    public class Inverter : Node {
        public Inverter(string name) : base(name) { }
        
        public override Status Process() {
            switch (children[0].Process()) {
                case Status.Running:
                    return Status.Running;
                case Status.Failure:
                    return Status.Success;
                default:
                    return Status.Failure;
            }
        }
    }
    
    public class RandomSelector : PrioritySelector {
        protected override List<Node> SortChildren() => children.Shuffle().ToList();
        
        public RandomSelector(string name, int priority = 0) : base(name, priority) { }
    }
    
    public class PrioritySelector : Selector {
        List<Node> sortedChildren;
        List<Node> SortedChildren => sortedChildren ??= SortChildren();
        
        protected virtual List<Node> SortChildren() => children.OrderByDescending(child => child.priority).ToList();
        
        public PrioritySelector(string name, int priority = 0) : base(name, priority) { }
        
        public override void Reset() {
            base.Reset();
            sortedChildren = null;
        }
        
        public override Status Process() {
            foreach (var child in SortedChildren) {
                switch (child.Process()) {
                    case Status.Running:
                        return Status.Running;
                    case Status.Success:
                        Reset();
                        return Status.Success;
                    default:
                        continue;
                }
            }

            Reset();
            return Status.Failure;
        }
    }
    
    public class Selector : Node {
        public Selector(string name, int priority = 0) : base(name, priority) { }

        public override Status Process() {
            if (currentChild < children.Count) {
                switch (children[currentChild].Process()) {
                    case Status.Running:
                        return Status.Running;
                    case Status.Success:
                        Reset();
                        return Status.Success;
                    default:
                        currentChild++;
                        return Status.Running;
                }
            }
            
            Reset();
            return Status.Failure;
        }
    }
    
    public class Sequence : Node {
        public Sequence(string name, int priority = 0) : base(name, priority) { }

        public override Status Process() {
            if (currentChild < children.Count) {
                switch (children[currentChild].Process()) {
                    case Status.Running:
                        return Status.Running;
                    case Status.Failure:
                        currentChild = 0;
                        return Status.Failure;
                    default:
                        currentChild++;
                        return currentChild == children.Count ? Status.Success : Status.Running;
                }
            }

            Reset();
            return Status.Success;
        }
    }

public interface IPolicy {
    bool ShouldReturn(Node.Status status);
}

public static class Policies {
    public static readonly IPolicy RunForever = new RunForeverPolicy();
    public static readonly IPolicy RunUntilSuccess = new RunUntilSuccessPolicy();
    public static readonly IPolicy RunUntilFailure = new RunUntilFailurePolicy();
        
    class RunForeverPolicy : IPolicy {
        public bool ShouldReturn(Node.Status status) => false;
    }
        
    class RunUntilSuccessPolicy : IPolicy {
        public bool ShouldReturn(Node.Status status) => status == Node.Status.Success;
    }
        
    class RunUntilFailurePolicy : IPolicy {
        public bool ShouldReturn(Node.Status status) => status == Node.Status.Failure;
    }
}