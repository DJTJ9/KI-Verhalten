using ImprovedTimers;
using UnityEngine;

    public interface IDetectionStrategy {
        bool Execute(Transform _objectToChase, Transform _detector, CountdownTimer _timer);
    }
