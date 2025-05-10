using System.Numerics;
using Ation.Entities;
using Ation.GameWorld;

namespace Ation.Common;

public static class CollisionHelper
{
    public static bool CheckEntityCollision(Entity self, Vector2 pos, Vector2 size, EntityManager em)
    {
        foreach (var (other, otherCol) in em.GetAll<ColliderComponent>())
        {
            if (other.Id == self.Id || otherCol.CollisionType != CollisionType.Solid) continue;
            if (!em.TryGetComponent(other, out TransformComponent otherPos)) continue;

            Vector2 aMin = pos;
            Vector2 aMax = pos + size;
            Vector2 bMin = otherPos.Position + otherCol.Offset;
            Vector2 bMax = bMin + otherCol.Size;

            if (!(aMax.X <= bMin.X || aMin.X >= bMax.X ||
                  aMax.Y <= bMin.Y || aMin.Y >= bMax.Y))
                return true;
        }

        return false;
    }

    public static bool CheckWorldCollision(Vector2 pos, Vector2 size, World world)
    {
        int minX = (int)MathF.Floor(pos.X);
        int maxX = (int)MathF.Floor(pos.X + size.X);
        int minY = (int)MathF.Floor(pos.Y);
        int maxY = (int)MathF.Floor(pos.Y + size.Y);

        for (int y = minY; y <= maxY; y++)
            for (int x = minX; x <= maxX; x++)
                if (world.IsCollidableAt(x, y)) return true;

        return false;
    }
}
