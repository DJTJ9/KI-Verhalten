using ImprovedTimers;
using UnityEngine;

    public class RadiusDetectionStrategy : IDetectionStrategy {
        readonly float detectionRadius;

        public RadiusDetectionStrategy(float _detectionRadius) {
            this.detectionRadius = _detectionRadius;
        }

        public bool Execute(Transform _objectToChase, Transform _detector, CountdownTimer _timer) {
            if (_timer.IsRunning) return false;

            var directionToObject = _objectToChase.position - _detector.position;

            if (!(directionToObject.magnitude < detectionRadius)) {
                return false;
            }

            _timer.Start();
            return true;
        }
    } 
