using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bridge.Attributes
{
    /// <summary>
    /// A method-level attribute indicating that the annotated method is to be used as a replacement for the method
    /// specified in the attribute.
    /// </summary>
    /// <see cref="PatchAttribute"/>
    /// <remarks>
    /// Replacement methods are inserted in place of the method body of the patched method. Arguments to the
    /// replacement method must match exactly the arguments of the method being replaced. Note that replacement methods
    /// have a marginally higher cost than the original method, since the original method acts solely as a proxy to the
    /// replacement method post patching. In other words, a method which is patched with a replacement will look like
    /// the following after patching:
    /// <code>
    /// public void PatchedMethod(String someArg, int anotherArg)
    /// {
    ///     ReplacementMethod(someArg, anotherArg);
    /// }
    /// </code>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Method)]
    public class ReplaceAttribute : PatchAttribute
    {
        public ReplaceAttribute(Type type, String method) : base(type, method) {}
    }
}
