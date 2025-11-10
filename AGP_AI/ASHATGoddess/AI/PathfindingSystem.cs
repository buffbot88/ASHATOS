using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace ASHATGoddessClient.AI
{
    /// <summary>
    /// A* pathfinding system for AI navigation
    /// </summary>
    public class PathfindingSystem
    {
        private readonly Grid _grid;

        public PathfindingSystem(int width, int height, float cellSize = 1.0f)
        {
            _grid = new Grid(width, height, cellSize);
        }

        /// <summary>
        /// Set a cell as walkable or blocked
        /// </summary>
        public void SetWalkable(int x, int y, bool walkable)
        {
            _grid.SetWalkable(x, y, walkable);
        }

        /// <summary>
        /// Find a path from start to end using A* algorithm
        /// </summary>
        public List<Vector2> FindPath(Vector2 start, Vector2 end)
        {
            var startNode = _grid.GetNode(start);
            var endNode = _grid.GetNode(end);

            if (startNode == null || endNode == null || !endNode.Walkable)
            {
                return new List<Vector2>();
            }

            var openSet = new PriorityQueue<PathNode, float>();
            var closedSet = new HashSet<PathNode>();
            var gScore = new Dictionary<PathNode, float>();
            var fScore = new Dictionary<PathNode, float>();
            var cameFrom = new Dictionary<PathNode, PathNode>();

            gScore[startNode] = 0;
            fScore[startNode] = Heuristic(startNode, endNode);
            openSet.Enqueue(startNode, fScore[startNode]);

            while (openSet.Count > 0)
            {
                var current = openSet.Dequeue();

                if (current == endNode)
                {
                    return ReconstructPath(cameFrom, current);
                }

                closedSet.Add(current);

                foreach (var neighbor in _grid.GetNeighbors(current))
                {
                    if (!neighbor.Walkable || closedSet.Contains(neighbor))
                    {
                        continue;
                    }

                    var tentativeGScore = gScore[current] + Vector2.Distance(current.Position, neighbor.Position);

                    if (!gScore.ContainsKey(neighbor) || tentativeGScore < gScore[neighbor])
                    {
                        cameFrom[neighbor] = current;
                        gScore[neighbor] = tentativeGScore;
                        fScore[neighbor] = gScore[neighbor] + Heuristic(neighbor, endNode);

                        if (!openSet.UnorderedItems.Any(item => item.Element == neighbor))
                        {
                            openSet.Enqueue(neighbor, fScore[neighbor]);
                        }
                    }
                }
            }

            return new List<Vector2>(); // No path found
        }

        private float Heuristic(PathNode a, PathNode b)
        {
            return Vector2.Distance(a.Position, b.Position);
        }

        private List<Vector2> ReconstructPath(Dictionary<PathNode, PathNode> cameFrom, PathNode current)
        {
            var path = new List<Vector2> { current.Position };

            while (cameFrom.ContainsKey(current))
            {
                current = cameFrom[current];
                path.Insert(0, current.Position);
            }

            return path;
        }
    }

    /// <summary>
    /// Grid for pathfinding
    /// </summary>
    public class Grid
    {
        private readonly PathNode[,] _nodes;
        private readonly int _width;
        private readonly int _height;
        private readonly float _cellSize;

        public Grid(int width, int height, float cellSize)
        {
            _width = width;
            _height = height;
            _cellSize = cellSize;
            _nodes = new PathNode[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    _nodes[x, y] = new PathNode
                    {
                        X = x,
                        Y = y,
                        Position = new Vector2(x * cellSize, y * cellSize),
                        Walkable = true
                    };
                }
            }
        }

        public void SetWalkable(int x, int y, bool walkable)
        {
            if (x >= 0 && x < _width && y >= 0 && y < _height)
            {
                _nodes[x, y].Walkable = walkable;
            }
        }

        public PathNode? GetNode(Vector2 position)
        {
            int x = (int)(position.X / _cellSize);
            int y = (int)(position.Y / _cellSize);

            if (x >= 0 && x < _width && y >= 0 && y < _height)
            {
                return _nodes[x, y];
            }

            return null;
        }

        public List<PathNode> GetNeighbors(PathNode node)
        {
            var neighbors = new List<PathNode>();
            var directions = new[]
            {
                (-1, 0), (1, 0), (0, -1), (0, 1),  // Cardinal
                (-1, -1), (-1, 1), (1, -1), (1, 1) // Diagonal
            };

            foreach (var (dx, dy) in directions)
            {
                int x = node.X + dx;
                int y = node.Y + dy;

                if (x >= 0 && x < _width && y >= 0 && y < _height)
                {
                    neighbors.Add(_nodes[x, y]);
                }
            }

            return neighbors;
        }
    }

    /// <summary>
    /// Node in the pathfinding grid
    /// </summary>
    public class PathNode
    {
        public int X { get; set; }
        public int Y { get; set; }
        public Vector2 Position { get; set; }
        public bool Walkable { get; set; }
    }
}
