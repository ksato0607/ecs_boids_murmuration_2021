using Unity.Entities;
using Unity.Mathematics;
namespace ColdShowerGames {
    
    public struct Boid : ISharedComponentData {
        public float AddedCellRadius;
        public float SeparationWeight;
        public float CohesionWeight;
        public float AlignmentWeight;
        public float TargetWeight;
        public float ObstacleAversionDistance;
        public float MoveSpeed;
        public float3 Scale;
    }
}
