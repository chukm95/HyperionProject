using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HE.Core.FileManagement
{
    public delegate void OnFileChanged();

    public class FileHandle
    {
        public string Path
        {
            get => Path;
        }

        public bool IsInternal
        {
            get => isInternal;
        }

        public DateTime LastWrite
        {
            get => lastWriteTime;
        }

        private string path;
        private Assembly assembly;
        private DateTime lastWriteTime;
        private bool isInternal;

        public OnFileChanged OnFileChanged;

        internal FileHandle(string path)
        {
            this.path = path;
            lastWriteTime = File.GetLastWriteTime(path);
            isInternal = false;
        }

        internal FileHandle(string path, Assembly assembly)
        {
            this.path = path;
            this.assembly = assembly;
            isInternal = true;
        }

        internal void CheckFileChanges()
        {
            DateTime currentLastWriteTime = File.GetLastWriteTime(path);
            if(DateTime.Compare(currentLastWriteTime, lastWriteTime) != 0)
            {
                lastWriteTime = currentLastWriteTime;
                OnFileChanged?.Invoke();
            }
        }

        public Stream Open()
        {
            if (isInternal)
                return assembly.GetManifestResourceStream(path);
            else
                return File.Open(path, FileMode.Open);
        }
    }
}
