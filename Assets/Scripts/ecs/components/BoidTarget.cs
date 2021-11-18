using Unity.Entities;
using Unity.Mathematics;
namespace ColdShowerGames {
    
    public struct BoidTarget : IComponentData {
        public float AvoidanceRadius;
        public float Weight;
    }
}
