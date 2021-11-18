using Unity.Entities;
using Unity.Mathematics;
namespace ColdShowerGames {
    
    public struct Boid : ISharedComponentData {
        public float AddedCellRadius;
        public float SeparationWeight;
        public float CohesionWeight;
        public float AlignmentWeight;
        public float TargetWeight;
        public float TargetAvoidanceWeight;
        public float MoveSpeed;
        public float TurnSpeed;
        public float3 Scale;
        public float MaintainAvgYWeight;
    }
}
