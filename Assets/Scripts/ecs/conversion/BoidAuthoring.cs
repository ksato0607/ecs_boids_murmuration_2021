using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
namespace ColdShowerGames {
    public class BoidAuthoring : MonoBehaviour {
        public float AddedCellRadius = 8.0f;
        public float SeparationWeight = 1.0f;
        public float CohesionWeight = 1.0f;
        public float AlignmentWeight = 1.0f;
        public float TargetWeight = 2.0f;
        public float TargetAvoidanceWeight = 2.0f;
        public float ObstacleAversionDistance = 30.0f;
        public float MoveSpeed = 25.0f;
        public float TurnSpeed = .1f;
    }

    public class BoidConversion : GameObjectConversionSystem {

        protected override void OnUpdate() {
            Entities.ForEach((BoidAuthoring input) => {
                var entity = GetPrimaryEntity(input);
                float3 localScale = input.transform.localScale;
                
                DstEntityManager.AddSharedComponentData(entity,
                    new Boid {
                        AddedCellRadius = input.AddedCellRadius,
                        AlignmentWeight = input.AlignmentWeight,
                        CohesionWeight = input.CohesionWeight,
                        MoveSpeed = input.MoveSpeed,
                        SeparationWeight = input.SeparationWeight,
                        TargetWeight = input.TargetWeight,
                        TargetAvoidanceWeight = input.TargetAvoidanceWeight,
                        ObstacleAversionDistance = input.ObstacleAversionDistance,
                        Scale = localScale,
                        TurnSpeed = input.TurnSpeed
                    });
                
                
                DstEntityManager.RemoveComponent<Translation>(entity);
                DstEntityManager.RemoveComponent<Rotation>(entity);
            });
        }
    }
}
