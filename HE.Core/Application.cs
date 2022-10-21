using HE.Core.Util;
using HE.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HE.Core
{
    public abstract class Application
    {
        internal string Title
        {
            get => title;
        }

        internal bool CloseOnRequested
        {
            get => closeOnRequest;
        }

        internal LoggerSettings LoggerSettings
        {
            get => loggerSettings;
        }

        internal WindowSettings WindowSettings
        {
            get => windowSettings;
        }

        private string title;
        private bool closeOnRequest;
        private LoggerSettings loggerSettings;
        private WindowSettings windowSettings;

        protected Application(string title)
        {
            this.title = title;
            closeOnRequest = true;
            loggerSettings = new LoggerSettings(title, LogType.ALL, LogType.ALL);
            windowSettings = new WindowSettings();
        }

        protected Application(string title, bool closeOnRequest)
        {
            this.title = title;
            this.closeOnRequest = closeOnRequest;
            loggerSettings = new LoggerSettings(title, LogType.ALL, LogType.ALL);
            windowSettings = new WindowSettings();
        }

        protected Application(string title, bool closeOnRequest, LoggerSettings loggerSettings)
        {
            this.title = title;
            this.closeOnRequest = closeOnRequest;
            this.loggerSettings = loggerSettings;
            windowSettings = new WindowSettings();
        }

        protected Application(string title, bool closeOnRequest, LoggerSettings loggerSettings, WindowSettings windowSettings)
        {
            this.title = title;
            this.closeOnRequest = closeOnRequest;
            this.loggerSettings = loggerSettings;
            this.windowSettings = windowSettings;
        }

        internal void Initialize()
        {
            OnInitialize();
        }

        protected abstract void OnInitialize();

        internal void Deinitialize()
        {
            OnDeinitialize();
        }

        protected abstract void OnDeinitialize();
    }
}
