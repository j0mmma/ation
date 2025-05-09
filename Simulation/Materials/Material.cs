using System.Numerics;
using System.Threading.Tasks.Dataflow;
using Ation.Common;
using Raylib_cs;
using Ation.GameWorld;

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
        public virtual MaterialClass Class => MaterialFactory.GetClass(Type);

        public float Mass = 1.0f;
        public float? Health { get; set; } = null;    // Health points (null = indestructible)
        public float? Lifetime { get; set; } = null;  // Lifetime in seconds (null = forever)
        public float Damage { get; set; } = 0; // Tunable, per second
        public float Flammability { get; set; } = 0.0f;
        public bool IsOnFire { get; set; } = false;
        public float ExplosionResistance { get; set; } = 1f;

        public Color Color { get; set; }
        public abstract string DisplayName { get; }
        public bool UpdatedThisFrame = false;
        public bool IsActive = false;
        public virtual bool IsCollidable => false;


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

        public abstract void Step(IMaterialContext ctx);

        public virtual bool ActOnNeighbor(Material neighbor, int selfX, int selfY, int neighborX, int neighborY, IMaterialContext ctx) => false;
        public virtual bool TryInteract(Material other, IMaterialContext ctx) => false;
        public void SetActive()
        {
            UpdatedThisFrame = true;
            IsActive = true;
        }

    }



    public interface IMaterialContext
    {
        Material? Get(int x, int y);
        void Set(int x, int y, Material? m);
        void Clear(int x, int y);
        void Swap(int x1, int y1, int x2, int y2);
        bool IsValidCell(int x, int y);
        bool IsEmpty(int x, int y);
        void ResetFlags();
        int Count();

    }

    public interface IChunkedMaterialContext : IMaterialContext
    {
        IEnumerable<Chunk> GetAllChunks();
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
        Lava,
        Acid,
        Smoke,
        Steam,
        AcidVapor,
        Wood,
        Stone,
        Fire,
        Eraser,
        Empty,
        Particle,
        FallingImmovable,
    }



    public static class MaterialFactory
    {
        private static readonly Dictionary<MaterialType, Func<Vector2, Material>> constructors = new()
        {
            { MaterialType.Sand, pos => new Sand(pos) },
            { MaterialType.Water, pos => new Water(pos) },
            { MaterialType.Lava, pos => new Lava(pos) },
            { MaterialType.Wood, pos => new Wood(pos) },
            { MaterialType.Stone, pos => new Stone(pos) },
            { MaterialType.Smoke, pos => new Smoke(pos) },
            { MaterialType.Steam, pos => new Steam(pos) },
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
            MaterialType.Lava => MaterialClass.Liquid,
            MaterialType.Acid => MaterialClass.Liquid,
            MaterialType.Smoke => MaterialClass.Gas,
            MaterialType.Steam => MaterialClass.Gas,
            MaterialType.Wood => MaterialClass.ImmovableSolid,
            MaterialType.Stone => MaterialClass.ImmovableSolid,
            MaterialType.Eraser => MaterialClass.Eraser,
            MaterialType.Empty => MaterialClass.Empty,
            _ => throw new ArgumentOutOfRangeException(nameof(type))
        };
    }
}
