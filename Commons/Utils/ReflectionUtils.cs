using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Klyte.Commons.Utils
{
    public static class ReflectionUtils
    {


        #region Reflection

        #region Extract Properties
        public static void GetPropertyDelegates<CL, PT>(string propertyName, out Action<CL, PT> setter, out Func<CL, PT> getter)
        {
            setter = (Action<CL, PT>) Delegate.CreateDelegate(typeof(Action<CL, PT>), null, typeof(CL).GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).GetSetMethod());
            getter = (Func<CL, PT>) Delegate.CreateDelegate(typeof(Func<CL, PT>), null, typeof(CL).GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).GetGetMethod());
        }
        public static void GetStaticPropertyDelegates<CL, PT>(string propertyName, out Action<PT> setter, out Func<PT> getter)
        {
            setter = (Action<PT>) Delegate.CreateDelegate(typeof(Action<PT>), null, typeof(CL).GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static).GetSetMethod());
            getter = (Func<PT>) Delegate.CreateDelegate(typeof(Func<PT>), null, typeof(CL).GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static).GetGetMethod());
        }

        #endregion
        public static T RunPrivateMethod<T>(object o, string methodName, params object[] paramList)
        {
            if ((methodName ?? "") != string.Empty)
            {
                MethodInfo method = o.GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                if (method != null)
                {
                    return (T) method.Invoke(o, paramList);
                }
            }
            return default;

        }

        public static T RunPrivateStaticMethod<T>(Type t, string methodName, params object[] paramList)
        {
            if ((methodName ?? "") != string.Empty)
            {
                MethodInfo method = t.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                if (method != null)
                {
                    return (T) method.Invoke(null, paramList);
                }
            }
            return default;

        }
        #region Field Access

        public static void GetFieldDelegates<TSource, TValue>(FieldInfo info, out Func<TSource, TValue> getter, out Action<TSource, TValue> setter)
        {
            getter = GetGetFieldDelegate<TSource, TValue>(info);
            setter = GetSetFieldDelegate<TSource, TValue>(info);
        }

        /// <summary>
        /// Gets a strong typed delegate to a generated method that allows you to get the field value, that is represented
        /// by the given <paramref name="fieldInfo"/>. The delegate is instance independend, means that you pass the source 
        /// of the field as a parameter to the method and get back the value of it's field.
        /// </summary>
        /// <typeparam name="TSource">The reflecting type. This can be an interface that is implemented by the field's declaring type
        /// or an derrived type of the field's declaring type.</typeparam>
        /// <typeparam name="TValue">The type of the field value.</typeparam>
        /// <param name="fieldInfo">Provides the metadata of the field.</param>
        /// <returns>A strong typed delegeate that can be cached to get the field's value with high performance.</returns>
        public static Func<TSource, TValue> GetGetFieldDelegate<TSource, TValue>(FieldInfo fieldInfo)
        {
            if (fieldInfo == null)
            {
                throw new ArgumentNullException("fieldInfo");
            }

            Type fieldDeclaringType = fieldInfo.DeclaringType;

            ParameterExpression sourceParameter =
             Expression.Parameter(typeof(TSource), "source");


            Expression sourceExpression = GetCastOrConvertExpression(
                sourceParameter, fieldDeclaringType);

            MemberExpression fieldExpression = Expression.Field(sourceExpression, fieldInfo);

            Expression resultExpression = GetCastOrConvertExpression(
            fieldExpression, typeof(TValue));

            LambdaExpression lambda =
                Expression.Lambda(typeof(Func<TSource, TValue>), resultExpression, sourceParameter);

            var compiled = (Func<TSource, TValue>) lambda.Compile();
            return compiled;
        }

        /// <summary>
        /// Gets a strong typed delegate to a generated method that allows you to get the field value, that is represented
        /// by the given <paramref name="fieldName"/>. The delegate is instance independend, means that you pass the source 
        /// of the field as a parameter to the method and get back the value of it's field.
        /// </summary>
        /// <typeparam name="TSource">The reflecting type. This can be an interface that is implemented by the field's declaring type
        /// or an derrived type of the field's declaring type.</typeparam>
        /// <typeparam name="TValue">The type of the field value.</typeparam>
        /// <param name="fieldName">The name of the field.</param>
        /// <param name="fieldType">The type of the field.</param>
        /// <param name="fieldDeclaringType">The type that declares the field.</param>
        /// <returns>A strong typed delegeate that can be cached to get the field's value with high performance.</returns>
        public static Func<TSource, TValue> GetGetFieldDelegate<TSource, TValue>(string fieldName, Type fieldDeclaringType)
        {
            if (fieldName == null)
            {
                throw new ArgumentNullException("fieldName");
            }

            if (fieldDeclaringType == null)
            {
                throw new ArgumentNullException("fieldDeclaringType");
            }

            FieldInfo fieldInfo = fieldDeclaringType.GetField(fieldName,
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);

            return GetGetFieldDelegate<TSource, TValue>(fieldInfo);
        }

        /// <summary>
        /// Gets a strong typed delegate to a generated method that allows you to set the field value, that is represented
        /// by the given <paramref name="fieldInfo"/>. The delegate is instance independend, means that you pass the source 
        /// of the field as a parameter to the generated method and get back the value of it's field.
        /// </summary>
        /// <typeparam name="TSource">The reflecting type. This can be an interface that is implemented by the field's declaring type
        /// or an derrived type of the field's declaring type.</typeparam>
        /// <typeparam name="TValue">The type of the field value.</typeparam>
        /// <param name="fieldInfo">Provides the metadata of the field.</param>
        /// <returns>A strong typed delegeate that can be cached to set the field's value with high performance.</returns>
        public static Action<TSource, TValue> GetSetFieldDelegate<TSource, TValue>(FieldInfo fieldInfo)
        {
            if (fieldInfo == null)
            {
                throw new ArgumentNullException("fieldInfo");
            }

            Type fieldDeclaringType = fieldInfo.DeclaringType;
            //Type fieldType = fieldInfo.FieldType;
            //String fieldName = fieldInfo.Name;

            // Define the parameters of the lambda expression: (source,value) =>
            ParameterExpression sourceParameter = Expression.Parameter(typeof(TSource), "source");
            ParameterExpression valueParameter = Expression.Parameter(typeof(TValue), "value");



            // Add cast or convert expression if necessary. (e.g. when fieldDeclaringType is not assignable from typeof(TSource)
            Expression sourceExpression = GetCastOrConvertExpression(sourceParameter, fieldDeclaringType);

            // Get the field access expression.
            Expression fieldExpression = Expression.Field(sourceExpression, fieldInfo);

            // Add cast or convert expression if necessary.
            Expression valueExpression = GetCastOrConvertExpression(valueParameter, fieldExpression.Type);

            // Get the generic method that assigns the field value.
            MethodInfo genericSetFieldMethodInfo = m_setFieldMethod.MakeGenericMethod(fieldExpression.Type);

            // get the set field expression 
            // e.g. source.SetField(ref (arg as MyClass).integerProperty, Convert(value)
            MethodCallExpression setFieldMethodCallExpression = Expression.Call(
                null, genericSetFieldMethodInfo, fieldExpression, valueExpression);

            // Create the final lambda expression
            // e.g. (source,value) => SetField(ref (arg as MyClass).integerProperty, Convert(value))
            LambdaExpression lambda = Expression.Lambda(typeof(Action<TSource, TValue>),
                                                        setFieldMethodCallExpression, sourceParameter, valueParameter);

            var result = (Action<TSource, TValue>) lambda.Compile();
            return result;
        }

        /// <summary>
        /// Gets a strong typed delegate to a generated method that allows you to set the field value, that is represented
        /// by the given <paramref name="fieldName"/>. The delegate is instance independend, means that you pass the source 
        /// of the field as a parameter to the generated method and get back the value of it's field.
        /// </summary>
        /// <typeparam name="TSource">The reflecting type. This can be an interface that is implemented by the field's declaring type
        /// or an derrived type of the field's declaring type.</typeparam>
        /// <typeparam name="TValue">The type of the field value.</typeparam>
        /// <param name="fieldName">The name of the field.</param>
        /// <param name="fieldType">The type of the field.</param>
        /// <param name="fieldDeclaringType">The type that declares the field.</param>
        /// <returns>A strong typed delegeate that can be cached to set the field's value with high performance.</returns>
        public static Action<TSource, TValue> GetSetFieldDelegate<TSource, TValue>(string fieldName, Type fieldType, Type fieldDeclaringType)
        {
            if (fieldName == null)
            {
                throw new ArgumentNullException("fieldName");
            }

            if (fieldType == null)
            {
                throw new ArgumentNullException("fieldType");
            }

            if (fieldDeclaringType == null)
            {
                throw new ArgumentNullException("fieldDeclaringType");
            }

            // Define the parameters of the lambda expression: (source,value) =>
            ParameterExpression sourceParameter = Expression.Parameter(typeof(TSource), "source");
            ParameterExpression valueParameter = Expression.Parameter(typeof(TValue), "value");


            // Add cast or convert expression if necessary. (e.g. when fieldDeclaringType is not assignable from typeof(TSource)
            Expression sourceExpression = GetCastOrConvertExpression(sourceParameter, fieldDeclaringType);
            Expression valueExpression = GetCastOrConvertExpression(valueParameter, fieldType);

            // Get the field access expression.
            MemberExpression fieldExpression = Expression.Field(sourceExpression, fieldName);

            // Get the generic method that assigns the field value.
            MethodInfo genericSetFieldMethodInfo = m_setFieldMethod.MakeGenericMethod(fieldType);

            // get the set field expression 
            // e.g. source.SetField(ref (arg as MyClass).integerProperty, Convert(value)
            MethodCallExpression setFieldMethodCallExpression = Expression.Call(
                null, genericSetFieldMethodInfo, fieldExpression, valueExpression);

            // Create the final lambda expression
            // e.g. (source,value) => SetField(ref (arg as MyClass).integerProperty, Convert(value))
            LambdaExpression lambda = Expression.Lambda(typeof(Action<TSource, TValue>),
                                                        setFieldMethodCallExpression, sourceParameter, valueParameter);

            var result = (Action<TSource, TValue>) lambda.Compile();
            return result;
        }

        /// <summary>
        /// Gets an expression that can be assigned to the given target type. 
        /// Creates a new expression when a cast or conversion is required, 
        /// or returns the given <paramref name="expression"/> if no cast or conversion is required.
        /// </summary>
        /// <param name="expression">The expression which resulting value should be passed to a 
        /// parameter with a different type.</param>
        /// <param name="targetType">The target parameter type.</param>
        /// <returns>The <paramref name="expression"/> if no cast or conversion is required, 
        /// otherwise a new expression instance that wraps the the given <paramref name="expression"/> 
        /// inside the required cast or conversion.</returns>
        private static Expression GetCastOrConvertExpression(Expression expression, Type targetType)
        {
            Expression result;
            Type expressionType = expression.Type;

            // Check if a cast or conversion is required.
            if (targetType.IsAssignableFrom(expressionType))
            {
                result = expression;
            }
            else
            {
                // Check if we can use the as operator for casting or if we must use the convert method
                if (targetType.IsValueType && !IsNullableType(targetType))
                {
                    result = Expression.Convert(expression, targetType);
                }
                else
                {
                    result = Expression.TypeAs(expression, targetType);
                }
            }

            return result;
        }

        internal static T GetPrivateField<T>(object prefabAI, string v) => (T) prefabAI.GetType().GetField(v).GetValue(prefabAI);


        #region Called by reflection - Don't delete.

        /// <summary>
        /// Stores the method info for the method that performs the assignment of the field value.
        /// Note: There is no assign expression in .NET 3.0/3.5. With .NET 4.0 this method becomes obsolete.
        /// </summary>
        private static readonly MethodInfo m_setFieldMethod = typeof(ReflectionUtils).GetMethod("SetField", BindingFlags.NonPublic | BindingFlags.Static);

        /// <summary>
        /// A strong type method that assigns the given value to the field that is represented by the given field reference.
        /// Note: .NET 4.0 provides an assignment expression. This method is just required for .NET 3.0/3.5.
        /// </summary>
        /// <typeparam name="TValue">The type of the field.</typeparam>
        /// <param name="field">A reference to the field.</param>
        /// <param name="newValue">The new value that should be assigned to the field.</param>
        public static void SetField<TValue>(ref TValue field, TValue newValue) => field = newValue;

        #endregion Called by reflection - Don't delete.

        #endregion Field Access
        #region Extract Method
        public static Delegate GetMethodDelegate(string propertyName, Type targetType, Type actionType) => GetMethodDelegate(targetType.GetMethod(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static), actionType);

        public static Delegate GetMethodDelegate(MethodInfo method, Type actionType) => Delegate.CreateDelegate(actionType, null, method);

        #endregion

        public static FieldInfo GetEventField(Type type, string eventName)
        {
            FieldInfo field = null;
            while (type != null)
            {
                /* Find events defined as field */
                field = type.GetField(eventName, BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic);
                if (field != null && (field.FieldType == typeof(MulticastDelegate) || field.FieldType.IsSubclassOf(typeof(MulticastDelegate))))
                {
                    break;
                }

                /* Find events defined as property { add; remove; } */
                field = type.GetField("EVENT_" + eventName.ToUpper(), BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic);
                if (field != null)
                {
                    break;
                }

                type = type.BaseType;
            }
            return field;
        }

        /// <summary>
        /// Determines whether the given type is a nullable type.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns>true if the given type is a nullable type, otherwise false.</returns>
        public static bool IsNullableType(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            bool result = false;
            if (type.IsGenericType && type.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                result = true;
            }

            return result;
        }

        public static bool HasField(object o, string fieldName)
        {
            FieldInfo[] fields = o.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (FieldInfo f in fields)
            {
                if (f.Name == fieldName)
                {
                    return true;
                }
            }
            return false;
        }
        public static List<Type> GetSubtypesRecursive(Type typeTarg, Type refType)
        {
            if (CommonProperties.DebugMode)
            {
                LogUtils.DoLog($"typeTarg = {typeTarg} | IsGenType={typeTarg.IsGenericType} ");
            }

            IEnumerable<Type> classes = (from t in AppDomain.CurrentDomain.GetAssemblies().Where(x => refType == null || x == refType.Assembly)?.SelectMany(x =>
            {
                try
                { return x?.GetTypes(); }
                catch { return new Type[0]; }
            })
                                         let y = t.BaseType
                                         where t.IsClass && y != null && y.IsGenericType && y.GetGenericTypeDefinition() == typeTarg
                                         select t);
            var result = new List<Type>();
            if (CommonProperties.DebugMode)
            {
                LogUtils.DoLog($"classes:\r\n\t {string.Join("\r\n\t", classes.Select(x => x.ToString()).ToArray())} ");
            }

            foreach (Type t in classes)
            {
                if (!t.IsSealed)
                {
                    result.AddRange(GetSubtypesRecursive(t, t));
                }
                if (!t.IsAbstract)
                {
                    result.Add(t);
                }
            }
            return result.Distinct().ToList();
        }

        public static List<Type> GetInterfaceImplementations(Type interfaceType, Type refType)
        {
            if (CommonProperties.DebugMode)
            {
                LogUtils.DoLog($"interfaceType = {interfaceType}");
            }

            IEnumerable<Type> classes = (from t in AppDomain.CurrentDomain.GetAssemblies().Where(x => refType == null || x == refType.Assembly)?.SelectMany(x =>
            {
                try
                { return x?.GetTypes(); }
                catch { return new Type[0]; }
            })
                                         let y = t.GetInterfaces()
                                         where t.IsClass && y.Contains(interfaceType) && !t.IsAbstract
                                         select t);

            var result = new List<Type>();
            if (CommonProperties.DebugMode)
            {
                LogUtils.DoLog($"classes:\r\n\t {string.Join("\r\n\t", classes.Select(x => x.ToString()).ToArray())} ");
            }

            return classes.ToList();
        }
        public static Type GetImplementationForGenericType(Type typeOr, params Type[] typeArgs)
        {
            Type typeTarg = typeOr.MakeGenericType(typeArgs);

            IEnumerable<Type> instances = (from t in Assembly.GetAssembly(typeOr).GetTypes()
                                           where t.IsClass && !t.IsAbstract && typeTarg.IsAssignableFrom(t) && !t.IsGenericType
                                           select t);
            if (instances.Count() != 1)
            {
                throw new Exception($"Defininções inválidas para [{ string.Join(", ", typeArgs.Select(x => x.ToString()).ToArray()) }] no tipo genérico {typeOr}");
            }

            Type targetType = instances.First();
            return targetType;
        }
        #endregion
    }
}
