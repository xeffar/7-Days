using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Bridge.Patcher
{
    /// <summary>
    /// Class used for patching assemblies by replacing and hooking methods.
    /// </summary>
    class CecilPatcher : IPatcher
    {
        private AssemblyDefinition assembly;

        /// <summary>
        /// Creates a new patcher for the assembly at the specified path.
        /// </summary>
        /// <param name="assemblyPath">The path to the assembly to load for patching.</param>
        /// <returns>A new CecilPatcher which will patch the specified assembly.</returns>
        /// <exception cref="System.IO.FileNotFoundException">Thrown if the specified path does not exist</exception>
        public static CecilPatcher Create(String assemblyPath)
        {
            AssemblyDefinition def = AssemblyDefinition.ReadAssembly(assemblyPath);
            return new CecilPatcher(def);
        }

        private CecilPatcher(AssemblyDefinition assembly)
        {
            this.assembly = assembly;
        }

        public void AddPatch(PatchDescriptor patch)
        {
            Type patchedType = patch.patchAttribute.type;
            String patchedMethod = patch.patchAttribute.method;
            bool addThis = patch.patchAttribute.addThisParameter;
            switch (patch.patchType)
            {
                case PatchType.PreHook:
                    AddPreHook(patchedType, patchedMethod, patch.patchMethod, addThis);
                    break;

                case PatchType.PostHook:
                    AddPostHook(patchedType, patchedMethod, patch.patchMethod, addThis);
                    break;

                case PatchType.Replace:
                    ReplaceMethod(patchedType, patchedMethod, patch.patchMethod, addThis);
                    break;

                default:
                    throw new ArgumentException("Unexpected PatchType " + patch.patchType, "patch");
            }
        }

        public void WritePatchedAssembly(Stream outputStream)
        {
            assembly.Write(outputStream);
        }

        /// <summary>
        /// Add a pre-hook to the specified method. The methodToCall will always be called before the methodToHook.
        /// </summary>
        /// <see cref="ModClient.Attributes.PreHookAttribute"/>
        /// <param name="type">The Type containing the method to be hooked.</param>
        /// <param name="methodToHook">The string name of the method to hook.</param>
        /// <param name="methodToCall">The method which will be added as a pre-hook.</param>
        /// <param name="addThisRef">
        /// Whether the method to be added as a pre-hook expects the first argument to be the instance object being
        /// called. This argument is ignored if the method being patched is static.
        /// </param>
        public void AddPreHook(Type type, String methodToHook, MethodInfo methodToCall, bool addThisRef)
        {
            var methodDef = GetMatchingMethod(type, methodToHook, methodToCall, addThisRef);
            var ilProcessor = methodDef.Body.GetILProcessor();
            int argOffset = methodDef.IsStatic || !addThisRef ? 0 : 1;
            int argCount = methodToCall.GetParameters().Length;
            Instruction firstInstruction = methodDef.Body.Instructions[0];
            for (int arg = 0; arg < argCount; arg++)
            {
                ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Ldarg, arg + argOffset));
            }
            ilProcessor.InsertBefore(
                    firstInstruction, ilProcessor.Create(OpCodes.Call, methodDef.Module.Import(methodToCall)));
        }

        public void AddPostHook(Type type, String methodToHook, MethodInfo methodToCall, bool addThisRef)
        {
            var methodDef = GetMatchingMethod(type, methodToHook, methodToCall, addThisRef);
            var ilProcessor = methodDef.Body.GetILProcessor();
            int argOffset = methodDef.IsStatic || !addThisRef ? 0 : 1;
            int argCount = methodToCall.GetParameters().Length;

            ilProcessor.Remove(methodDef.Body.Instructions.Last());
            for (int arg = 0; arg < argCount; arg++)
            {
                ilProcessor.Emit(OpCodes.Ldarg, arg + argOffset);
            }
            ilProcessor.Emit(OpCodes.Call, methodDef.Module.Import(methodToCall));
            ilProcessor.Emit(OpCodes.Ret);
        }

        public void ReplaceMethod(Type type, String methodToReplace, MethodInfo replacement, bool addThisRef)
        {
            var methodDef = GetMatchingMethod(type, methodToReplace, replacement, addThisRef);
            var ilProcessor = methodDef.Body.GetILProcessor();
            methodDef.Body.Instructions.Clear();
            int argOffset = methodDef.IsStatic || !addThisRef ? 0 : 1;
            int argCount = replacement.GetParameters().Length;

            for (int arg = 0; arg < argCount; arg++)
            {
                ilProcessor.Emit(OpCodes.Ldarg, arg + argOffset);
            }
            ilProcessor.Emit(OpCodes.Call, methodDef.Module.Import(replacement));
            ilProcessor.Emit(OpCodes.Ret);
        }

        private MethodDefinition GetMatchingMethod(Type type, String methodToHook, MethodInfo methodToCall, bool addThisRef)
        {
            ModuleDefinition module = assembly.Modules.First((mod) => (mod.Name == type.Module.Name));
            var typeDef = module.Types.First((typeDefinition) => (typeDefinition.Name == type.Name));
            var methodDef = typeDef.Methods.First(MethodMatcher(methodToCall, addThisRef));
            return methodDef;
        }

        private static bool SignaturesMatch(MethodDefinition methodDef, MethodInfo methodInfo, bool includeThis)
        {
            if (!methodDef.HasParameters)
            {
                return methodInfo.GetParameters().Length == 0;
            }
            var defParams = methodDef.Parameters;
            var infoParams = methodInfo.GetParameters();
            int currentParam = methodDef.IsStatic || !includeThis ? 0 : 1;
            foreach (ParameterDefinition defParam in defParams)
            {
                if (defParam.ParameterType.FullName != infoParams[currentParam].Name)
                {
                    return false;
                }
                currentParam++;
            }
            return true;
        }
        private static Func<MethodDefinition, bool> MethodMatcher(MethodInfo method, bool includeThis)
        {
            return (methodDef) => (methodDef.Name.Equals(method.Name) && SignaturesMatch(methodDef, method, includeThis));
        }
    }
}
