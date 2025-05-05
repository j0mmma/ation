using System;
using System.Collections.Generic;
using System.Numerics;

using Ation.Common;

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
        public override string ToString() => $"Entity({Id})";
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
            AddComponent(player, new PositionComponent(startPos));
            AddComponent(player, new VelocityComponent(Vector2.Zero));
            AddComponent(player, new SizeComponent(new Vector2(Variables.PixelSize * 2, Variables.PixelSize * 3)));
            AddComponent(player, new GravityComponent(150f));
            AddComponent(player, new PlayerInputComponent());
            return player;
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