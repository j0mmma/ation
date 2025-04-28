using System.Numerics;
using Raylib_cs;
using Ation.Common;

namespace Ation.Simulation
{
    public abstract class MovableSolid : Material
    {
        protected float friction = 0.9f;
        protected float speedClamp = 200f;

        public MovableSolid(Vector2 worldPos) : base(worldPos) { }

        public override void Step(SimulationGrid grid)
        {
            float dt = Raylib.GetFrameTime();

            UpdatedThisFrame = true;
            IsActive = false;

            ApplyForce(FallingSandSim.Gravity);
            Integrate(dt);
            movementRemainder += Velocity * dt;

            int moveX = (int)MathF.Floor(movementRemainder.X);
            int moveY = (int)MathF.Floor(movementRemainder.Y);
            movementRemainder.X -= moveX;
            movementRemainder.Y -= moveY;

            int x = (int)gridPos.X;
            int y = (int)gridPos.Y;

            for (int i = 0; i < Math.Abs(moveY); i++)
            {
                int targetY = y + Math.Sign(moveY);
                if (!grid.IsValidCell(x, targetY)) return;

                if (grid.IsEmpty(x, targetY))
                {
                    grid.Swap(x, y, x, targetY);
                    y = targetY;
                    gridPos = new Vector2(x, y);
                    IsActive = true;
                    continue;
                }

                var neighbor = grid.Get(x, targetY);
                if (neighbor != null && ActOnNeighbor(neighbor, x, targetY, grid))
                {
                    // Neighbor handled (pushed or swapped), so move ourselves down
                    y = targetY;
                    gridPos = new Vector2(x, y);
                    IsActive = true;
                    continue;
                }

                // Try sliding diagonally into empty or liquid
                bool slid = false;

                var leftDown = grid.Get(x - 1, y + 1);
                if (grid.IsValidCell(x - 1, y + 1) && (grid.IsEmpty(x - 1, y + 1) || leftDown is Liquid))
                {
                    grid.Swap(x, y, x - 1, y + 1);
                    x -= 1; y += 1;
                    slid = true;
                }
                else
                {
                    var rightDown = grid.Get(x + 1, y + 1);
                    if (grid.IsValidCell(x + 1, y + 1) && (grid.IsEmpty(x + 1, y + 1) || rightDown is Liquid))
                    {
                        grid.Swap(x, y, x + 1, y + 1);
                        x += 1; y += 1;
                        slid = true;
                    }
                }

                if (slid)
                {
                    gridPos = new Vector2(x, y);
                    IsActive = true;
                    continue;
                }

                Velocity *= friction;
                break;
            }

            Velocity = Vector2.Clamp(Velocity, new Vector2(-speedClamp, -speedClamp), new Vector2(speedClamp, speedClamp));
            worldPos = Utils.GridToWorld(gridPos);
        }

        public override bool ActOnNeighbor(Material neighbor, int targetX, int targetY, SimulationGrid grid)
        {
            if (neighbor == null)
                return false;

            if (neighbor is Liquid)
            {
                // Try to push the liquid first
                (int dx, int dy)[] pushes = {
            (0, 1),   // Down
            (-1, 0),  // Left
            (1, 0),   // Right
            (0, -1)   // Up
        };

                foreach (var (dx, dy) in pushes)
                {
                    int nx = (int)neighbor.gridPos.X;
                    int ny = (int)neighbor.gridPos.Y;
                    int tx = nx + dx;
                    int ty = ny + dy;

                    if (grid.IsValidCell(tx, ty) && grid.IsEmpty(tx, ty))
                    {
                        // Push liquid aside
                        grid.Swap(nx, ny, tx, ty);
                        // NOW immediately move self into the neighbor's old position
                        grid.Swap((int)gridPos.X, (int)gridPos.Y, targetX, targetY);
                        return true;
                    }
                }

                // If can't push, swap directly with liquid
                grid.Swap((int)gridPos.X, (int)gridPos.Y, targetX, targetY);
                return true;
            }


            return false;
        }



    }
}
