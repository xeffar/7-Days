using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

using Mono.Cecil;
using Mono.Cecil.Cil;

using Bridge.Attributes;
using Bridge.Methods;

namespace Bridge.Patcher
{
    class MainClass
    {
        private static readonly String DEFAULT_MODS_DIR = "Mods";
        private static readonly String DEFAULT_ASSEMBLY_DIR = "Managed";
        private static readonly BindingFlags ALL_INCLUSIVE_BINDING_FLAGS =
            BindingFlags.Instance |
            BindingFlags.Static |
            BindingFlags.Public |
            BindingFlags.NonPublic |
            BindingFlags.DeclaredOnly;

        static void Main(string[] args)
        {
            IPatcher patcher;
            // TODO: Check which mods have already been installed, only install new ones
            // TODO: If this is the first time patching, backup the assembly in a "pristine" folder.
            // TODO: Corollary to first: maintain list of mods and patches, add this to the assembly for later reading.
            
            String targetAssembly = args.Length > 0 ? args[0] : Path.Combine(DEFAULT_ASSEMBLY_DIR, "Assembly-CSharp.dll");
            String pristineLocation = targetAssembly.Replace(".dll", "-pristine.dll");
            String outputLocation = targetAssembly;
            if (File.Exists(pristineLocation))
            {
                Console.WriteLine("Found pristine DLL at {0}, using this as patching base.", pristineLocation);
                targetAssembly = pristineLocation;
            }
            else
            {
                Console.WriteLine("Making backup of current {0} at {1}....", targetAssembly, pristineLocation);
                try
                {
                    File.Copy(targetAssembly, pristineLocation);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Failed to make backup of {0} at {1}!", targetAssembly, pristineLocation);
                    Console.WriteLine(e);
                    return;
                }
            }
            try
            {
                patcher = CecilPatcher.Create(targetAssembly);
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to read input assembly {0}: {1}", targetAssembly, e);
                return;
            }
            IEnumerable<string> modDlls =
                    Directory.EnumerateFileSystemEntries(DEFAULT_MODS_DIR, "*.dll", SearchOption.AllDirectories);
            foreach (String modDll in modDlls)
            {
                Assembly assembly;
                try
                {
                    // TODO: use reflection only context to avoid overhead of loading assembly transitive deps.
                    // Requires converting all GetCustomAttribute to CustomAttributeData.GetCustomAttributes.
                    assembly = Assembly.LoadFrom(modDll);
                    assembly.GetTypes();
                }
                catch (Exception exception)
                {
                    Console.WriteLine("Exception while attempting to load assembly [{0}]", modDll);
                    Console.WriteLine(exception);
                    continue;
                }
                ModAttribute modAttr = Attribute.GetCustomAttribute(assembly, typeof(ModAttribute)) as ModAttribute;
                if (modAttr == null)
                {
                    Console.WriteLine("Assembly {0} had no Mod attribute", assembly);
                    return;
                }
                Console.WriteLine("Processing data for mod {0}:", modAttr.ToString());
                ICollection<PatchDescriptor> patchesForMod = ProcessAssembly(assembly);
                // TODO: Verify PatchDescriptors locally (make sure method signatures match)
                // and globally (no conflicting patches).
                // TODO: Populate dictionary of <String, Patches> or make a list of "ModDescriptor" objects.
                foreach (var patch in patchesForMod)
                {
                    Console.WriteLine(
                            "Adding patch for method {0} in type {1}.",
                            patch.patchAttribute.method,
                            patch.patchAttribute.type);
                    try
                    {
                        patcher.AddPatch(patch);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Failed to add patch! Exception: {0}", e);
                    }
                }
            }
            using (var outputStream = new FileStream(outputLocation, FileMode.OpenOrCreate, FileAccess.Write))
            {
                patcher.WritePatchedAssembly(outputStream);
            }
            
        }

        static ICollection<PatchDescriptor> ProcessAssembly(Assembly assembly)
        {
            var descriptors = new List<PatchDescriptor>();
            foreach (Type type in assembly.GetTypes())
            {
                ProcessType(type, descriptors);
            }
            return descriptors;
        }

        static void ProcessModule(Module module, ICollection<PatchDescriptor> descriptors)
        {
            var allTypes = module.GetTypes();
            foreach (var type in allTypes) { ProcessType(type, descriptors); } 
        }

        static void ProcessType(Type type, ICollection<PatchDescriptor> descriptors)
        {
            var allMethods = type.GetMethods(ALL_INCLUSIVE_BINDING_FLAGS);
            foreach (var method in allMethods)
            {
                PatchAttribute attr = Attribute.GetCustomAttribute(method, typeof(PatchAttribute)) as PatchAttribute;
                if (attr != null)
                {
                    descriptors.Add(new PatchDescriptor(method, attr));
                }
            }
        }
    }
}
