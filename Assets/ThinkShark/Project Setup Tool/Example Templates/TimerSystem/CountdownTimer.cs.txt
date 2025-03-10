namespace ImprovedTimers {

    public class CountdownTimer : Timer {
        public CountdownTimer(float value) : base(value) { }

        public override void Tick(float deltaTime) {
            if (IsRunning && CurrentTime > 0) {
                CurrentTime -= deltaTime;
            }

            if (IsRunning && CurrentTime <= 0) {
                Stop();
            }
        }

        public bool IsFinished => CurrentTime <= 0;

        public void Reset() => CurrentTime = initialTime;

        public void Reset(float newTime) {
            initialTime = newTime;
            Reset();
        }
    }
}
