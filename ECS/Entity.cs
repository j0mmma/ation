using System;
using System.Collections.Generic;
using System.Numerics;
using Raylib_cs;

using Ation.Common;
using System.Runtime.Serialization;
using Ation.GameWorld;

namespace Ation.Entities
{

    public class Entity
    {
        public int Id { get; }

        internal Entity(int id)
        {
            Id = id;
        }

        public override int GetHashCode() => Id;
    }


    public class EntityManager
    {
        public int EntityCount => aliveEntities.Count;
        public bool IsAlive(Entity entity) => aliveEntities.Contains(entity.Id);
        private int nextId = 1;
        private readonly Dictionary<Type, Dictionary<int, Component>> components = new();
        private readonly HashSet<int> aliveEntities = new();
        public void DestroyEntity(Entity entity)
        {
            foreach (var store in components.Values)
                store.Remove(entity.Id);
            aliveEntities.Remove(entity.Id);
        }

        public void AddComponent<T>(Entity entity, T component) where T : Component
        {
            if (!aliveEntities.Contains(entity.Id))
                throw new InvalidOperationException($"Cannot add component to destroyed entity {entity.Id}");

            var type = typeof(T);
            if (!components.TryGetValue(type, out var store))
                store = components[type] = new Dictionary<int, Component>();
            store[entity.Id] = component;
        }

        public bool TryGetComponent<T>(Entity entity, out T component) where T : Component
        {
            component = null!;
            return components.TryGetValue(typeof(T), out var store)
                && store.TryGetValue(entity.Id, out var raw)
                && (component = (T)raw) != null;
        }

        public void RemoveComponent<T>(Entity entity) where T : Component
        {
            if (components.TryGetValue(typeof(T), out var store))
                store.Remove(entity.Id);
        }

        public IEnumerable<(Entity, T)> GetAll<T>() where T : Component
        {
            if (components.TryGetValue(typeof(T), out var store))
            {
                foreach (var (id, comp) in store)
                {
                    if (aliveEntities.Contains(id))
                        yield return (new Entity(id), (T)comp);
                }
            }
        }

        public IEnumerable<Entity> GetAllEntities()
        {
            foreach (int id in aliveEntities)
                yield return new Entity(id);
        }


        public Entity CreateEntity()
        {
            var entity = new Entity(nextId++);
            aliveEntities.Add(entity.Id);
            return entity;
        }

        public Entity CreatePlayer(Vector2 startPos)
        {
            var player = CreateEntity();

            float scale = 1.1f;
            var baseColliderSize = new Vector2(6, 16);
            var colliderSize = baseColliderSize * scale;
            var transform = new TransformComponent(startPos);
            var velocity = new VelocityComponent(Vector2.Zero);
            var gravity = new GravityComponent(500f);
            var input = new PlayerInputComponent();
            var state = new StateComponent();
            var inventory = new InventoryComponent();

            Texture2D texture = Raylib.LoadTexture("Assets/Sprites/Wanderer Magican/Idle.png");
            Rectangle source = new Rectangle(0, 0, 128, 128);


            var colliderOffset = new Vector2(-colliderSize.X / 2f, -colliderSize.Y); // from feet
            var renderableOffset = new Vector2(0, 0);

            var collider = new ColliderComponent(colliderSize, colliderOffset);
            var renderable = new RenderableComponent(texture, source, renderableOffset, scale);


            // Add components
            AddComponent(player, state);
            AddComponent(player, transform);
            AddComponent(player, velocity);
            AddComponent(player, gravity);
            AddComponent(player, input);
            AddComponent(player, collider);
            AddComponent(player, renderable);
            AddComponent(player, inventory);
            var health = new HealthComponent(100f);
            // health.Current = health.Max * 0.25f;
            AddComponent(player, health);
            AddComponent(player, new ScoreComponent());
            AddComponent(player, new ManaComponent(150f, 40f));
            return player;
        }

        public Entity CreateDefaultSpell(Vector2 position)
        {
            var item = CreateEntity();


            float scale = 0.8f;
            var baseColliderSize = new Vector2(48 / Variables.PixelSize, 48 / Variables.PixelSize);
            var colliderSize = baseColliderSize * scale;
            var transform = new TransformComponent(position);
            var velocity = new VelocityComponent(Vector2.Zero);
            var gravity = new GravityComponent(300f);

            Texture2D texture = Raylib.LoadTexture("Assets/Sprites/rpg_icons/spritesheet/spritesheet_48x48.png");
            Rectangle source = new Rectangle(0, 14 * 48, 48, 48);

            var colliderOffset = new Vector2(-colliderSize.X / 2f, -colliderSize.Y); // from feet
            var renderableOffset = Vector2.Zero;

            var collider = new ColliderComponent(colliderSize, colliderOffset, CollisionType.Passive);

            var renderable = new RenderableComponent(texture, source, renderableOffset, scale);

            // Add core components
            AddComponent(item, new StateComponent());
            AddComponent(item, transform);
            AddComponent(item, velocity);
            AddComponent(item, gravity);
            AddComponent(item, collider);
            AddComponent(item, renderable);

            // Add pickup behavior
            AddComponent(item, new PickupableComponent());
            AddComponent(item, new DropCooldownComponent());
            AddComponent(item, new ItemComponent((em, user, item, cursorPos) =>
            {
                const float ManaCost = 20f;

                if (!em.TryGetComponent(user, out ManaComponent mana)) return;
                if (mana.Current < ManaCost) return;

                mana.Current -= ManaCost;

                if (!em.TryGetComponent(user, out TransformComponent playerTransform)) return;
                if (!em.TryGetComponent(user, out ColliderComponent playerCol)) return;

                Vector2 playerCenter = playerTransform.Position + playerCol.Offset + playerCol.Size * 0.5f;
                Vector2 direction = cursorPos - playerCenter * Variables.PixelSize;

                if (direction.LengthSquared() < 0.01f)
                    direction = new Vector2(1, 0); // fallback

                direction = Vector2.Normalize(direction);
                Vector2 spawnPos = playerCenter + direction * 10f;

                em.CreateDefaultProjectile(spawnPos, direction, user);
            }));






            return item;
        }

        public Entity CreateHealingPotion(Vector2 position)
        {
            var potion = CreateEntity();

            float scale = 0.8f;
            var size = new Vector2(48 / Variables.PixelSize, 48 / Variables.PixelSize) * scale;
            var transform = new TransformComponent(position);
            var texture = Raylib.LoadTexture("Assets/Sprites/rpg_icons/spritesheet/spritesheet_48x48.png");
            var source = new Rectangle(5 * 48, 15 * 48, 48, 48); // adjust to potion sprite

            AddComponent(potion, new TransformComponent(position));
            AddComponent(potion, new VelocityComponent(Vector2.Zero));
            AddComponent(potion, new GravityComponent(300f));
            AddComponent(potion, new ColliderComponent(size, new Vector2(-size.X / 2f, -size.Y), CollisionType.Passive));
            AddComponent(potion, new RenderableComponent(texture, source, Vector2.Zero, scale));
            AddComponent(potion, new StateComponent());
            AddComponent(potion, new PickupableComponent());
            AddComponent(potion, new DropCooldownComponent());

            AddComponent(potion, new ItemComponent((em, user, item, _) =>
            {
                if (!em.TryGetComponent(user, out HealthComponent health)) return;

                float healAmount = 30f;
                health.Current = MathF.Min(health.Current + healAmount, health.Max);

                em.DestroyEntity(item); // consume potion
            }));

            return potion;
        }

        public Entity CreateEnemy(Vector2 position)
        {
            var enemy = CreateEntity();

            float scale = 1.1f;
            var baseColliderSize = new Vector2(6, 16);
            var colliderSize = baseColliderSize * scale;
            var transform = new TransformComponent(position);
            var velocity = new VelocityComponent(Vector2.Zero);
            var gravity = new GravityComponent(500f);
            var state = new StateComponent();
            var inventory = new InventoryComponent();

            Texture2D texture = Raylib.LoadTexture("Assets/Sprites/Fire vizard/Idle.png");
            Rectangle source = new Rectangle(128, 0, 128, 128);



            var colliderOffset = new Vector2(-colliderSize.X / 2f - 5, -colliderSize.Y); // from feet
            var renderableOffset = new Vector2(0, 0);

            var collider = new ColliderComponent(colliderSize, colliderOffset);
            var renderable = new RenderableComponent(texture, source, renderableOffset, scale);


            // Add components
            AddComponent(enemy, state);
            AddComponent(enemy, transform);
            AddComponent(enemy, velocity);
            AddComponent(enemy, gravity);
            //AddComponent(enemy, input);
            AddComponent(enemy, collider);
            AddComponent(enemy, renderable);
            AddComponent(enemy, inventory);
            var health = new HealthComponent(100f);
            //health.Current = health.Max / 1.5f;
            AddComponent(enemy, health);
            AddComponent(enemy, new AIComponent());
            //AddComponent(enemy, new DamageComponent(50f, enemy));

            return enemy;
        }

        public Entity CreateDefaultProjectile(Vector2 position, Vector2 direction, Entity source)
        {
            var projectile = CreateEntity();

            float scale = 0.3f; // consistent with item scaling
            Vector2 baseSize = new Vector2(48, 48); // size in pixels from sprite
            Vector2 worldSize = baseSize / Variables.PixelSize * scale; // scale to world units
            Vector2 colliderSize = worldSize;
            Vector2 colliderOffset = new Vector2(-colliderSize.X / 2f, -colliderSize.Y); // center-bottom
            Vector2 renderOffset = Vector2.Zero;

            Texture2D texture = Raylib.LoadTexture("Assets/Sprites/rpg_icons/spritesheet/spritesheet_48x48.png");
            Rectangle sprite = new Rectangle(5 * 48, 9 * 48, 48, 48);

            AddComponent(projectile, new TransformComponent(position, scale));
            AddComponent(projectile, new VelocityComponent(direction * 400f));
            AddComponent(projectile, new StateComponent());
            AddComponent(projectile, new GravityComponent(100f));
            AddComponent(projectile, new MovementIntentComponent());

            AddComponent(projectile, new ColliderComponent(colliderSize, colliderOffset, CollisionType.Passive));
            AddComponent(projectile, new RenderableComponent(texture, sprite, renderOffset, scale));

            AddComponent(projectile, new ProjectileComponent(
                direction,
                500f,
                3f,
                true,
                true
            ));
            var dmg = new DamageComponent(45f, source);
            AddComponent(projectile, dmg);
            Console.WriteLine($"Added DamageComponent to projectile {projectile.Id} with amount {dmg.Amount}");

            return projectile;
        }


    }


    public static class ProjectileFactory
    {
        public static Entity CreateFireball(EntityManager em, Vector2 position, Vector2 direction)
        {
            var projectile = em.CreateEntity();

            // Dummy visual
            Texture2D texture = Raylib.LoadTexture("Assets/Spells/fireball.png"); // Make sure this exists
            Rectangle sprite = new Rectangle(0, 0, 16, 16);

            em.AddComponent(projectile, new TransformComponent(position));
            em.AddComponent(projectile, new VelocityComponent(direction * 150f));
            em.AddComponent(projectile, new ColliderComponent(new Vector2(6, 6)));
            em.AddComponent(projectile, new RenderableComponent(texture, sprite));
            // em.AddComponent(projectile, new ProjectileComponent
            // {
            //     Damage = 20f,
            //     Owner = default,
            //     ExplodesOnImpact = false,
            //     InteractsWithGeometry = true
            // });

            return projectile;
        }
    }
}


