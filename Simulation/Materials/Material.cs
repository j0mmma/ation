using System.Numerics;
using System.Threading.Tasks.Dataflow;
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
        public float? Health { get; set; } = null;    // Health points (null = indestructible)
        public float? Lifetime { get; set; } = null;  // Lifetime in seconds (null = forever)
        public float Damage { get; set; } = 0; // Tunable, per second
        public float Flammability { get; set; } = 0.0f;
        public bool IsOnFire { get; set; } = false;


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

        public virtual bool ActOnNeighbor(Material neighbor, int selfX, int selfY, int neighborX, int neighborY, SimulationGrid grid) => false;
        public virtual bool TryInteract(Material other, SimulationGrid grid) => false;
        public void SetActive()
        {
            UpdatedThisFrame = true;
            IsActive = true;
        }

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
        Acid,
        Smoke,
        AcidVapor,
        Wood,
        Fire,
        Eraser,
        Empty,
        Particle,
    }



    public static class MaterialFactory
    {
        private static readonly Dictionary<MaterialType, Func<Vector2, Material>> constructors = new()
        {
            { MaterialType.Sand, pos => new Sand(pos) },
            { MaterialType.Water, pos => new Water(pos) },
            { MaterialType.Wood, pos => new Wood(pos) },
            { MaterialType.Smoke, pos => new Smoke(pos) },
            { MaterialType.Eraser, pos => new Eraser(pos) },
            { MaterialType.Fire, pos => new Fire(pos) },
            { MaterialType.Acid, pos => new Acid(pos) },
            { MaterialType.AcidVapor, pos => new AcidVapor(pos) },
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
            MaterialType.Acid => MaterialClass.Liquid,
            MaterialType.Smoke => MaterialClass.Gas,
            MaterialType.Wood => MaterialClass.ImmovableSolid,
            MaterialType.Eraser => MaterialClass.Eraser,
            MaterialType.Empty => MaterialClass.Empty,
            _ => throw new ArgumentOutOfRangeException(nameof(type))
        };
    }
}
