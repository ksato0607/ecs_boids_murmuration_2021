using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
namespace ColdShowerGames {
    public class Spawner : MonoBehaviour {
        [SerializeField] private int nrToSpawn;
        [SerializeField] private GameObject spawnPrefab;
        [SerializeField] private float radius;
        [SerializeField] private Vector3 center;
        private BlobAssetStore _blobAssetStore;

        private EntityManager _entityManager;
        private Entity _spawnEntity;

        private void OnEnable() {
            Init();
        }

        private void OnDisable() {
            _blobAssetStore.Dispose();
        }

        private void OnDrawGizmosSelected() {
            Gizmos.DrawWireSphere(center, radius);
        }

        private void Init() {
            _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            _blobAssetStore = new BlobAssetStore();
            var settings =
                GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld,
                    _blobAssetStore);
            _spawnEntity = GameObjectConversionUtility.ConvertGameObjectHierarchy(spawnPrefab, settings);

        }

        public void Spawn() {
            for (var i = 0; i < nrToSpawn; i++) {
                var spawn = _entityManager.Instantiate(_spawnEntity);
                _entityManager.AddComponentData(spawn, new Translation {
                    Value = Random.insideUnitSphere * radius + center
                });
            }
        }
    }
}
