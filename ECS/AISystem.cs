using System.Numerics;
using Ation.GameWorld;

namespace Ation.Entities
{
    public class AISystem : BaseSystem
    {
        public override string Name { get; set; } = "AISystem";
        private readonly Entity player;
        private readonly World world;

        public AISystem(Entity playerEntity, World world)
        {
            this.player = playerEntity;
            this.world = world;
        }

        public override void Update(EntityManager em, float dt, World _)
        {
            if (!em.TryGetComponent(player, out TransformComponent playerTransform)) return;

            foreach (var (entity, ai) in em.GetAll<AIComponent>())
            {
                if (!em.TryGetComponent(entity, out TransformComponent transform)) continue;
                if (!em.TryGetComponent(entity, out VelocityComponent velocity)) continue;

                Vector2 enemyPos = transform.Position;
                Vector2 playerPos = playerTransform.Position;

                float dx = playerPos.X - enemyPos.X;
                float dy = playerPos.Y - enemyPos.Y;

                // Only aggro if within X-range
                if (MathF.Abs(dx) > ai.AggroRange)
                {
                    velocity.Velocity.X = 0f;
                    continue;
                }

                // Move toward player on X axis
                velocity.Velocity.X = MathF.Sign(dx) * ai.MoveSpeed;

                // Jump if player is clearly above
                if (dy < -1.5f &&
                    em.TryGetComponent(entity, out ColliderComponent col) &&
                    col.IsGrounded)
                {
                    velocity.Velocity.Y = ai.JumpVelocity;
                }
            }
        }
    }
}
