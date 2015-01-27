using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

using Bridge.Attributes;

namespace Bridge.Patcher
{
    struct PatchDescriptor
    {
        public MethodInfo patchMethod;
        public PatchAttribute patchAttribute;
        public PatchType patchType;

        public PatchDescriptor(MethodInfo method, PatchAttribute attr)
        {
            patchMethod = method;
            patchAttribute = attr;
            if (patchAttribute is ReplaceAttribute)
            {
                patchType = PatchType.Replace;
            }
            else if (patchAttribute is PreHookAttribute)
            {
                patchType = PatchType.PreHook;
            }
            else if (patchAttribute is PostHookAttribute)
            {
                patchType = PatchType.PostHook;
            }
            else
            {
                throw new ArgumentException("Unknown PatchAttribute! " + attr, "attr");
            }
        }
    }
}
