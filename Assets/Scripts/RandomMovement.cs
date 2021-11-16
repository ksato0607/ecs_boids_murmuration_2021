using UnityEngine;
namespace ColdShowerGames {
    public class RandomMovement : MonoBehaviour {
        private Transform _transform;
        private Vector3 _originPos;
        public new Transform transform => _transform ? _transform : _transform = gameObject.transform;

        [SerializeField]
        private float movementSpeedRandom1,
            movementSpeedRandom2,
            movementSpeedRandom3,
            varySpeed1,
            varySpeed2,
            varySpeed3,
            length;



        private void Start() {
            _originPos = transform.position;
        }

        private void Update() {
            transform.position = _originPos +
                                 new Vector3(Mathf.Sin(Time.time * movementSpeedRandom1 * varySpeed1),
                                     Mathf.Cos(Time.time * movementSpeedRandom2 * varySpeed2),
                                     Mathf.Sin(Time.time * movementSpeedRandom3 * varySpeed3)) * length;
        }
    }
}
