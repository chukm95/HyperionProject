using HE.Logging;
using HE.Core.Util;
using System.Reflection;
using Timer = HE.Core.Util.Timer;

namespace HE.Core.FileManagement
{
    public class FileManager
    {
        private Timer fileChangeCheckTimer;
        private Dictionary<string, FileHandle> fileHandles;

        internal FileManager()
        {
            fileChangeCheckTimer = new Timer(TimeSpan.FromSeconds(10), true);
            fileHandles = new Dictionary<string, FileHandle>();

            //search for internal files
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                string[] resources = assembly.GetManifestResourceNames();
                foreach (string resourcePath in resources)
                {
                    if (resourcePath.Contains(".Assets."))
                    {
                        Core.LogHandle.WriteInfo("FileManager", string.Format("Found internal resource: {0}!", resourcePath));
                        fileHandles.Add(resourcePath, new FileHandle(resourcePath, assembly));
                    }
                }
            }
        }

        public FileHandle GetFileHandle(string path, LogHandle logHandle)
        {
            FileHandle fh = null;

            if (string.IsNullOrEmpty(path))
                return fh;

            bool isExternal = path.StartsWith("*");

            if (isExternal)
            {
                path = GetUniformPath(path);

                if (fileHandles.ContainsKey(path))
                {
                    fh = fileHandles[path];
                }
                else
                {
                    if (File.Exists(path))
                    {
                        fh = new FileHandle(path);
                        fileHandles.Add(path, fh);
                    }
                    else
                    {
                        Core.LogHandle.WriteWarning("FileManager", string.Format("Cannot find file {0}!", path));
                    }
                }
            }
            else
            {
                if (fileHandles.ContainsKey(path))
                    fh = fileHandles[path];
            }

            return fh;
        }

        private static string GetUniformPath(string path)
        {
            path = path.Remove(0, 1);
            path = path.Replace("\\", Path.DirectorySeparatorChar.ToString());
            path = path.Replace("/", Path.DirectorySeparatorChar.ToString());
            string directory = Directory.GetCurrentDirectory();
            string filePath = string.Concat(directory, path);
            return filePath;
        }

        internal void Update()
        {
            Timer.UpdateTimer(ref fileChangeCheckTimer, Core.GameTime.DeltaTime);
            if(fileChangeCheckTimer.HasElapsed)
            {
                foreach(FileHandle fh in fileHandles.Values)
                {
                    if(!fh.IsInternal)
                        fh.CheckFileChanges();
                }
            }
        }
    }
}
