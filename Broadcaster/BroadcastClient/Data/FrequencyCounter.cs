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
            timeInterval = timeIntervalInSeconds * 1000; // Convert seconds to milliseconds
            counter = 0;
            timer = new Timer(timeInterval);
            timer.Elapsed += OnTimerElapsed;
            this.onTimerElapsedAction = onTimerElapsedAction;
        }

        // Start the timer
        public void Start()
        {
            counter = 0; // Reset the counter when starting
            frequency = 0; // Reset the frequency
            timer.Start();
        }

        // Stop the timer
        public void Stop()
        {
            timer.Stop();
        }

        // Increment the counter
        public void IncrementCounter()
        {
            counter++;
        }

        // Get the current frequency
        public double GetFrequency()
        {
            return frequency;
        }

        // Set a new counter value
        public void SetCounter(int value)
        {
            counter = value;
        }

        // Set a new time interval (in seconds)
        public void SetTimeInterval(double newTimeIntervalInSeconds)
        {
            timeInterval = newTimeIntervalInSeconds * 1000;
            timer.Interval = timeInterval;
        }

        // Set a new lambda function to execute on timer elapsed
        public void SetOnTimerElapsedAction(Action action)
        {
            onTimerElapsedAction = action;
        }

        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            frequency = counter / (timeInterval / 1000.0); // Calculate frequency
            counter = 0; // Reset the counter after each interval

            onTimerElapsedAction?.Invoke(); // Execute the lambda function if provided
        }
    }
}
