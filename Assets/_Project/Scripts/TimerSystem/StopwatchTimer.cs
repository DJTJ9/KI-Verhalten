using UnityEngine;

namespace ImprovedTimers {
    public class StopwatchTimer : Timer {
        public StopwatchTimer() : base(0) { }

        public override void Tick(float deltaTime) {
            if (IsRunning) {
                CurrentTime += Time.deltaTime;
            }
        }

        public void Reset() => CurrentTime = 0;

        public float GetTime() => CurrentTime;
    }
}
