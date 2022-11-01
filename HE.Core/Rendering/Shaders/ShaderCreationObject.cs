using HE.Logging;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HE.Core.Rendering.Shaders
{
    internal class ShaderCreationObject
    {
        private const int UNIFORM_NAME_BUFF_SIZE = 60;

        private Shader parentShader;
        private DateTime shaderSourceTime;
        private string vertexShaderSource;
        private string fragmentShaderSource;
        private int gl_program;

        public ShaderCreationObject(Shader parentShader, string vertexShaderSource, string fragmentShaderSource)
        {
            this.parentShader = parentShader;
            shaderSourceTime = parentShader.ShaderSourceTime;
            this.vertexShaderSource = vertexShaderSource;
            this.fragmentShaderSource = fragmentShaderSource;
            gl_program = -1;
        }

        public void Create(LogHandle logHandle)
        {
            if(shaderSourceTime != parentShader.ShaderSourceTime)
            {
                logHandle.WriteWarning("Shader file changed", "Shader file changed again and this source will not be created!");
                return;
            }

            int vertexShaderId;
            int fragmentShaderId;

            bool vertexShaderSucces = CreateShader(out vertexShaderId, vertexShaderSource, ShaderType.VertexShader, logHandle);
            bool fragmentShaderSucces = CreateShader(out fragmentShaderId, fragmentShaderSource, ShaderType.FragmentShader, logHandle);

            if(!(vertexShaderSucces && fragmentShaderSucces))
            {
                GL.DeleteShader(vertexShaderId);
                GL.DeleteShader(fragmentShaderId);
                return;
            }

            if(CreateProgram(out gl_program, vertexShaderId, fragmentShaderId, logHandle))
            {
                ShaderUniform[] shaderUniforms;
                FindAllActiveUniforms(out shaderUniforms, logHandle);
            }
        }

        private bool CreateShader(out int shaderId, string shaderSource, ShaderType shaderType, LogHandle logHandle)
        {
            int shaderIdTemp = GL.CreateShader(shaderType);

            if (shaderIdTemp == 0)
            {
                logHandle.WriteError("Shader", $"Cannot create {shaderType.ToString()} shader for {parentShader.Path}!");
                shaderId = -1;
                return false;
            }

            GL.ShaderSource(shaderIdTemp, shaderSource.ToString());
            GL.CompileShader(shaderIdTemp);
            int compileStatus;
            GL.GetShader(shaderIdTemp, ShaderParameter.CompileStatus, out compileStatus);

            if (compileStatus == 0)
            {
                logHandle.WriteError("Shader", $"Cannot compile {shaderType.ToString()} shader for {parentShader.Path}!\n{GL.GetShaderInfoLog(shaderIdTemp)}");
                GL.DeleteShader(shaderIdTemp);
                shaderId = -1;
                return false;
            }

            shaderId = shaderIdTemp;
            return true;
        }

        private bool CreateProgram(out int programID, int vertexShaderId, int fragmentShaderId, LogHandle logHandle)
        {
            int tempProgramId = GL.CreateProgram();

            if (tempProgramId == 0)
            {
                logHandle.WriteError("Shader", $"Failed to create a program for {parentShader.Path}");
                programID = -1;
                return false;
            }

            GL.AttachShader(tempProgramId, vertexShaderId);
            GL.AttachShader(tempProgramId, fragmentShaderId);
            GL.LinkProgram(tempProgramId);

            int linkStatus;
            GL.GetProgram(tempProgramId, GetProgramParameterName.LinkStatus, out linkStatus);

            if (linkStatus == 0)
            {
                logHandle.WriteError("Shader", $"Failed to link shader program for {parentShader.Path}!\n{GL.GetProgramInfoLog(tempProgramId)}");
                GL.DeleteShader(vertexShaderId);
                GL.DeleteShader(fragmentShaderId);
                GL.DeleteProgram(tempProgramId);
                programID = -1;
                return false;
            }

            //since shaders are linked we can detach them
            GL.DetachShader(tempProgramId, vertexShaderId);
            GL.DetachShader(tempProgramId, fragmentShaderId);

            GL.ValidateProgram(tempProgramId);
            int validationStatus;
            GL.GetProgram(tempProgramId, GetProgramParameterName.ValidateStatus, out validationStatus);

            if (validationStatus == 0)
            {
                logHandle.WriteError("Shader", $"Failed to validate shader program for {parentShader.Path}!");
                GL.DeleteShader(vertexShaderId);
                GL.DeleteShader(fragmentShaderId);
                GL.DeleteProgram(tempProgramId);

                //set program to invalid id
                programID = -1;
                return false;
            }
            GL.DeleteShader(vertexShaderId);
            GL.DeleteShader(fragmentShaderId);
            programID = tempProgramId;
            return true;
        }

        private void FindAllActiveUniforms(out ShaderUniform[] uniforms, LogHandle logHandle)
        {
            //get the number of shaderuniforms
            int numOfUniforms;
            GL.GetProgram(gl_program, GetProgramParameterName.ActiveUniforms, out numOfUniforms);

            //allocate the array
            uniforms = new ShaderUniform[numOfUniforms];

            for (int i = 0; i < numOfUniforms; i++)
            {
                int length;
                int size;
                ActiveUniformType aut;
                string name;
                GL.GetActiveUniform(gl_program, i, UNIFORM_NAME_BUFF_SIZE, out length, out size, out aut, out name);
                int location = GL.GetUniformLocation(gl_program, name);
                logHandle.WriteDebug("Shader uniform", $"name:{name} index:{i} location:{location}");
                uniforms[i] = new ShaderUniform(location, name, aut);
            }

        }
    }
}
