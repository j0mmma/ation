using System.Numerics;
using Raylib_cs;
using Ation.Common;

namespace Ation.Simulation
{
    public abstract class MovableSolid : Material
    {
        protected float friction = 0.9f;
        protected float speedClamp = 200f;
        public override bool IsCollidable => true;

        public MovableSolid(Vector2 worldPos) : base(worldPos) { }

        public override void Step(IMaterialContext grid)
        {
            float dt = Raylib.GetFrameTime();

            UpdatedThisFrame = true;
            IsActive = false;

            ApplyForce(FallingSandSim.Gravity);

            // Slow down if inside liquid
            var below = grid.Get((int)gridPos.X, (int)gridPos.Y + 1);
            if (below is Liquid lq)
            {
                Velocity.Y *= lq.VerticalDamping;
                Velocity.X += Raylib.GetRandomValue(-1000, 1000) / 1000f * lq.TurbulenceStrength;

            }
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
                bool interacted = false;

                if (neighbor != null)
                {
                    if (neighbor is Liquid liquid)
                    {
                        interacted = ActOnLiquid(liquid, x, y, x, targetY, grid);
                    }
                    else if (neighbor is ImmovableSolid immovable)
                    {
                        interacted = ActOnImmovableSolid(immovable);
                    }

                    if (interacted)
                    {
                        y = targetY;
                        gridPos = new Vector2(x, y);
                        IsActive = true;

                        int nextY = y + Math.Sign(moveY);
                        if (grid.IsValidCell(x, nextY) && grid.IsEmpty(x, nextY))
                        {
                            continue; // still falling after swap
                        }
                    }
                }

                // IMPORTANT: Always try sliding after interaction (or failed interaction)
                bool slid = TrySlide(grid, ref x, ref y);
                if (slid)
                {
                    gridPos = new Vector2(x, y);
                    IsActive = true;
                    continue;
                }

                // Fully stuck: friction and stop
                Velocity *= friction;
                break;
            }

            Velocity = Vector2.Clamp(Velocity, new Vector2(-speedClamp, -speedClamp), new Vector2(speedClamp, speedClamp));
            worldPos = Utils.GridToWorld(gridPos);
        }



        protected virtual bool ActOnLiquid(Liquid liquid, int selfX, int selfY, int targetX, int targetY, IMaterialContext grid)
        {
            // Direct swap with liquid
            grid.Swap(selfX, selfY, targetX, targetY);

            var temp = liquid.gridPos;
            liquid.gridPos = new Vector2(selfX, selfY);
            this.gridPos = new Vector2(targetX, targetY);

            SetActive();
            liquid.SetActive();

            return true;
        }

        protected virtual bool ActOnImmovableSolid(ImmovableSolid solid)
        {
            // Touching immovable → turn red
            SetActive();
            return false;
        }
        private bool TrySlide(IMaterialContext grid, ref int x, ref int y)
        {
            bool preferLeftFirst = Raylib.GetRandomValue(0, 1) == 0;

            if (preferLeftFirst)
            {
                // Try left first
                if (grid.IsValidCell(x - 1, y + 1))
                {
                    var leftDown = grid.Get(x - 1, y + 1);
                    if (leftDown == null || leftDown is Liquid)
                    {
                        grid.Swap(x, y, x - 1, y + 1);
                        x -= 1;
                        y += 1;
                        return true;
                    }
                }

                // Then right
                if (grid.IsValidCell(x + 1, y + 1))
                {
                    var rightDown = grid.Get(x + 1, y + 1);
                    if (rightDown == null || rightDown is Liquid)
                    {
                        grid.Swap(x, y, x + 1, y + 1);
                        x += 1;
                        y += 1;
                        return true;
                    }
                }
            }
            else
            {
                // Try right first
                if (grid.IsValidCell(x + 1, y + 1))
                {
                    var rightDown = grid.Get(x + 1, y + 1);
                    if (rightDown == null || rightDown is Liquid)
                    {
                        grid.Swap(x, y, x + 1, y + 1);
                        x += 1;
                        y += 1;
                        return true;
                    }
                }

                // Then left
                if (grid.IsValidCell(x - 1, y + 1))
                {
                    var leftDown = grid.Get(x - 1, y + 1);
                    if (leftDown == null || leftDown is Liquid)
                    {
                        grid.Swap(x, y, x - 1, y + 1);
                        x -= 1;
                        y += 1;
                        return true;
                    }
                }
            }

            return false;
        }



    }
}