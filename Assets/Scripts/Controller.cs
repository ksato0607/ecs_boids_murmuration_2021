using Unity.Mathematics;
using UnityEngine;
namespace ColdShowerGames {
    public class Controller : MonoBehaviour {
        [SerializeField]
        private FlockSettings settings;

        private Spawner _spawner;

        public FlockSettings Settings => settings;

        public static Controller Instance { get; private set; }
        public float CellSizeVaried { get; private set; }
        public float3 CellPositionOffsetVaried { get; private set; }

        [SerializeField]
        private Transform cellOffsetTransform;

        [SerializeField]
        private bool showGridGizmos;
        

        private void Awake() {
            Instance = this;

            _spawner = GetComponent<Spawner>();
        }

        private void Start() {
            _spawner.Spawn();
        }

        private void Update() {
            if (Input.GetKeyDown(KeyCode.S)) {
                _spawner.Spawn();
            }

            CellSizeVaried =
                FromMinusOneOneToZeroOne(Mathf.Sin(Time.time * Settings.CellSizeVarySpeed)) *
                (Settings.CellSizeMax - Settings.CellSizeMin) +
                Settings.CellSizeMin;
            CellPositionOffsetVaried = cellOffsetTransform.position;
        }

        private float FromMinusOneOneToZeroOne(float f) {
            return f * 0.5f + 0.5f;
        }

        private void OnDrawGizmos() {
            if (!showGridGizmos) {
                return;
            }
            
            Gizmos.color = Color.green;
            if (Settings == null)
                return;

            var spatialHashSteps = 1;

            if (spatialHashSteps > 0) {
                Gizmos.color = new Color(1, 1, 0, .6f);
                for (int i = -spatialHashSteps; i <= spatialHashSteps; i++) {
                    for (int j = -spatialHashSteps; j <= spatialHashSteps; j++) {
                        for (int k = -spatialHashSteps; k <= spatialHashSteps; k++) {
                            Gizmos.DrawWireCube(
                                new Vector3(i, j, k) * CellSizeVaried + Vector3.one * CellSizeVaried * .5f +
                                (Vector3)CellPositionOffsetVaried,
                                CellSizeVaried * Vector3.one);
                        }
                    }
                }
                return;
            }
        }
    }
}
