using System;
using System.Collections.Generic;

namespace ASHATGoddessClient.AI;

/// <summary>
/// Behavior tree for AI decision making
/// </summary>
public class BehaviorTree
{
    private readonly BehaviorNode _rootNode;

    public BehaviorTree(BehaviorNode rootNode)
    {
        _rootNode = rootNode;
    }

    /// <summary>
    /// Tick the behavior tree
    /// </summary>
    public NodeStatus Tick()
    {
        return _rootNode.Execute();
    }
}

/// <summary>
/// Status returned by behavior nodes
/// </summary>
public enum NodeStatus
{
    Success,
    Failure,
    Running
}

/// <summary>
/// Base class for all behavior tree nodes
/// </summary>
public abstract class BehaviorNode
{
    public abstract NodeStatus Execute();
}

/// <summary>
/// Composite node that has multiple children
/// </summary>
public abstract class CompositeNode : BehaviorNode
{
    protected readonly List<BehaviorNode> _children = new();

    public void AddChild(BehaviorNode node)
    {
        _children.Add(node);
    }
}

/// <summary>
/// Sequence node - executes children in order, fails if any child fails
/// </summary>
public class SequenceNode : CompositeNode
{
    public override NodeStatus Execute()
    {
        foreach (var child in _children)
        {
            var status = child.Execute();
            
            if (status != NodeStatus.Success)
            {
                return status;
            }
        }

        return NodeStatus.Success;
    }
}

/// <summary>
/// Selector node - tries children in order, succeeds if any child succeeds
/// </summary>
public class SelectorNode : CompositeNode
{
    public override NodeStatus Execute()
    {
        foreach (var child in _children)
        {
            var status = child.Execute();
            
            if (status != NodeStatus.Failure)
            {
                return status;
            }
        }

        return NodeStatus.Failure;
    }
}

/// <summary>
/// Parallel node - executes all children simultaneously
/// </summary>
public class ParallelNode : CompositeNode
{
    private readonly int _successThreshold;
    private readonly int _failureThreshold;

    public ParallelNode(int successThreshold, int failureThreshold)
    {
        _successThreshold = successThreshold;
        _failureThreshold = failureThreshold;
    }

    public override NodeStatus Execute()
    {
        int successCount = 0;
        int failureCount = 0;
        int runningCount = 0;

        foreach (var child in _children)
        {
            var status = child.Execute();
            
            switch (status)
            {
                case NodeStatus.Success:
                    successCount++;
                    break;
                case NodeStatus.Failure:
                    failureCount++;
                    break;
                case NodeStatus.Running:
                    runningCount++;
                    break;
            }
        }

        if (successCount >= _successThreshold)
        {
            return NodeStatus.Success;
        }
        
        if (failureCount >= _failureThreshold)
        {
            return NodeStatus.Failure;
        }

        return NodeStatus.Running;
    }
}

/// <summary>
/// Decorator node that has exactly one child
/// </summary>
public abstract class DecoratorNode : BehaviorNode
{
    protected BehaviorNode? _child;

    public void SetChild(BehaviorNode node)
    {
        _child = node;
    }
}

/// <summary>
/// Inverter node - inverts the result of its child
/// </summary>
public class InverterNode : DecoratorNode
{
    public override NodeStatus Execute()
    {
        if (_child == null) return NodeStatus.Failure;

        var status = _child.Execute();

        return status switch
        {
            NodeStatus.Success => NodeStatus.Failure,
            NodeStatus.Failure => NodeStatus.Success,
            _ => status
        };
    }
}

/// <summary>
/// Repeater node - repeats its child a specified number of times
/// </summary>
public class RepeaterNode : DecoratorNode
{
    private readonly int _maxRepeats;
    private int _currentRepeats;

    public RepeaterNode(int maxRepeats)
    {
        _maxRepeats = maxRepeats;
    }

    public override NodeStatus Execute()
    {
        if (_child == null) return NodeStatus.Failure;

        while (_currentRepeats < _maxRepeats)
        {
            var status = _child.Execute();
            
            if (status == NodeStatus.Running)
            {
                return NodeStatus.Running;
            }

            _currentRepeats++;
        }

        _currentRepeats = 0;
        return NodeStatus.Success;
    }
}

/// <summary>
/// Action node - leaf node that performs an action
/// </summary>
public class ActionNode : BehaviorNode
{
    private readonly Func<NodeStatus> _action;

    public ActionNode(Func<NodeStatus> action)
    {
        _action = action;
    }

    public override NodeStatus Execute()
    {
        return _action();
    }
}

/// <summary>
/// Condition node - leaf node that checks a condition
/// </summary>
public class ConditionNode : BehaviorNode
{
    private readonly Func<bool> _condition;

    public ConditionNode(Func<bool> condition)
    {
        _condition = condition;
    }

    public override NodeStatus Execute()
    {
        return _condition() ? NodeStatus.Success : NodeStatus.Failure;
    }
}
