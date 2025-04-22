using System.Numerics;
using Raylib_cs;
using Ation.Common;
using Ation.ParticleSimulation;

namespace Ation.Entities
{
    public interface ICollider
    {
        public Rectangle GetBounds();         // World space
        public Vector2 GetVelocity();         // Optional, for pushing logic
    }



    class Player : ICollider
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public Vector2 Size = new Vector2(30, 50); // Player hitbox size

        private const float Speed = 100f;      // Move speed in pixels/sec
        private const float JumpForce = -200f; // Upward jump velocity
        private const float Gravity = 500f;    // Gravity in pixels/sec^2

        private ParticleSim particleSim;
        public bool WandEnabled { get; set; } = false;

        private float wandLength = 20f; // pixels
        private float wandCooldown = 0.05f;
        private float wandTimer = 0f;

        public Player(ParticleSim particleSimRef)
        {
            particleSim = particleSimRef;
            Position = new Vector2(100, 100);
            Velocity = Vector2.Zero;
        }

        public void Update(float dt)
        {

            // Push up if embedded in particles
            if (IsColliding(Position))
            {
                const int maxPushSteps = 20;
                for (int i = 1; i <= maxPushSteps; i++)
                {
                    Vector2 pushed = new Vector2(Position.X, Position.Y - i);
                    if (!IsColliding(pushed) && Utils.IsPositionInsideWindow(pushed))
                    {
                        Position = pushed;
                        Velocity.Y = 0;
                        break;
                    }
                }
            }

            // Apply gravity
            Velocity.Y += Gravity * dt;

            // Predict next position
            Vector2 nextPos = Position + Velocity * dt;

            // Horizontal movement with step-up logic
            bool movedHorizontally = false;
            for (int step = 0; step <= Variables.PixelSize * 2; step += Variables.PixelSize)
            {
                Vector2 attemptPos = new Vector2(nextPos.X, Position.Y - step);
                if (!IsColliding(attemptPos) && Utils.IsPositionInsideWindow(attemptPos))
                {
                    Position.X = attemptPos.X;
                    Position.Y = attemptPos.Y;
                    movedHorizontally = true;
                    break;
                }
            }
            if (!movedHorizontally)
                Velocity.X = 0;

            // Vertical movement
            Vector2 verticalMove = new Vector2(Position.X, nextPos.Y);
            if (!IsColliding(verticalMove) && Utils.IsPositionInsideWindow(verticalMove))
                Position.Y = verticalMove.Y;
            else
                Velocity.Y = 0;

            // Clamp to screen
            Position.X = Math.Clamp(Position.X, 0, Variables.WindowWidth - Size.X);
            Position.Y = Math.Clamp(Position.Y, 0, Variables.WindowHeight - Size.Y);

            if (WandEnabled && Raylib.IsMouseButtonDown(MouseButton.Left))
                UpdateWand(dt);
        }


        private void UpdateWand(float dt)
        {
            wandTimer -= dt;

            if (Raylib.IsMouseButtonDown(MouseButton.Left) && wandTimer <= 0)
            {
                float intensity = 500f; // pixels per second, adjust to control stream power

                Vector2 playerCenter = Position + Size / 2f;
                Vector2 mouse = Raylib.GetMousePosition();
                Vector2 direction = Vector2.Normalize(mouse - playerCenter);
                Vector2 tip = playerCenter + direction * wandLength;

                Vector2 velocity = direction * intensity;

                //particleSim.AddParticle(tip, ParticleType.Water, radius: 1, initialVelocity: velocity);
                wandTimer = wandCooldown;
            }
        }


        private Vector2 GetWandTip()
        {
            Vector2 playerCenter = Position + Size / 2f;
            Vector2 mouse = Raylib.GetMousePosition();
            Vector2 direction = Vector2.Normalize(mouse - playerCenter);

            return playerCenter + direction * wandLength;
        }




        public void MoveLeft() => Velocity.X = -Speed;
        public void MoveRight() => Velocity.X = Speed;
        public void StopHorizontal() => Velocity.X = 0;

        public void Jump()
        {
            if (IsOnGround())
                Velocity.Y = JumpForce;
        }

        public bool IsOnGround()
        {
            int steps = (int)(Size.X / Variables.PixelSize);
            int tolerance = Variables.PixelSize;

            for (int i = 0; i <= steps; i++)
            {
                float x = Position.X + i * Variables.PixelSize;

                for (int offset = 1; offset <= tolerance; offset++)
                {
                    Vector2 check = new Vector2(x, Position.Y + Size.Y + offset);
                    if (particleSim.IsOccupied(check))
                        return true;
                }
            }

            return false;
        }


        private bool IsColliding(Vector2 checkPos)
        {
            int stepsX = (int)(Size.X / Variables.PixelSize);
            int stepsY = (int)(Size.Y / Variables.PixelSize);

            for (int x = 0; x <= stepsX; x++)
            {
                for (int y = 0; y <= stepsY; y++)
                {
                    Vector2 p = new Vector2(
                        checkPos.X + x * Variables.PixelSize,
                        checkPos.Y + y * Variables.PixelSize
                    );

                    if (particleSim.IsOccupied(p))
                        return true;
                }
            }

            return false;
        }


        public void Render()
        {
            Raylib.DrawRectangle(
                (int)Position.X,
                (int)Position.Y,
                (int)Size.X,
                (int)Size.Y,
                Color.Red
            );

            Raylib.DrawRectangleLines(
                (int)Position.X,
                (int)Position.Y,
                (int)Size.X,
                (int)Size.Y,
                Color.Black
            );

            // Draw wand
            Vector2 center = Position + Size / 2f;
            Vector2 tip = GetWandTip();
            Raylib.DrawLineEx(center, tip, 2f, Color.DarkBlue);
        }


        public Rectangle GetBounds()
        {
            // Shrink the top slightly to avoid displacing particles when gliding
            float yOffset = Variables.PixelSize * 0.5f;

            return new Rectangle(
                Position.X,
                Position.Y + yOffset,
                Size.X,
                Size.Y - yOffset
            );
        }

        public Vector2 GetVelocity()
        {
            return Velocity;
        }

    }
}
