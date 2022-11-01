using HE.Core.FileManagement;
using HE.Core.TaskManagement;
using HE.Logging;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HE.Core.Rendering.Shaders
{
    public class Shader : ITask
    {
        private const string VERTEX_TAG = "$VERTEX";
        private const string FRAGMENT_TAG = "$FRAGMENT";

        public string Path
        {
            get => fileHandle.Path;
        }

        public DateTime ShaderSourceTime
        {
            get => currentShaderSourceTime;
        }

        private int id;
        private FileHandle fileHandle;
        private DateTime currentShaderSourceTime;

        private StringBuilder vertexShaderSource;
        private StringBuilder fragmentShaderSource;

        private int gl_program;
        private ShaderUniform[] uniforms;
        private bool isReady;

        internal Shader(int id, FileHandle fileHandle)
        {
            this.id = id;
            this.fileHandle = fileHandle;
            currentShaderSourceTime = DateTime.MinValue;

            vertexShaderSource = new StringBuilder();
            fragmentShaderSource = new StringBuilder();

            gl_program = -1;
            isReady = false;

            fileHandle.OnFileChanged += () => { Core.TaskManager.QueueTask(this); };
        }

        public void OnExecution(LogHandle logHandle)
        {
            try
            {
                vertexShaderSource.Clear();
                fragmentShaderSource.Clear();

                string line = null;
                StringBuilder currentBuilder = null;
                using(StreamReader sr = new StreamReader(fileHandle.Open()))
                {
                    while((line = sr.ReadLine())!= null)
                    {
                        switch(line)
                        {
                            case VERTEX_TAG:
                                currentBuilder = vertexShaderSource;
                                break;
                            case FRAGMENT_TAG:
                                currentBuilder = fragmentShaderSource;
                                break;
                            default:
                                currentBuilder?.AppendLine(line);
                                break;
                        }
                    }
                    currentShaderSourceTime = fileHandle.LastWrite;
                    ShaderCreationObject sco = new ShaderCreationObject(this, vertexShaderSource.ToString(), fragmentShaderSource.ToString());
                    Renderer.ShaderManager.SubmitForCreation(sco);
                }
            }
            catch(Exception e)
            {
                logHandle.WriteError("Shader source loading!", e.ToString());
            }
        }

        internal void SetShader(int gl_program, ShaderUniform[] uniforms)
        {
            if (this.gl_program != -1)
                GL.DeleteProgram(this.gl_program);

            this.gl_program = gl_program;
        }

        internal void Deinitialize()
        {
            if (gl_program != -1)
                GL.DeleteProgram(gl_program);
        }

        
    }
}
