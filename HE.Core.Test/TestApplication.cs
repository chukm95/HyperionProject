using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HE.Core.Test
{
    internal class TestApplication : Application
    {
        public TestApplication() : base("Test application")
        {

        }

        protected override void OnInitialize()
        {
            Core.LogHandle.WriteInfo("Client", "Client initialized!");
        }

        protected override void OnDeinitialize()
        {
            Core.LogHandle.WriteInfo("Client", "Client deinitialized!");
        }
    }
}
