using System.Numerics;
using Raylib_cs;
using Ation.Common;
using Ation.ParticleSimulation;

namespace Ation.Entities
{
    class Player
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public Vector2 Size = new Vector2(10, 10); // 10x10 pixels player box

        private const float Speed = 100f;      // Move speed in pixels/sec
        private const float JumpForce = -200f; // Upward jump velocity
        private const float Gravity = 500f;    // Gravity in pixels/sec^2

        private ParticleSim particleSim;

        public Player(ParticleSim particleSimRef)
        {
            particleSim = particleSimRef;
            Position = new Vector2(100, 100);
            Velocity = Vector2.Zero;
        }

        public void Update(float dt)
        {
            // Apply gravity
            Velocity.Y += Gravity * dt;

            // Predict next position
            Vector2 nextPos = Position + Velocity * dt;

            // Horizontal collision
            Vector2 horizontalMove = new Vector2(nextPos.X, Position.Y);
            if (!IsColliding(horizontalMove) && Utils.IsInGridBounds(horizontalMove))
                Position.X = horizontalMove.X;
            else
                Velocity.X = 0;

            // Vertical collision
            Vector2 verticalMove = new Vector2(Position.X, nextPos.Y);
            if (!IsColliding(verticalMove) && Utils.IsInGridBounds(verticalMove))
                Position.Y = verticalMove.Y;
            else
                Velocity.Y = 0;

            // Final clamping to make absolutely sure player is inside window
            Position.X = Math.Clamp(Position.X, 0, Variables.WindowWidth - Size.X);
            Position.Y = Math.Clamp(Position.Y, 0, Variables.WindowHeight - Size.Y);
        }


        public void MoveLeft()
        {
            Velocity.X = -Speed;
        }

        public void MoveRight()
        {
            Velocity.X = Speed;
        }

        public void Jump()
        {
            if (IsOnGround())
                Velocity.Y = JumpForce;
        }

        public void StopHorizontal()
        {
            Velocity.X = 0;
        }

        public bool IsOnGround()
        {
            Vector2 feet = new Vector2(Position.X, Position.Y + Size.Y + 1);
            return particleSim.IsOccupied(feet);
        }

        private bool IsColliding(Vector2 checkPos)
        {
            // Check 4 corners of the player's box
            Vector2 topLeft = checkPos;
            Vector2 topRight = checkPos + new Vector2(Size.X, 0);
            Vector2 bottomLeft = checkPos + new Vector2(0, Size.Y);
            Vector2 bottomRight = checkPos + Size;

            return particleSim.IsOccupied(topLeft) ||
                   particleSim.IsOccupied(topRight) ||
                   particleSim.IsOccupied(bottomLeft) ||
                   particleSim.IsOccupied(bottomRight);
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
        }
    }
}
