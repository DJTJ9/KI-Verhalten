using System;
using UnityEngine;

namespace ImprovedTimers {
    public abstract class Timer : IDisposable {
        public float CurrentTime { get; protected set; }

        public bool IsRunning { get; protected set; }

        protected float initialTime;

        public float Progress => Mathf.Clamp(CurrentTime, 0f, 1f);

        public Action OnTimerStart = delegate { };
        public Action OnTimerStop = delegate { };

        bool disposed;

        protected Timer(float value) {
            initialTime = value;
            IsRunning = false;
        }

        public void Start() {
            CurrentTime = initialTime;
            if (!IsRunning) {
                IsRunning = true;
                TimerManager.RegisterTimer(this);
                OnTimerStart.Invoke();
            }
        }

        public void Stop() {
            CurrentTime = initialTime;
            if (IsRunning) {
                IsRunning = false;
                TimerManager.DeregisterTimer(this);
                OnTimerStop.Invoke();
            }
        }

        public abstract void Tick(float deltaTime);
        //public virtual bool IsFinished { get; }

        public void Resume() => IsRunning = true;
        public void Pause() => IsRunning = false;

        //public virtual void Reset() => CurrentTime = initialTime;
        //public virtual void Reset(float newTime)
        //{
        //    initialTime = newTime;
        //    Reset();
        //}

        ~Timer() {
            Dispose(false);
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            if (disposed) return;

            if (disposing) {
                TimerManager.DeregisterTimer(this);
            }

            disposed = true;
        }
    }
}