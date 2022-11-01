using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HE.Core.Util
{
    public class GameTime
    {
        public TimeSpan DeltaTime
        {
            get => deltaTime;
        }

        public TimeSpan ElapsedTime
        {
            get => elapsedTime;
        }

        private Stopwatch stopwatch;
        private TimeSpan deltaTime;
        private TimeSpan elapsedTime;

        internal GameTime()
        {
            stopwatch = new Stopwatch();
            deltaTime = TimeSpan.Zero;
            elapsedTime = TimeSpan.Zero;
        }

        internal void Update()
        {
            deltaTime = stopwatch.Elapsed;
            elapsedTime += deltaTime;
            stopwatch.Restart();
        }
    }
}
