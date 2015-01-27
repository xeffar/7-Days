using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Bridge.Patcher
{
    /// <summary>
    /// An interface for patching assemblies using patches described by the PatchAttribute struct.
    /// </summary>
    interface IPatcher
    {
        /// <summary>
        /// Add an IL patch to the current assembly.
        /// </summary>
        /// <see cref="PatchAttribute"/>
        /// <param name="patch">A descriptor object that contains information about the patch.</param>
        void AddPatch(PatchDescriptor patch);

        /// <summary>
        /// Write the current state of the patched assembly to the specified stream.
        /// </summary>
        /// <param name="outputStream">A stream to output the patched assembly bytes.</param>
        void WritePatchedAssembly(Stream outputStream);
    }
}
