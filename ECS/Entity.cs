using System;
using System.Collections.Generic;
using System.Numerics;
using Raylib_cs;

using Ation.Common;

namespace Ation.Entities
{

    public class Entity
    {
        public int Id { get; }
        public string DisplayName { get; set; }

        internal Entity(int id, string displayName = "")
        {
            Id = id;
            DisplayName = string.IsNullOrEmpty(displayName) ? $"Entity({id})" : displayName;
        }

        public override int GetHashCode() => Id;
    }

    public class EntityManager
    {
        private int nextId = 1;
        private readonly Dictionary<Type, Dictionary<int, Component>> components = new();
        private readonly HashSet<int> aliveEntities = new();

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
            var baseColliderSize = new Vector2(10, 16);
            var colliderSize = baseColliderSize * scale;
            var transform = new TransformComponent(startPos);
            var velocity = new VelocityComponent(Vector2.Zero);
            var gravity = new GravityComponent(500f);
            var input = new PlayerInputComponent();
            var state = new StateComponent();
            Texture2D texture = Raylib.LoadTexture("Assets/Sprites/Wanderer Magican/Idle.png");
            Rectangle source = new Rectangle(0, 0, 128, 128);


            var colliderOffset = new Vector2(-colliderSize.X / 2f, -colliderSize.Y); // from feet
            var renderableOffset = Vector2.Zero; // origin handled by DrawTexturePro

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

            return player;
        }


        public Entity CreateStaticBlock(Vector2 position, Vector2 size)
        {
            var entity = CreateEntity();
            AddComponent(entity, new TransformComponent(position));
            AddComponent(entity, new ColliderComponent(size));
            return entity;
        }

        public Entity CreateItem(Vector2 position)
        {
            var item = CreateEntity();

            float scale = 0.8f;
            var baseColliderSize = new Vector2(48 / Variables.PixelSize, 48 / Variables.PixelSize);
            var colliderSize = baseColliderSize * scale;
            var transform = new TransformComponent(position);
            var gravity = new GravityComponent(300f);
            var velocity = new VelocityComponent(Vector2.Zero);

            Texture2D texture = Raylib.LoadTexture("Assets/Sprites/rpg_icons/spritesheet/spritesheet_48x48.png");
            Rectangle source = new Rectangle(0, 0, 48, 48);


            var colliderOffset = new Vector2(-colliderSize.X / 2f, -colliderSize.Y); // from feet
            var renderableOffset = Vector2.Zero; // origin handled by DrawTexturePro

            var collider = new ColliderComponent(colliderSize, colliderOffset);
            var renderable = new RenderableComponent(texture, source, renderableOffset, scale);


            // Add components
            AddComponent(item, transform);
            AddComponent(item, velocity);
            AddComponent(item, gravity);
            AddComponent(item, collider);
            AddComponent(item, renderable);

            return item;
        }

        public void DestroyEntity(Entity entity)
        {
            foreach (var store in components.Values)
                store.Remove(entity.Id);
            aliveEntities.Remove(entity.Id);
        }

        public bool IsAlive(Entity entity) => aliveEntities.Contains(entity.Id);

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

        public int EntityCount => aliveEntities.Count;

    }

}