using System;
using System.Collections.Generic;
using System.Numerics;

namespace ASHATGoddessClient.PhysicsEngine
{
    /// <summary>
    /// Physics engine with collision detection and rigid body dynamics
    /// Supports 2D physics for desktop mascot interactions and game elements
    /// </summary>
    public class PhysicsEngine
    {
        private readonly List<RigidBody> _bodies = new();
        private readonly List<Collider> _colliders = new();
        private readonly float _gravity = 9.81f;

        public PhysicsEngine(float gravity = 9.81f)
        {
            _gravity = gravity;
        }

        /// <summary>
        /// Register a rigid body for physics simulation
        /// </summary>
        public void AddRigidBody(RigidBody body)
        {
            if (!_bodies.Contains(body))
            {
                _bodies.Add(body);
            }
        }

        /// <summary>
        /// Remove a rigid body from physics simulation
        /// </summary>
        public void RemoveRigidBody(RigidBody body)
        {
            _bodies.Remove(body);
        }

        /// <summary>
        /// Register a collider for collision detection
        /// </summary>
        public void AddCollider(Collider collider)
        {
            if (!_colliders.Contains(collider))
            {
                _colliders.Add(collider);
            }
        }

        /// <summary>
        /// Remove a collider from collision detection
        /// </summary>
        public void RemoveCollider(Collider collider)
        {
            _colliders.Remove(collider);
        }

        /// <summary>
        /// Update physics simulation for all bodies
        /// </summary>
        public void Update(float deltaTime)
        {
            // Update rigid body physics
            foreach (var body in _bodies)
            {
                if (body.IsKinematic) continue;

                // Apply gravity
                body.ApplyForce(new Vector2(0, _gravity * body.Mass));

                // Update velocity and position
                body.Velocity += body.Acceleration * deltaTime;
                body.Position += body.Velocity * deltaTime;

                // Apply drag
                body.Velocity *= (1.0f - body.Drag * deltaTime);

                // Reset acceleration
                body.Acceleration = Vector2.Zero;
            }

            // Check for collisions
            DetectCollisions();
        }

        /// <summary>
        /// Detect and resolve collisions between all colliders
        /// </summary>
        private void DetectCollisions()
        {
            for (int i = 0; i < _colliders.Count; i++)
            {
                for (int j = i + 1; j < _colliders.Count; j++)
                {
                    var colliderA = _colliders[i];
                    var colliderB = _colliders[j];

                    if (CheckCollision(colliderA, colliderB, out var collision))
                    {
                        ResolveCollision(colliderA, colliderB, collision);
                    }
                }
            }
        }

        /// <summary>
        /// Check if two colliders are intersecting
        /// </summary>
        private bool CheckCollision(Collider a, Collider b, out CollisionInfo collision)
        {
            collision = new CollisionInfo();

            if (a is BoxCollider boxA && b is BoxCollider boxB)
            {
                return CheckBoxBoxCollision(boxA, boxB, out collision);
            }
            else if (a is CircleCollider circleA && b is CircleCollider circleB)
            {
                return CheckCircleCircleCollision(circleA, circleB, out collision);
            }
            else if (a is BoxCollider box && b is CircleCollider circle)
            {
                return CheckBoxCircleCollision(box, circle, out collision);
            }
            else if (a is CircleCollider circle2 && b is BoxCollider box2)
            {
                var result = CheckBoxCircleCollision(box2, circle2, out collision);
                collision.Normal = -collision.Normal; // Flip normal
                return result;
            }

            return false;
        }

        /// <summary>
        /// Box-Box collision detection using AABB
        /// </summary>
        private bool CheckBoxBoxCollision(BoxCollider a, BoxCollider b, out CollisionInfo collision)
        {
            collision = new CollisionInfo();

            var aMin = a.Position - a.Size / 2;
            var aMax = a.Position + a.Size / 2;
            var bMin = b.Position - b.Size / 2;
            var bMax = b.Position + b.Size / 2;

            if (aMax.X < bMin.X || aMin.X > bMax.X ||
                aMax.Y < bMin.Y || aMin.Y > bMax.Y)
            {
                return false;
            }

            // Calculate penetration depth
            var overlapX = Math.Min(aMax.X - bMin.X, bMax.X - aMin.X);
            var overlapY = Math.Min(aMax.Y - bMin.Y, bMax.Y - aMin.Y);

            collision.ColliderA = a;
            collision.ColliderB = b;

            if (overlapX < overlapY)
            {
                collision.Normal = new Vector2(aMax.X < bMax.X ? -1 : 1, 0);
                collision.Penetration = overlapX;
            }
            else
            {
                collision.Normal = new Vector2(0, aMax.Y < bMax.Y ? -1 : 1);
                collision.Penetration = overlapY;
            }

            collision.ContactPoint = (a.Position + b.Position) / 2;
            return true;
        }

        /// <summary>
        /// Circle-Circle collision detection
        /// </summary>
        private bool CheckCircleCircleCollision(CircleCollider a, CircleCollider b, out CollisionInfo collision)
        {
            collision = new CollisionInfo();

            var direction = b.Position - a.Position;
            var distance = direction.Length();
            var radiusSum = a.Radius + b.Radius;

            if (distance >= radiusSum)
            {
                return false;
            }

            collision.ColliderA = a;
            collision.ColliderB = b;
            collision.Normal = distance > 0 ? Vector2.Normalize(direction) : Vector2.UnitX;
            collision.Penetration = radiusSum - distance;
            collision.ContactPoint = a.Position + collision.Normal * a.Radius;

            return true;
        }

        /// <summary>
        /// Box-Circle collision detection
        /// </summary>
        private bool CheckBoxCircleCollision(BoxCollider box, CircleCollider circle, out CollisionInfo collision)
        {
            collision = new CollisionInfo();

            var boxMin = box.Position - box.Size / 2;
            var boxMax = box.Position + box.Size / 2;

            // Find closest point on box to circle center
            var closestPoint = new Vector2(
                Math.Clamp(circle.Position.X, boxMin.X, boxMax.X),
                Math.Clamp(circle.Position.Y, boxMin.Y, boxMax.Y)
            );

            var direction = circle.Position - closestPoint;
            var distance = direction.Length();

            if (distance >= circle.Radius)
            {
                return false;
            }

            collision.ColliderA = box;
            collision.ColliderB = circle;
            collision.Normal = distance > 0 ? Vector2.Normalize(direction) : Vector2.UnitY;
            collision.Penetration = circle.Radius - distance;
            collision.ContactPoint = closestPoint;

            return true;
        }

        /// <summary>
        /// Resolve collision using impulse-based physics
        /// </summary>
        private void ResolveCollision(Collider a, Collider b, CollisionInfo collision)
        {
            var bodyA = a.RigidBody;
            var bodyB = b.RigidBody;

            if (bodyA == null || bodyB == null) return;
            if (bodyA.IsKinematic && bodyB.IsKinematic) return;

            // Separate overlapping objects
            var correction = collision.Normal * collision.Penetration / 2;
            if (!bodyA.IsKinematic) bodyA.Position -= correction;
            if (!bodyB.IsKinematic) bodyB.Position += correction;

            // Calculate relative velocity
            var relativeVelocity = bodyB.Velocity - bodyA.Velocity;
            var velocityAlongNormal = Vector2.Dot(relativeVelocity, collision.Normal);

            // Don't resolve if velocities are separating
            if (velocityAlongNormal > 0) return;

            // Calculate restitution (bounciness)
            var restitution = Math.Min(bodyA.Restitution, bodyB.Restitution);

            // Calculate impulse scalar
            var impulseScalar = -(1 + restitution) * velocityAlongNormal;
            impulseScalar /= (bodyA.IsKinematic ? 0 : 1 / bodyA.Mass) +
                             (bodyB.IsKinematic ? 0 : 1 / bodyB.Mass);

            // Apply impulse
            var impulse = collision.Normal * impulseScalar;
            if (!bodyA.IsKinematic) bodyA.Velocity -= impulse / bodyA.Mass;
            if (!bodyB.IsKinematic) bodyB.Velocity += impulse / bodyB.Mass;

            // Trigger collision events
            a.OnCollisionEnter?.Invoke(collision);
            b.OnCollisionEnter?.Invoke(new CollisionInfo
            {
                ColliderA = collision.ColliderB,
                ColliderB = collision.ColliderA,
                Normal = -collision.Normal,
                Penetration = collision.Penetration,
                ContactPoint = collision.ContactPoint
            });
        }

        /// <summary>
        /// Cast a ray and return the first collision
        /// </summary>
        public bool Raycast(Vector2 origin, Vector2 direction, float maxDistance, out RaycastHit hit)
        {
            hit = new RaycastHit();
            var closestDistance = maxDistance;
            var foundHit = false;

            direction = Vector2.Normalize(direction);

            foreach (var collider in _colliders)
            {
                if (RaycastCollider(origin, direction, maxDistance, collider, out var tempHit))
                {
                    if (tempHit.Distance < closestDistance)
                    {
                        closestDistance = tempHit.Distance;
                        hit = tempHit;
                        foundHit = true;
                    }
                }
            }

            return foundHit;
        }

        /// <summary>
        /// Raycast against a specific collider
        /// </summary>
        private bool RaycastCollider(Vector2 origin, Vector2 direction, float maxDistance,
            Collider collider, out RaycastHit hit)
        {
            hit = new RaycastHit();

            if (collider is CircleCollider circle)
            {
                var oc = origin - circle.Position;
                var a = Vector2.Dot(direction, direction);
                var b = 2.0f * Vector2.Dot(oc, direction);
                var c = Vector2.Dot(oc, oc) - circle.Radius * circle.Radius;
                var discriminant = b * b - 4 * a * c;

                if (discriminant < 0) return false;

                var t = (-b - MathF.Sqrt(discriminant)) / (2.0f * a);
                if (t < 0 || t > maxDistance) return false;

                hit.Point = origin + direction * t;
                hit.Normal = Vector2.Normalize(hit.Point - circle.Position);
                hit.Distance = t;
                hit.Collider = collider;
                return true;
            }

            return false;
        }
    }

    /// <summary>
    /// Rigid body with physics properties
    /// </summary>
    public class RigidBody
    {
        public Vector2 Position { get; set; }
        public Vector2 Velocity { get; set; }
        public Vector2 Acceleration { get; set; }
        public float Mass { get; set; } = 1.0f;
        public float Drag { get; set; } = 0.1f;
        public float Restitution { get; set; } = 0.5f; // Bounciness
        public bool IsKinematic { get; set; } = false; // Doesn't respond to forces

        public void ApplyForce(Vector2 force)
        {
            if (!IsKinematic)
            {
                Acceleration += force / Mass;
            }
        }

        public void ApplyImpulse(Vector2 impulse)
        {
            if (!IsKinematic)
            {
                Velocity += impulse / Mass;
            }
        }
    }

    /// <summary>
    /// Base collider class
    /// </summary>
    public abstract class Collider
    {
        public Vector2 Position { get; set; }
        public RigidBody? RigidBody { get; set; }
        public bool IsTrigger { get; set; } = false;
        public Action<CollisionInfo>? OnCollisionEnter { get; set; }
        public Action<CollisionInfo>? OnCollisionExit { get; set; }
    }

    /// <summary>
    /// Box-shaped collider for AABB collision
    /// </summary>
    public class BoxCollider : Collider
    {
        public Vector2 Size { get; set; }
    }

    /// <summary>
    /// Circle-shaped collider
    /// </summary>
    public class CircleCollider : Collider
    {
        public float Radius { get; set; }
    }

    /// <summary>
    /// Collision information
    /// </summary>
    public class CollisionInfo
    {
        public Collider? ColliderA { get; set; }
        public Collider? ColliderB { get; set; }
        public Vector2 Normal { get; set; }
        public float Penetration { get; set; }
        public Vector2 ContactPoint { get; set; }
    }

    /// <summary>
    /// Raycast hit information
    /// </summary>
    public class RaycastHit
    {
        public Vector2 Point { get; set; }
        public Vector2 Normal { get; set; }
        public float Distance { get; set; }
        public Collider? Collider { get; set; }
    }
}
