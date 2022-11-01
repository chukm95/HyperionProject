using HE.Core.FileManagement;
using HE.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HE.Core.Rendering.Shaders
{
    public class ShaderManager
    {
        private int idCounter;
        private Queue<int> unusedShaderIds;
        private object shaderIdCreationLock;

        private Dictionary<string, Shader> shadersByName;
        private Dictionary<int, Shader> shadersById;

        private List<ShaderCreationObject> shadersToInit;
        private object shaderToInitWriteLock;
        private object shaderToInitReadLock;

        internal ShaderManager()
        {
            idCounter = 0;
            unusedShaderIds = new Queue<int>();
            shaderIdCreationLock = new object();

            shadersByName = new Dictionary<string, Shader>();
            shadersById = new Dictionary<int, Shader>();

            shadersToInit = new List<ShaderCreationObject>();
            shaderToInitWriteLock = new object();
            shaderToInitReadLock = new object();
        }

        public Shader CreateShader(string path)
        {
            FileHandle fh = Core.FileManager.GetFileHandle(path, Core.LogHandle);
            Shader shader = null;

            if(fh != null)
            {
                lock (shaderIdCreationLock)
                {
                    if (shadersByName.ContainsKey(path))
                    {
                        shader = shadersByName[path];
                    }
                    else
                    {
                        int newShaderId = CreateId();
                        shader = new Shader(newShaderId, fh);
                        shadersByName.Add(path, shader);
                        shadersById.Add(newShaderId, shader);
                        Core.TaskManager.QueueTask(shader);
                    }
                }
            }

            return shader;
        }

        private int CreateId()
        {
            int newId;

            if (unusedShaderIds.Count > 0)
                newId = unusedShaderIds.Dequeue();
            else
                newId = idCounter++;

            return newId;
        }

        internal void SubmitForCreation(ShaderCreationObject shaderCreationObject)
        {
            lock(shaderToInitWriteLock)
            {
                lock(shaderToInitReadLock)
                {
                    shadersToInit.Add(shaderCreationObject);
                }
            }
        }

        internal void Update(LogHandle logHandle)
        {
            lock (shaderToInitReadLock)
            {
                foreach(ShaderCreationObject sco in shadersToInit)
                {
                    sco.Create(logHandle);
                }
                shadersToInit.Clear();
            }
        }

        internal void Deinitialize()
        {
            foreach(Shader shader in shadersById.Values)
            {
                shader.Deinitialize();
            }
        }
    }
}
