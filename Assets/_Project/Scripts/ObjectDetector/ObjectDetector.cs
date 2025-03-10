using ImprovedTimers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    public class ObjectDetector : MonoBehaviour {
        [SerializeField] public Transform ObjectToDetect;

        [SerializeField] float detectionRadius;
        // [SerializeField] float detectionAngle;
        // [SerializeField] float innerDetectionRadius;
        [SerializeField] float detectionCooldown;
        
        CountdownTimer detectionTimer;

        IDetectionStrategy detectionStrategy;

        private void Start() {
            detectionTimer = new CountdownTimer(detectionCooldown);
            // detectionStrategy = new ConeDetectionStrategy(detectionAngle, detectionRadius, innerDetectionRadius);
            detectionStrategy = new RadiusDetectionStrategy(detectionRadius);
        }

        void Update() => detectionTimer.Tick(Time.deltaTime);

        public bool CanDetectObject() {
            return detectionTimer.IsRunning || detectionStrategy.Execute(ObjectToDetect, transform, detectionTimer);
        }

        // Methode to set strategy in a factory
        public void SetDetectionStrategy(IDetectionStrategy _detectionStrategy) => this.detectionStrategy = _detectionStrategy;
        
        public float GetRadius() => detectionRadius;

        public void SetRadius(float newRadius) => detectionRadius = newRadius;

        private void OnDrawGizmos() {
            Gizmos.color = Color.green;

            // Draws a sphere for the radii
           Gizmos.DrawWireSphere(transform.position, detectionRadius);
           // Gizmos.DrawWireSphere(transform.position, innerDetectionRadius);

            // Calculate the cone directions
            // Vector3 forwardConeDirection = Quaternion.Euler(0, detectionAngle / 2, 0) * transform.forward * detectionRadius;
            // Vector3 backwardConeDirection = Quaternion.Euler(0, -detectionAngle / 2, 0) * transform.forward * detectionRadius;

            // Draws lines to represent the cone
            // Gizmos.DrawLine(transform.position, transform.position + forwardConeDirection);
            // Gizmos.DrawLine(transform.position, transform.position + backwardConeDirection);
        }
    }
