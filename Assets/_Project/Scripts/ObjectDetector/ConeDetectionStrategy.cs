using ImprovedTimers;
using UnityEngine;

    public class ConeDetectionStrategy : IDetectionStrategy {
        readonly float detectionAngle;
        readonly float detectionRadius;
        readonly float innerDetectionRadius;

        public ConeDetectionStrategy(float _detectionAngle, float _detectionRadius, float _innerDetectionRadius) {
            this.detectionAngle = _detectionAngle;
            this.detectionRadius = _detectionRadius;
            this.innerDetectionRadius = _innerDetectionRadius;
        }

        public bool Execute(Transform _objectToChase, Transform _detector, CountdownTimer _timer) {
            if (_timer.IsRunning) return false;

            var directionToPlayer = _objectToChase.position - _detector.position;
            var angleToPlayer = Vector3.Angle(directionToPlayer, _detector.forward);

            // If the player is not within the detection angle + outer radius (aka the cone in front of the dog)

            if ((!(angleToPlayer < detectionAngle / 2) || !(directionToPlayer.magnitude < detectionRadius)))/* && !(directionToPlayer.magnitude < innerDetectionRadius))*/
                return false;

            _timer.Start();
            return true;
        }
    }