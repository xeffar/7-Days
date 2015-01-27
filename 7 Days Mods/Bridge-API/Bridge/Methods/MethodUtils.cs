using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace Bridge.Methods
{
    /// <summary>
    /// A utility class containing methods for acting on private methods and fields of classes.
    /// </summary>
    public class MethodUtils
    {
        /// <summary>
        /// Call a method of a base class using an instance of a derived class. This is to allow patching methods to
        /// simulate calling <code>base.SomeFunc()</code>.
        /// </summary>
        /// <remarks>
        /// This method works by generating a new dynamic method within the base class which proxies to the base method
        /// ultimately called. This introduces overhead in a number of ways, including the creation and generation of
        /// a new DynamicMethod and the additional jump necessary to first call the proxy method and then the actual
        /// base method. Additionally all of this is done via reflective code, so is naturally much slower than normal.
        /// 
        /// It is possible to call a private method on a base class, but keep in mind this is not normally possible.
        /// Reconsider using this for that functionality.
        /// </remarks>
        /// <typeparam name="T">The return type of the base method.</typeparam>
        /// <param name="baseType">The type of the base class containing the method to call.</param>
        /// <param name="instance">The instance of the derived class used to call the base method.</param>
        /// <param name="methodName">The name of the method in the base class to call.</param>
        /// <param name="args">The arguments to the base method.</param>
        /// <returns>The result of calling the base method.</returns>
        public static T CallBaseMethod<T>(Type baseType, Object instance, String methodName, params object[] args)
        {
            MethodInfo baseMethod = baseType.GetMethod(
                    methodName,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                    null, // binder
                    args.Select((arg) => (arg.GetType())).ToArray(),
                    null); // parameterModifiers
            return (T) baseMethod.InvokeNotOverride(instance, args);
        }

        /// <summary>
        /// Utility method for calling a private instance method on the specified object.
        /// </summary>
        /// <remarks>
        /// Methods on the instance class are discovered by matching the signature of the method with the types of the
        /// arguments passed in the <code>parameters</code> parameter. If a corresponding method can not be found,
        /// an ArgumentException is thrown.
        /// </remarks>
        /// <typeparam name="T">The return type of the method.</typeparam>
        /// <param name="instance">The instance of the object to call the method on.</param>
        /// <param name="methodName">The string name of the method to call.</param>
        /// <param name="parameters">The parameters to pass to the method.</param>
        /// <returns>The result of calling the specified method.</returns>
        /// <exception cref="ArgumentException">Thrown if an appropriate method is not found on the instance.</exception>
        public static T CallPrivateMethod<T>(Object instance, String methodName, params Object[] parameters)
        {
            Type type = instance.GetType();
            MethodInfo method = type.GetMethod(
                    methodName,
                    BindingFlags.NonPublic | BindingFlags.Instance,
                    null, // bindingAttr
                    Array.ConvertAll<Object, Type>(parameters, (val) => { return val.GetType(); }),
                    null); // modifiers
            if (method == null)
            {
                throw new ArgumentException(
                        "Unable to find private method " + methodName + " for type " + type, "methodName");
            }
            return (T) method.Invoke(instance, parameters);
        }

        /// <summary>
        /// Calls a private static method on a type with the specified parameters.
        /// </summary>
        /// <remarks>
        /// Methods matching the specified method name are discovered by matching the signature of the method with the
        /// types of the arguments passed in the <code>parameters</code> parameter. If a corresponding method cannot
        /// be found, an ArgumentException is thrown.
        /// </remarks>
        /// <typeparam name="T">The return type of the method.</typeparam>
        /// <param name="instance">The type containing the method to call.</param>
        /// <param name="methodName">The string name of the method to call.</param>
        /// <param name="parameters">The parameters to pass to the method.</param>
        /// <returns>The result of calling the specified method.</returns>
        /// <exception cref="ArgumentException">Thrown if an appropriate method is not found in the type.</exception>
        public static T CallPrivateStaticMethod<T>(Type type, string methodName, params Object[] parameters)
        {
            MethodInfo method = type.GetMethod(
                    methodName,
                    BindingFlags.NonPublic | BindingFlags.Static,
                    null, // bindingAttr
                    Array.ConvertAll<Object, Type>(parameters, (val) => { return val.GetType(); }),
                    null); // modifiers
            if (method == null)
            {
                throw new ArgumentException(
                        "Unabled to find private method " + methodName + " in type " + type, "methodName");
            }
            return (T) method.Invoke(null, parameters);
        }

        /// <summary>
        /// Returns the value of a private static field from a type.
        /// </summary>
        /// <typeparam name="T">The type of the field.</typeparam>
        /// <param name="type">The type containing the field to return.</param>
        /// <param name="fieldName">The string name of the field.</param>
        /// <returns>The value of the field.</returns>
        /// <exception cref="ArgumentException">Thrown if the specified field cannot be found in the type.</exception>
        public static T GetPrivateStaticField<T>(Type type, string fieldName) {
            FieldInfo field = type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Static);
            if (field == null)
            {
                throw new ArgumentException("Unable to find field " + fieldName + " in type " + type, "fieldName");
            }
            return (T) field.GetValue(null);
        }

        /// <summary>
        /// Returns the value of a private field from an instance of a type.
        /// </summary>
        /// <typeparam name="T">The type of the field.</typeparam>
        /// <param name="type">The instance object to retrieve the field from.</param>
        /// <param name="fieldName">The string name of the field.</param>
        /// <returns>The value of the field.</returns>
        /// <exception cref="ArgumentException">Thrown if the specified field cannot be found in the type.</exception>
        public static T GetPrivateField<T, U>(U instance, string fieldName)
        {
            Type type = instance.GetType();
            FieldInfo field = type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (field == null)
            {
                throw new ArgumentException("Unable to find field " + fieldName + " in type " + type, "fieldName");
            }
            return (T) field.GetValue(instance);
        }
    }

    /// <summary>
    /// A class containing relevant extension methods for the MethodInfo class.
    /// </summary>
    public static class MethodInfoExtensions
    {
        /// <summary>
        /// An extension method to allow calling a MethodInfo from a base class given a derived class instance.
        /// </summary>
        /// <param name="methodInfo">The MethodInfo object which describes the method to call in the base class.</param>
        /// <param name="targetObject">The target instance on which the base method will be invoked.</param>
        /// <param name="arguments">The arguments to the base method.</param>
        /// <returns>The result of calling the base method.</returns>
        /// <exception cref="ArgumentException">
        /// Thrown if there is an argument count mismatch between supplied arguments and the method to call.
        /// </exception>
        public static object InvokeNotOverride(
                this MethodInfo methodInfo, object targetObject, params object[] arguments)
        {
            var parameters = methodInfo.GetParameters();
            var inputParamCount = arguments == null ? 0 : arguments.Length;

            if (inputParamCount != parameters.Length)
            {
                throw new ArgumentException(String.Format(
                    "Incorrect number of arguments! Expected {0}, got {1}",
                        parameters.Length,
                        inputParamCount));
            }
            Type returnType = null;
            if (methodInfo.ReturnType != typeof(void))
            {
                returnType = methodInfo.ReturnType;
            }

            var type = targetObject.GetType();
            var dynamicMethod = new DynamicMethod("", returnType,
                    new Type[] { type, typeof(Object) }, type);

            var ilGenerator = dynamicMethod.GetILGenerator();
            ilGenerator.Emit(OpCodes.Ldarg_0); // this

            for (var i = 0; i < parameters.Length; i++)
            {
                var parameter = parameters[i];

                ilGenerator.Emit(OpCodes.Ldarg_1); // load array argument

                // get element at index
                ilGenerator.Emit(OpCodes.Ldc_I4_S, i); // specify index
                ilGenerator.Emit(OpCodes.Ldelem_Ref); // get element

                var parameterType = parameter.ParameterType;
                if (parameterType.IsPrimitive)
                {
                    ilGenerator.Emit(OpCodes.Unbox_Any, parameterType);
                }
                else if (parameterType != typeof(object))
                {
                    ilGenerator.Emit(OpCodes.Castclass, parameterType);
                }
            }

            ilGenerator.Emit(OpCodes.Call, methodInfo);
            ilGenerator.Emit(OpCodes.Ret);

            return dynamicMethod.Invoke(null, new object[] { targetObject, arguments });
        }
    }
}
