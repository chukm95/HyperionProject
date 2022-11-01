using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HE.Core.Rendering.Shaders
{
    internal struct ShaderUniform
    {
        public readonly int location;
        public readonly string name;
        public readonly ActiveUniformType activeUniformType;

        public ShaderUniform(int location, string name, ActiveUniformType activeUniformType)
        {
            this.location = location;
            this.name = name;
            this.activeUniformType = activeUniformType;
        }
    }
}
