using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bridge.Attributes
{
    /// <summary>
    /// The base class for all typed attributes indicating a method should be patched in to an assembly.
    /// </summary>
    /// 
    /// <remarks>
    /// A PatchAttribute should never be used directly -- only accessed via one of the subclasses, such
    /// as ReplaceAttribute. PatchAttributes can only be applied to static methods. Additionally, the method signature
    /// of the patching method (the method with the PatchAttribute) will be used to break any method overloading ties.
    /// So, if a method MyPatch requests to patch TypeA.MethodB, and MethodB has three overrides, the signature of
    /// MyPatch is used to determine the proper overloaded form of MethodB. Furthermore, if no MethodB is found which
    /// matches the signature of MyPatch, no patch will be applied, and a runtime exception may occur (patcher
    /// dependent).
    /// </remarks>
    /// 
    /// <example>
    /// This is an example of using the ReplaceAttribute to replace a
    /// TypeToPatch.MethodToPatch(int, RandomType) method with Console.WriteLine. The result of this, once a patcher
    /// has processed the patching code, is calling TypeToPatch.MethodToPatch(int, RandomType) prints "HelloWorld!"
    /// to the console.
    /// <code>
    /// class MyPatchContainer
    /// {
    ///     [Replace(typeof(TypeToPatch), "MethodToPatch")]
    ///     public void PatchMethod(int anArgument, RandomType anotherArgument)
    ///     {
    ///         Console.WriteLine("HelloWorld!");
    ///     }
    /// }
    /// </code>
    /// </example>
    public partial class PatchAttribute : Attribute
    {
        /// <summary>
        /// The Type containing the method to be patched.
        /// </summary>
        public Type type;

        /// <summary>
        /// The name of the method to be patched.
        /// </summary>
        public String method;

        /// <summary>
        /// When patching non-static (instance) methods on a class, this parameter specifies whether the implicit
        /// 'this' argument should be passed to the patching method. In that case the type of the first argument must
        /// be the value of 'type'.
        /// </summary>
        public bool addThisParameter;

        protected PatchAttribute(Type type, String method)
        {
            this.type = type;
            this.method = method;
            addThisParameter = false;
        }
    }
}
