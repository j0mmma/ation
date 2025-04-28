using System.Numerics;
using Ation.Common;
using Raylib_cs;

namespace Ation.Simulation
{
    public abstract class Material
    {
        public Vector2 Velocity = Vector2.Zero;
        public Vector2 NetForce = Vector2.Zero;
        protected Vector2 movementRemainder = Vector2.Zero;
        public Vector2 worldPos;
        public Vector2 gridPos;
        public abstract MaterialType Type { get; }

        public float Mass = 1.0f;
        public Color Color;
        public abstract string DisplayName { get; }
        public bool UpdatedThisFrame = false;
        public bool IsActive = false;

        public virtual void ApplyForce(Vector2 force) => NetForce += force;

        public virtual void Integrate(float dt)
        {
            var acceleration = NetForce / Mass;
            Velocity += acceleration * dt;
            NetForce = Vector2.Zero;
        }

        public Material(Vector2 worldPos)
        {
            this.worldPos = worldPos;
            gridPos = Utils.WorldToGrid(worldPos);
        }

        public abstract void Step(SimulationGrid grid);

        public virtual bool ActOnNeighbor(Material neighbor, int targetX, int targetY, SimulationGrid grid) => false;
    }

    public enum MaterialClass
    {
        MovableSolid,
        ImmovableSolid,
        Liquid,
        Gas,
        Eraser,
        Empty
    }

    public enum MaterialType
    {
        Sand,
        Water,
        Smoke,
        Wall,
        Eraser,
        Empty
    }

    public static class MaterialFactory
    {
        private static readonly Dictionary<MaterialType, Func<Vector2, Material>> constructors = new()
        {
            { MaterialType.Sand, pos => new Sand(pos) },
            { MaterialType.Water, pos => new Water(pos) },
            { MaterialType.Wall, pos => new Wall(pos) },
            { MaterialType.Smoke, pos => new Smoke(pos) },
            { MaterialType.Eraser, pos => new Eraser(pos) },
        };

        public static Material Create(MaterialType type, Vector2 worldPos)
        {
            if (constructors.TryGetValue(type, out var ctor))
                return ctor(worldPos);

            throw new NotSupportedException($"No constructor defined for {type}");
        }


        public static MaterialClass GetClass(MaterialType type) => type switch
        {
            MaterialType.Sand => MaterialClass.MovableSolid,
            MaterialType.Water => MaterialClass.Liquid,
            MaterialType.Smoke => MaterialClass.Gas,
            MaterialType.Wall => MaterialClass.ImmovableSolid,
            MaterialType.Eraser => MaterialClass.Eraser,
            MaterialType.Empty => MaterialClass.Empty,
            _ => throw new ArgumentOutOfRangeException(nameof(type))
        };
    }
}
