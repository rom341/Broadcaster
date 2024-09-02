using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace BroadcastServer.Data
{
    public class FrequencyCounter
    {
        private double timeInterval; // Time interval in milliseconds
        private Timer timer;
        private int counter;
        private double frequency;
        private Action onTimerElapsedAction;

        public FrequencyCounter(double timeIntervalInSeconds, Action onTimerElapsedAction = null)
        {
            timeInterval = timeIntervalInSeconds * 1000;
            counter = 0;
            timer = new Timer(timeInterval);
            timer.Elapsed += OnTimerElapsed;
            this.onTimerElapsedAction = onTimerElapsedAction;
        }

        public void Start()
        {
            counter = 0;
            frequency = 0;
            timer.Start();
        }

        public void Stop()
        {
            timer.Stop();
        }

        public void IncrementCounter()
        {
            counter++;
        }

        public double GetFrequency()
        {
            return frequency;
        }

        public void SetCounter(int value)
        {
            counter = value;
        }

        public void SetTimeInterval(double newTimeIntervalInSeconds)
        {
            timeInterval = newTimeIntervalInSeconds * 1000;
            timer.Interval = timeInterval;
        }

        public void SetOnTimerElapsedAction(Action action)
        {
            onTimerElapsedAction = action;
        }

        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            frequency = counter / (timeInterval / 1000.0);
            counter = 0;

            onTimerElapsedAction?.Invoke();
        }
    }
}
