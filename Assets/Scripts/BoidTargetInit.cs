using UnityEngine;

namespace ColdShowerGames {
    public class BoidTargetInit : MonoBehaviour {
        [SerializeField]
        private BoidTargetsManager boidTargetsManager;

        [SerializeField]
        private float avoidanceRadius = 3, weight = 1;
        

        private void Start() {
            boidTargetsManager.AddTarget(transform, weight, avoidanceRadius);
        }

        private void OnDestroy() {
            boidTargetsManager.RemoveTarget(transform);
        }

        private void OnDrawGizmos() {
            Gizmos.color = new Color(1, 1, 1, .4f);
            Gizmos.DrawWireSphere(transform.position, avoidanceRadius);
        }
    }
}
