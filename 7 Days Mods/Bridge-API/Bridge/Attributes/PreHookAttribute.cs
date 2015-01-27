using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bridge.Attributes
{
    /// <summary>
    /// A method-level attribute indicating that the annotated method is to be added as a pre-hook on the method
    /// specified in the attribute.
    /// </summary>
    /// <see cref="PatchAttribute"/>
    /// <see cref="PostHookAttribute"/>
    /// <remarks>
    /// A pre-hook method is one which is called before the main body of the patched method. The code from the patching
    /// method will be inserted before the first instruction of the patched method. It is up to a patcher
    /// implementation to resolve any pre-hook conflicts.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Method)]
    public class PreHookAttribute : PatchAttribute
    {
        public PreHookAttribute(Type type, String method) : base(type, method) {}
    }
}
