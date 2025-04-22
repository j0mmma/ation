using System.Numerics;
using Raylib_cs;
using Ation.Common;

namespace Ation.Simulation
{
    public abstract class MovableSolid : Material
    {
        protected float friction = 0.9f;
        protected float speedClamp = 100f;

        public MovableSolid(Vector2 worldPos) : base(worldPos) { }

        public override void Step(SimulationGrid grid)
        {
            float dt = Raylib.GetFrameTime();

            // Apply gravity
            ApplyForce(new Vector2(0, 1000f));

            // Integrate motion
            Integrate(dt);
            movementRemainder += Velocity * dt;

            int moveX = (int)MathF.Floor(movementRemainder.X);
            int moveY = (int)MathF.Floor(movementRemainder.Y);
            movementRemainder.X -= moveX;
            movementRemainder.Y -= moveY;

            int x = (int)gridPos.X;
            int y = (int)gridPos.Y;

            // Vertical movement
            for (int i = 0; i < Math.Abs(moveY); i++)
            {
                int targetY = y + Math.Sign(moveY);
                if (!grid.IsValidCell(x, targetY)) return;

                if (grid.IsEmpty(x, targetY))
                {
                    grid.Swap(x, y, x, targetY);
                    y = targetY;
                    gridPos = new Vector2(x, y);
                    continue;
                }

                // Try sliding diagonally
                bool slid = false;
                if (grid.IsEmpty(x - 1, y + 1))
                {
                    grid.Swap(x, y, x - 1, y + 1);
                    x -= 1; y += 1; slid = true;
                }
                else if (grid.IsEmpty(x + 1, y + 1))
                {
                    grid.Swap(x, y, x + 1, y + 1);
                    x += 1; y += 1; slid = true;
                }

                if (slid)
                {
                    gridPos = new Vector2(x, y);
                    continue;
                }

                Velocity *= friction;
                break;
            }

            // Clamp velocity
            Velocity = Vector2.Clamp(Velocity, new Vector2(-speedClamp, -speedClamp), new Vector2(speedClamp, speedClamp));

            // Update worldPos for consistency
            worldPos = Utils.GridToWorld(gridPos);
        }
    }
}
