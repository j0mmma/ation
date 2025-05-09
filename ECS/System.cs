using System.Numerics;
using Raylib_cs;
using Ation.GameWorld;
using Ation.Simulation;


namespace Ation.Entities
{
    public abstract class BaseSystem
    {
        public abstract string Name { get; set; }
        public abstract void Update(EntityManager em, float dt, World world);
    }

    public class PlayerInputSystem : BaseSystem
    {
        private const float MoveSpeed = 50f;
        private const float JumpVelocity = -200f;

        public override string Name { get; set; } = "PlayerInputSystem";

        public override void Update(EntityManager em, float dt, World world)
        {
            foreach (var (entity, _) in em.GetAll<PlayerInputComponent>())
            {
                if (!em.TryGetComponent(entity, out VelocityComponent velocity)) continue;
                if (!em.TryGetComponent(entity, out TransformComponent position)) continue;
                if (!em.TryGetComponent(entity, out ColliderComponent collider)) continue;
                if (!em.TryGetComponent(entity, out StateComponent state)) continue;

                if (Raylib.IsKeyDown(KeyboardKey.D)) velocity.Velocity.X = MoveSpeed;
                else if (Raylib.IsKeyDown(KeyboardKey.A)) velocity.Velocity.X = -MoveSpeed;
                else velocity.Velocity.X = 0f;

                if (Raylib.IsKeyPressed(KeyboardKey.W))
                {
                    if (state.IsInLiquid)
                        velocity.Velocity.Y = -50f;
                    else if (collider.IsGrounded)
                        velocity.Velocity.Y = JumpVelocity;
                }

                if (Raylib.IsKeyPressed(KeyboardKey.One))
                {
                    if (em.TryGetComponent(entity, out InventoryComponent inventory))
                        inventory.SelectedIndex = 0;
                }

                if (Raylib.IsKeyPressed(KeyboardKey.Two))
                {
                    if (em.TryGetComponent(entity, out InventoryComponent inventory))
                        inventory.SelectedIndex = 1;
                }
                if (Raylib.IsKeyPressed(KeyboardKey.Three))
                {
                    if (em.TryGetComponent(entity, out InventoryComponent inventory))
                        inventory.SelectedIndex = 2;
                }
                if (Raylib.IsKeyPressed(KeyboardKey.Q))
                {
                    if (em.TryGetComponent(entity, out InventoryComponent inventory))
                    {
                        int index = inventory.SelectedIndex;
                        var item = inventory.Slots[index];
                        if (item != null)
                        {
                            inventory.Slots[index] = null;

                            // Drop position: just below the player's feet
                            Vector2 dropPos = position.Position + new Vector2(0, -0.5f);

                            // Re-enable world components
                            em.AddComponent(item, new PickupableComponent());
                            em.AddComponent(item, new DropCooldownComponent(0.5f)); // Half second pickup delay
                            em.AddComponent(item, new ColliderComponent(
                                new Vector2(1, 1),                         // Default size
                                new Vector2(-0.5f, -1f),                   // Offset from center-bottom
                                CollisionType.Passive));                  // Doesn’t block movement
                            em.AddComponent(item, new GravityComponent(300f));
                            em.AddComponent(item, new VelocityComponent(Vector2.Zero));

                            if (em.TryGetComponent(item, out TransformComponent t))
                                t.Position = dropPos;
                            else
                                em.AddComponent(item, new TransformComponent(dropPos));
                        }
                    }
                }


            }
        }

    }

    public class GravitySystem : BaseSystem
    {
        public override string Name { get; set; } = "GravitySystem";

        public override void Update(EntityManager em, float dt, World world)
        {
            foreach (var (entity, gravity) in em.GetAll<GravityComponent>())
            {
                if (!em.TryGetComponent(entity, out VelocityComponent velocity)) continue;
                if (!em.TryGetComponent(entity, out StateComponent state)) continue;

                float gravityFactor = state.IsInLiquid ? 0.1f : 1.0f;
                velocity.Velocity.Y += gravity.Gravity * gravityFactor * dt;
            }
        }
    }
    public class MovementIntentSystem : BaseSystem
    {
        public override string Name { get; set; } = "MovementIntentSystem";

        public override void Update(EntityManager em, float dt, World world)
        {
            foreach (var (entity, velocity) in em.GetAll<VelocityComponent>())
            {
                if (!em.TryGetComponent(entity, out MovementIntentComponent intent))
                {
                    intent = new MovementIntentComponent();
                    em.AddComponent(entity, intent);
                }

                intent.Delta = velocity.Velocity * dt;
            }
        }
    }

    public class CollisionSystem : BaseSystem
    {
        public override string Name { get; set; } = "CollisionSystem";

        public override void Update(EntityManager em, float dt, World world)
        {
            foreach (var (entity, intent) in em.GetAll<MovementIntentComponent>())
            {
                if (!em.TryGetComponent(entity, out TransformComponent position)) continue;
                if (!em.TryGetComponent(entity, out VelocityComponent velocity)) continue;
                if (!em.TryGetComponent(entity, out ColliderComponent collider)) continue;

                Vector2 pos = position.Position;
                Vector2 moveX = new Vector2(intent.Delta.X, 0);
                Vector2 moveY = new Vector2(0, intent.Delta.Y);

                TryMove(entity, ref pos, moveX, ref velocity.Velocity, collider, world, em);
                TryMove(entity, ref pos, moveY, ref velocity.Velocity, collider, world, em);

                collider.IsGrounded = CheckGrounded(pos, collider, world);

                position.Position = pos;
            }
        }


        private void TryMove(Entity entity, ref Vector2 pos, Vector2 delta, ref Vector2 velocity, ColliderComponent collider, World world, EntityManager em)
        {
            Vector2 newPos = pos + delta;

            bool hitsWorld = CollidesWithWorld(newPos + collider.Offset, collider.Size, world);
            bool hitsEntities = CollidesWithEntities(entity, newPos + collider.Offset, collider.Size, em);

            if (!hitsWorld && !hitsEntities)
            {
                pos = newPos;
                return;
            }

            // Try stepping up by a small amount (e.g., 1–2 world units)
            if (delta.X != 0)
            {
                const float maxStepHeight = 2f; // world units
                for (float step = 0.2f; step <= maxStepHeight; step += 0.2f)
                {
                    Vector2 stepUpPos = newPos - new Vector2(0, step); // try moving slightly up
                    bool clear = !CollidesWithWorld(stepUpPos + collider.Offset, collider.Size, world) &&
                                 !CollidesWithEntities(entity, stepUpPos + collider.Offset, collider.Size, em);
                    if (clear)
                    {
                        pos = stepUpPos;
                        return;
                    }
                }
            }

            // Regular collision resolution
            if (delta.X != 0) velocity.X = 0;
            if (delta.Y != 0) velocity.Y = 0;
        }


        private bool CollidesWithWorld(Vector2 pos, Vector2 size, World world)
        {
            int minX = (int)MathF.Floor(pos.X);
            int maxX = (int)MathF.Floor(pos.X + size.X);
            int minY = (int)MathF.Floor(pos.Y);
            int maxY = (int)MathF.Floor(pos.Y + size.Y);

            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    if (world.IsCollidableAt(x, y)) return true;
                }
            }

            return false;
        }

        private bool CollidesWithEntities(Entity self, Vector2 pos, Vector2 size, EntityManager em)
        {
            foreach (var (other, otherCollider) in em.GetAll<ColliderComponent>())
            {
                if (other.Id == self.Id) continue;
                if (otherCollider.CollisionType != CollisionType.Solid) continue;
                if (!em.TryGetComponent(other, out TransformComponent otherPos)) continue;

                var aMin = pos;
                var aMax = pos + size;
                var bMin = otherPos.Position + otherCollider.Offset;
                var bMax = bMin + otherCollider.Size;

                bool overlap = !(aMax.X <= bMin.X || aMin.X >= bMax.X ||
                                 aMax.Y <= bMin.Y || aMin.Y >= bMax.Y);

                if (overlap) return true;
            }

            return false;
        }

        private bool CheckGrounded(Vector2 pos, ColliderComponent collider, World world)
        {
            float startX = pos.X + collider.Offset.X;
            float endX = startX + collider.Size.X;
            float probeY = pos.Y + collider.Offset.Y + collider.Size.Y + 0.5f;

            for (float x = startX; x < endX; x += 0.2f) // small step for precision
            {
                if (world.IsCollidableAt((int)x, (int)probeY))
                    return true;
            }

            return false;
        }
        private bool CheckInLiquid(Vector2 pos, ColliderComponent collider, World world)
        {
            float startX = pos.X + collider.Offset.X;
            float endX = startX + collider.Size.X;
            float startY = pos.Y + collider.Offset.Y;
            float endY = startY + collider.Size.Y;

            for (float y = startY; y < endY; y += 1f)
                for (float x = startX; x < endX; x += 0.5f)
                {
                    var material = world.Get((int)x, (int)y);
                    if (material != null && MaterialFactory.GetClass(material.Type) == MaterialClass.Liquid)
                        return true;
                }

            return false;
        }



    }

    public class StateSystem : BaseSystem
    {
        public override string Name { get; set; } = "StateSystem";

        public override void Update(EntityManager em, float dt, World world)
        {
            foreach (var (entity, state) in em.GetAll<StateComponent>())
            {
                if (!em.TryGetComponent(entity, out TransformComponent transform)) continue;
                if (!em.TryGetComponent(entity, out ColliderComponent collider)) continue;

                var pos = transform.Position;
                var startX = pos.X + collider.Offset.X;
                var endX = startX + collider.Size.X;
                var startY = pos.Y + collider.Offset.Y;
                var endY = startY + collider.Size.Y;

                // Reset states
                state.IsInLiquid = false;
                state.IsInLava = false;

                for (float y = startY; y < endY; y += 0.5f)
                {
                    for (float x = startX; x < endX; x += 2f)
                    {
                        var material = world.Get((int)x, (int)y);
                        if (material == null) continue;

                        var matClass = MaterialFactory.GetClass(material.Type);

                        if (matClass == MaterialClass.Liquid)
                            state.IsInLiquid = true;

                        if (material.Type == MaterialType.Lava)
                            state.IsInLava = true;
                    }
                }

                // Auto ignite if in lava
                if (state.IsInLava)
                {
                    state.IsOnFire = true;
                    state.FireDuration = 2f;
                }

                // Handle fire duration
                if (state.IsOnFire)
                {
                    state.FireDuration -= dt;
                    if (state.FireDuration <= 0f)
                    {
                        state.IsOnFire = false;
                        state.FireDuration = 0f;
                    }
                }
            }
        }
    }

    public class PickupSystem : BaseSystem
    {
        public override string Name { get; set; } = "PickupSystem";

        public override void Update(EntityManager em, float dt, World world)
        {
            foreach (var (player, _) in em.GetAll<PlayerInputComponent>())
            {
                if (!em.TryGetComponent(player, out ColliderComponent playerCol) ||
                    !em.TryGetComponent(player, out TransformComponent playerPos) ||
                    !em.TryGetComponent(player, out InventoryComponent inventory))
                    continue;

                var playerMin = playerPos.Position + playerCol.Offset;
                var playerMax = playerMin + playerCol.Size;

                foreach (var (item, _) in em.GetAll<PickupableComponent>())
                {
                    // Handle drop cooldown
                    if (em.TryGetComponent(item, out DropCooldownComponent cooldown))
                    {
                        cooldown.TimeRemaining -= dt;
                        if (cooldown.TimeRemaining > 0) continue;
                        em.RemoveComponent<DropCooldownComponent>(item);
                    }

                    if (!em.TryGetComponent(item, out ColliderComponent itemCol) ||
                        !em.TryGetComponent(item, out TransformComponent itemPos))
                        continue;

                    var itemMin = itemPos.Position + itemCol.Offset;
                    var itemMax = itemMin + itemCol.Size;

                    bool overlaps = !(playerMax.X <= itemMin.X || playerMin.X >= itemMax.X ||
                                      playerMax.Y <= itemMin.Y || playerMin.Y >= itemMax.Y);

                    if (!overlaps) continue;

                    for (int i = 0; i < inventory.Slots.Length; i++)
                    {
                        if (inventory.Slots[i] == null)
                        {
                            inventory.Slots[i] = item;

                            // Remove from world, keep alive
                            em.RemoveComponent<PickupableComponent>(item);
                            em.RemoveComponent<ColliderComponent>(item);
                            em.RemoveComponent<TransformComponent>(item);
                            em.RemoveComponent<VelocityComponent>(item);
                            em.RemoveComponent<GravityComponent>(item);

                            return;
                        }
                    }
                }
            }
        }
    }


    public class ItemUseSystem : BaseSystem
    {
        public override string Name { get; set; } = "ItemUseSystem";
        private readonly Camera2D camera;

        public ItemUseSystem(Camera2D camera)
        {
            this.camera = camera;
        }

        public override void Update(EntityManager em, float dt, World world)
        {
            if (!Raylib.IsMouseButtonPressed(MouseButton.Left)) return;

            foreach (var (player, _) in em.GetAll<PlayerInputComponent>())
            {
                if (!em.TryGetComponent(player, out InventoryComponent inventory)) continue;

                var item = inventory.Slots[inventory.SelectedIndex];
                if (item == null) continue;

                if (em.TryGetComponent(item, out ItemComponent itemComp) && itemComp.UseAction != null)
                {
                    var mouseWorld = Raylib.GetScreenToWorld2D(Raylib.GetMousePosition(), camera);
                    itemComp.UseAction(em, player, item, mouseWorld);
                }
            }
        }
    }



}

