using UnityEngine;
namespace ColdShowerGames {
    public class BoidTargetAuthoring : MonoBehaviour {
        public float AvoidanceRadius = 2f;

        private void OnDrawGizmos() {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, AvoidanceRadius);
        }
    }

    public class BoidTargetConversion : GameObjectConversionSystem {

        protected override void OnUpdate() {
            Entities.ForEach((BoidTargetAuthoring input) => {
                var entity = GetPrimaryEntity(input);
                
                DstEntityManager.AddComponentData(entity,
                    new BoidTarget() {
                        AvoidanceRadius = input.AvoidanceRadius
                    });
            });
        }
    }
}
