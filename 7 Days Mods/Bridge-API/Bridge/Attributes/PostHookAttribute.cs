using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bridge.Attributes
{
    /// <summary>
    /// A method-level attribute indicating that the annotated method is to be added as a post-hook on the method
    /// specified in the attribute.
    /// </summary>
    /// <see cref="PatchAttribute"/>
    /// <remarks>
    /// A post-hook method is one which is called at the end of the main body of the hooked method. It is
    /// called just before the method would normally return control to the hooked-method caller.
    /// 
    /// Note that the structure of the execution stack could potentially be altered by a post-hook method, particularly
    /// if the method returns a value. Generally methods communicate their return values with the calling method by
    /// placing the return value on the top of the stack. Since a post-hook method is run just before the return of the
    /// hooked method, this return value, or even the structure of the stack itself, could be altered. Beware. This can
    /// cause unintended side-effects if not done very carefully. Additionally, if multiple methods request to be added
    /// as a post-hook for the same method, a patcher could decide to add them in any order or fail due to a hooking
    /// conflict.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Method)]
    public class PostHookAttribute : PatchAttribute
    {
        public PostHookAttribute(Type type, String method) : base(type, method) {}
    }
}
