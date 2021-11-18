using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
namespace ColdShowerGames {
    public class BoidTargetsManager : MonoBehaviour {

        [SerializeField]
        private List<Transform> initialTargets = new();
        [SerializeField]
        private float initialTargetsWeight = 1f, initialTargetsAvoidanceRadius = 2f;

        private Dictionary<Transform, Entity> targets = new();
        private EntityManager _entityManager;
        private EntityArchetype _entityArchetype;

        private void Awake() {
            _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            _entityArchetype = _entityManager.CreateArchetype(ComponentType.ReadOnly<BoidTarget>(),
                ComponentType.ReadWrite<LocalToWorld>());
            
            initialTargets.ForEach(trans => AddTarget(trans, initialTargetsWeight, initialTargetsAvoidanceRadius));
        }

        private void Update() {
            foreach (var (targetTrans, entity) in targets) {
                _entityManager.SetComponentData(entity,
                    new LocalToWorld {
                        Value = float4x4.TRS(targetTrans.position, quaternion.identity, float3.zero)
                    });
            }
        }

        /// <summary>
        /// Add a target for the boids to follow.
        /// </summary>
        /// <param name="targetTrans">The target to follow</param>
        /// <param name="weight">How should the boids prioritize following this target. (1 = default behavior)</param>
        /// <param name="avoidanceRadius">The distance for the boids to keep away from the target.</param>
        public void AddTarget(Transform targetTrans, float weight = 1, float avoidanceRadius = 2f) {
            var entity = _entityManager.CreateEntity(_entityArchetype);

            _entityManager.AddComponentData(entity,
                new LocalToWorld {
                    Value = float4x4.TRS(float3.zero, quaternion.identity, float3.zero)
                });
            _entityManager.AddComponentData(entity,
                new BoidTarget {
                    Weight = weight,
                    AvoidanceRadius = avoidanceRadius,
                });
            targets.Add(targetTrans, entity);
        }

        /// <summary>
        /// Remove a target for the boids to follow.
        /// </summary>
        public void RemoveTarget(Transform targetTrans) {
            var entity = targets[targetTrans];
            _entityManager.DestroyEntity(entity);
            targets.Remove(targetTrans);
        }
    }
}
