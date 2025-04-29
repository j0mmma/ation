// using System.Numerics;
// using Raylib_cs;
// using Ation.Common;

// namespace Ation.Simulation
// {
//     public class Acid : Liquid
//     {
//         public override string DisplayName => "Acid";
//         public override MaterialType Type => MaterialType.Acid;

//         private static readonly Random rng = new();

//         public Acid(Vector2 worldPos) : base(worldPos)
//         {
//             Color = new Color(0, 255, 0, 255); // Bright green
//             Mass = 1.0f;
//         }

//         public override bool ActOnNeighbor(Material neighbor, int selfX, int selfY, int neighborX, int neighborY, SimulationGrid grid)
//         {
//             if (neighbor == null) return false;

//             // Try to corrode solids (movable or immovable)
//             if (neighbor is MovableSolid || neighbor is ImmovableSolid)
//             {
//                 if (rng.NextDouble() < 0.05) // 5% chance per frame
//                 {
//                     // Corrode the neighbor
//                     grid.Clear(targetX, targetY);

//                     // Chance to evaporate into vapor
//                     if (rng.NextDouble() < 0.3) // 30% chance
//                     {
//                         grid.Set((int)gridPos.X, (int)gridPos.Y, new AcidVapor(Utils.GridToWorld(gridPos)));
//                     }

//                     return true;
//                 }
//             }

//             return false;
//         }
//     }
// }
