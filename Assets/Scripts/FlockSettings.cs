using UnityEngine;
namespace ColdShowerGames {
    [CreateAssetMenu(menuName = "ECS/Settings")]
    public class FlockSettings : ScriptableObject {
        [SerializeField]
        private float unitSpeed;
        public float UnitSpeed => unitSpeed;

        [SerializeField]
        private float cellSizeMax;

        public float CellSizeMax => cellSizeMax;


        [SerializeField]
        private float cellSizeMin;

        public float CellSizeMin => cellSizeMin;

        [SerializeField]
        private float cellSizeVarySpeed;

        public float CellSizeVarySpeed => cellSizeVarySpeed;
        
        
    }
}
