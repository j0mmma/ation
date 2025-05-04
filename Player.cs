// using System.Numerics;
// using Raylib_cs;
// using Ation.GameWorld; // For World

// namespace Ation.Entities
// {
//     public class Player
//     {
//         public Vector2 Position;
//         public Vector2 Velocity;
//         public Vector2 Size = new Vector2(0.6f, 1.2f); // in world units

//         private const float Gravity = 20f;
//         private const float MoveSpeed = 5f;
//         private const float JumpVelocity = -8f;

//         public void Update(float dt, World world)
//         {
//             // Input
//             if (Raylib.IsKeyDown(KeyboardKey.D)) Velocity.X = -MoveSpeed;
//             else if (Raylib.IsKeyDown(KeyboardKey.A)) Velocity.X = MoveSpeed;
//             else Velocity.X = 0f;

//             if (IsGrounded(world) && Raylib.IsKeyPressed(KeyboardKey.W))
//                 Velocity.Y = JumpVelocity;

//             // Gravity
//             Velocity.Y += Gravity * dt;

//             // Axis-aligned movement
//             TryMove(new Vector2(Velocity.X * dt, 0f), world);
//             TryMove(new Vector2(0f, Velocity.Y * dt), world);
//         }

//         public void Draw(float tileSize)
//         {
//             Raylib.DrawRectangle(
//                 (int)(Position.X * tileSize),
//                 (int)(Position.Y * tileSize),
//                 (int)(Size.X * tileSize),
//                 (int)(Size.Y * tileSize),
//                 Color.Blue
//             );
//         }

//         private void TryMove(Vector2 delta, World world)
//         {
//             Vector2 newPos = Position + delta;
//             if (!CollidesAt(newPos, world))
//             {
//                 Position = newPos;
//             }
//             else
//             {
//                 // Stop blocked axis
//                 if (delta.Y != 0) Velocity.Y = 0;
//                 if (delta.X != 0) Velocity.X = 0;
//             }
//         }

//         private bool CollidesAt(Vector2 pos, World world)
//         {
//             int minX = (int)MathF.Floor(pos.X);
//             int maxX = (int)MathF.Floor(pos.X + Size.X);
//             int minY = (int)MathF.Floor(pos.Y);
//             int maxY = (int)MathF.Floor(pos.Y + Size.Y);

//             for (int y = minY; y <= maxY; y++)
//             {
//                 for (int x = minX; x <= maxX; x++)
//                 {
//                     if (world.IsCollidableAt(x, y)) return true;
//                 }
//             }

//             return false;
//         }

//         private bool IsGrounded(World world)
//         {
//             Vector2 probe = new Vector2(Position.X + Size.X * 0.5f, Position.Y + Size.Y + 0.05f);
//             return world.IsCollidableAt((int)probe.X, (int)probe.Y);
//         }
//     }
// }
