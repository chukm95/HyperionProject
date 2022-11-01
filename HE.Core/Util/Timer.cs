using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HE.Core.Util
{
    public struct Timer
    {
        public bool HasElapsed
        {
            get => hasElapsed;
        }

        private TimeSpan interval;
        private TimeSpan elapsed;
        private bool repeating;
        private bool hasElapsed;

        public Timer(TimeSpan interval, bool repeating)
        {
            this.interval = interval;
            elapsed = TimeSpan.Zero;
            this.repeating = repeating;
            hasElapsed = false;
        }

        public static void UpdateTimer(ref Timer timer, TimeSpan gameTime)
        {
            timer.elapsed += gameTime;

            if(timer.elapsed > timer.interval)
            {
                if(timer.repeating)
                    timer.elapsed -= timer.interval;

                timer.hasElapsed = true;
            }
            else
            {
                timer.hasElapsed = false;
            }
        }
    }
}
