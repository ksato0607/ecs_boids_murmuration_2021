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
        public float MoveSpeed = 25.0f;
        public float TurnSpeed = .1f;
        public float MaintainAvgYWeight = .5f;
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
                        Scale = localScale,
                        TurnSpeed = input.TurnSpeed,
                        MaintainAvgYWeight = input.MaintainAvgYWeight
                    });
                
                
                DstEntityManager.RemoveComponent<Translation>(entity);
                DstEntityManager.RemoveComponent<Rotation>(entity);
            });
        }
    }
}
