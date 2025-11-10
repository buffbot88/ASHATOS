using System;
using System.Collections.Generic;
using System.Linq;

namespace ASHATGoddessClient.VisualScripting;

/// <summary>
/// Visual scripting system with node-based game logic editor
/// </summary>
public class VisualScriptingEngine
{
    private readonly Dictionary<string, ScriptGraph> _graphs = new();
    private readonly Dictionary<string, NodeType> _nodeTypes = new();

    public VisualScriptingEngine()
    {
        RegisterBuiltInNodeTypes();
    }

    /// <summary>
    /// Register built-in node types
    /// </summary>
    private void RegisterBuiltInNodeTypes()
    {
        RegisterNodeType(new NodeType
        {
            TypeId = "math.add",
            Name = "Add",
            Category = "Math",
            Inputs = new[] { 
                new NodePort { Name = "A", DataType = "float" },
                new NodePort { Name = "B", DataType = "float" }
            },
            Outputs = new[] { 
                new NodePort { Name = "Result", DataType = "float" }
            },
            Execute = (inputs) =>
            {
                var a = Convert.ToSingle(inputs[0]);
                var b = Convert.ToSingle(inputs[1]);
                return new object[] { a + b };
            }
        });

        RegisterNodeType(new NodeType
        {
            TypeId = "logic.if",
            Name = "If",
            Category = "Logic",
            Inputs = new[] { 
                new NodePort { Name = "Condition", DataType = "bool" }
            },
            Outputs = new[] { 
                new NodePort { Name = "True", DataType = "flow" },
                new NodePort { Name = "False", DataType = "flow" }
            },
            Execute = (inputs) =>
            {
                var condition = Convert.ToBoolean(inputs[0]);
                return new object[] { condition, !condition };
            }
        });

        RegisterNodeType(new NodeType
        {
            TypeId = "event.start",
            Name = "On Start",
            Category = "Events",
            Inputs = Array.Empty<NodePort>(),
            Outputs = new[] { 
                new NodePort { Name = "Flow", DataType = "flow" }
            },
            Execute = (inputs) => new object[] { true }
        });

        RegisterNodeType(new NodeType
        {
            TypeId = "debug.log",
            Name = "Print",
            Category = "Debug",
            Inputs = new[] { 
                new NodePort { Name = "Flow", DataType = "flow" },
                new NodePort { Name = "Message", DataType = "string" }
            },
            Outputs = new[] { 
                new NodePort { Name = "Flow", DataType = "flow" }
            },
            Execute = (inputs) =>
            {
                Console.WriteLine($"[VisualScript] {inputs[1]}");
                return new object[] { true };
            }
        });
    }

    /// <summary>
    /// Register a custom node type
    /// </summary>
    public void RegisterNodeType(NodeType nodeType)
    {
        _nodeTypes[nodeType.TypeId] = nodeType;
        Console.WriteLine($"[VisualScript] Registered node type: {nodeType.Name}");
    }

    /// <summary>
    /// Create a new script graph
    /// </summary>
    public ScriptGraph CreateGraph(string name)
    {
        var graph = new ScriptGraph(name, this);
        _graphs[name] = graph;
        return graph;
    }

    /// <summary>
    /// Get a script graph by name
    /// </summary>
    public ScriptGraph? GetGraph(string name)
    {
        _graphs.TryGetValue(name, out var graph);
        return graph;
    }

    /// <summary>
    /// Get a node type by ID
    /// </summary>
    public NodeType? GetNodeType(string typeId)
    {
        _nodeTypes.TryGetValue(typeId, out var nodeType);
        return nodeType;
    }

    /// <summary>
    /// Get all available node types
    /// </summary>
    public IEnumerable<NodeType> GetAllNodeTypes()
    {
        return _nodeTypes.Values;
    }
}

/// <summary>
/// Script graph containing nodes and connections
/// </summary>
public class ScriptGraph
{
    private readonly Dictionary<string, ScriptNode> _nodes = new();
    private readonly List<NodeConnection> _connections = new();
    private readonly VisualScriptingEngine _engine;

    public string Name { get; }
    public IReadOnlyDictionary<string, ScriptNode> Nodes => _nodes;
    public IReadOnlyList<NodeConnection> Connections => _connections;

    public ScriptGraph(string name, VisualScriptingEngine engine)
    {
        Name = name;
        _engine = engine;
    }

    /// <summary>
    /// Add a node to the graph
    /// </summary>
    public ScriptNode AddNode(string typeId, float x = 0, float y = 0)
    {
        var nodeType = _engine.GetNodeType(typeId);
        if (nodeType == null)
        {
            throw new Exception($"Node type not found: {typeId}");
        }

        var node = new ScriptNode
        {
            Id = Guid.NewGuid().ToString(),
            TypeId = typeId,
            NodeType = nodeType,
            X = x,
            Y = y
        };

        _nodes[node.Id] = node;
        return node;
    }

    /// <summary>
    /// Remove a node from the graph
    /// </summary>
    public void RemoveNode(string nodeId)
    {
        if (_nodes.Remove(nodeId))
        {
            _connections.RemoveAll(c => c.SourceNodeId == nodeId || c.TargetNodeId == nodeId);
        }
    }

    /// <summary>
    /// Connect two nodes
    /// </summary>
    public void Connect(string sourceNodeId, string sourcePortName, 
                       string targetNodeId, string targetPortName)
    {
        var connection = new NodeConnection
        {
            SourceNodeId = sourceNodeId,
            SourcePortName = sourcePortName,
            TargetNodeId = targetNodeId,
            TargetPortName = targetPortName
        };

        _connections.Add(connection);
    }

    /// <summary>
    /// Execute the graph
    /// </summary>
    public void Execute()
    {
        // Find entry point nodes (usually event nodes)
        var entryNodes = _nodes.Values.Where(n => n.TypeId.StartsWith("event."));

        foreach (var entryNode in entryNodes)
        {
            ExecuteNode(entryNode);
        }
    }

    /// <summary>
    /// Execute a specific node
    /// </summary>
    private object[]? ExecuteNode(ScriptNode node)
    {
        // Gather input values
        var inputs = new List<object>();
        
        foreach (var inputPort in node.NodeType.Inputs)
        {
            var connection = _connections.FirstOrDefault(c => 
                c.TargetNodeId == node.Id && c.TargetPortName == inputPort.Name);

            if (connection != null)
            {
                var sourceNode = _nodes[connection.SourceNodeId];
                var sourceOutputs = ExecuteNode(sourceNode);
                
                if (sourceOutputs != null)
                {
                    var outputIndex = Array.FindIndex(sourceNode.NodeType.Outputs, 
                        p => p.Name == connection.SourcePortName);
                    
                    if (outputIndex >= 0 && outputIndex < sourceOutputs.Length)
                    {
                        inputs.Add(sourceOutputs[outputIndex]);
                    }
                }
            }
            else
            {
                inputs.Add(GetDefaultValue(inputPort.DataType));
            }
        }

        // Execute node
        var outputs = node.NodeType.Execute(inputs.ToArray());

        // Execute connected flow nodes
        for (int i = 0; i < node.NodeType.Outputs.Length; i++)
        {
            var outputPort = node.NodeType.Outputs[i];
            
            if (outputPort.DataType == "flow" && outputs != null && 
                i < outputs.Length && Convert.ToBoolean(outputs[i]))
            {
                var flowConnections = _connections.Where(c => 
                    c.SourceNodeId == node.Id && c.SourcePortName == outputPort.Name);

                foreach (var connection in flowConnections)
                {
                    var targetNode = _nodes[connection.TargetNodeId];
                    ExecuteNode(targetNode);
                }
            }
        }

        return outputs;
    }

    private object GetDefaultValue(string dataType)
    {
        return dataType switch
        {
            "float" => 0.0f,
            "int" => 0,
            "bool" => false,
            "string" => "",
            _ => null!
        };
    }
}

/// <summary>
/// Node type definition
/// </summary>
public class NodeType
{
    public string TypeId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public NodePort[] Inputs { get; set; } = Array.Empty<NodePort>();
    public NodePort[] Outputs { get; set; } = Array.Empty<NodePort>();
    public Func<object[], object[]> Execute { get; set; } = (_) => Array.Empty<object>();
}

/// <summary>
/// Node instance in a graph
/// </summary>
public class ScriptNode
{
    public string Id { get; set; } = string.Empty;
    public string TypeId { get; set; } = string.Empty;
    public NodeType NodeType { get; set; } = new();
    public float X { get; set; }
    public float Y { get; set; }
}

/// <summary>
/// Node port (input or output)
/// </summary>
public class NodePort
{
    public string Name { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
}

/// <summary>
/// Connection between nodes
/// </summary>
public class NodeConnection
{
    public string SourceNodeId { get; set; } = string.Empty;
    public string SourcePortName { get; set; } = string.Empty;
    public string TargetNodeId { get; set; } = string.Empty;
    public string TargetPortName { get; set; } = string.Empty;
}
