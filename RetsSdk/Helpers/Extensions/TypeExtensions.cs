using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace CrestApps.RetsSdk.Helpers.Extensions
{
    public static class TypeExtensions
    {

        public static object GetSafeObject(this Type type, string value)
        {
            if (Nullable.GetUnderlyingType(type) != null && string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            Type trueType = Nullable.GetUnderlyingType(type) ?? type;


            if (trueType == typeof(string))
            {
                return value;
            }

            if (trueType.IsEnum)
            {
                return Enum.Parse(trueType, value);
            }

            if (trueType == typeof(bool))
            {
                if (bool.TryParse(value, out bool isValid))
                {
                    return isValid;
                }

                return false;
            }

            if (string.IsNullOrWhiteSpace(value) && type.IsNumeric())
            {
                value = "0";
            }

            TypeConverter tc = TypeDescriptor.GetConverter(type);

            return tc.ConvertFromString(value);
        }



        // Integral = sbyte, byte, short, ushort, int, unint, long, ulong
        private static HashSet<Type> IntegralNumericTypes = new HashSet<Type>
        {
            typeof(sbyte), typeof(byte), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong)
        };

        // Fractional = float, double, decimal
        private static HashSet<Type> FractionalNumericTypes = new HashSet<Type>
        {
            typeof(float), typeof(double), typeof(decimal)
        };

        /// <summary>
        /// Finds the BaseType
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IEnumerable<Type> BaseTypes(this Type type)
        {
            Type baseType = type;
            while (true)
            {
                baseType = baseType.BaseType;

                if (baseType == null)
                {
                    break;
                }

                yield return baseType;
            }
        }

        /// <summary>
        /// Find any base type that matches the gives type
        /// </summary>
        /// <param name="type"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static bool AnyBaseType(this Type type, Func<Type, bool> predicate)
        {
            return type.BaseTypes()
                       .Any(predicate);
        }


        public static Type FirstParticularType(this Type type, Type generic)
        {
            return type.BaseTypes()
                       .FirstOrDefault(x => generic.IsAssignableFrom(x));
        }

        /// <summary>
        /// Finds a particular generic type
        /// </summary>
        /// <param name="type"></param>
        /// <param name="generic"></param>
        /// <returns></returns>
        public static bool IsParticularGeneric(this Type type, Type generic)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == generic;
        }

        /// <summary>
        /// Extension method to determine if a type if numeric.
        /// </summary>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <returns>
        /// True if the type is numeric, otherwise false.
        /// </returns>
        public static bool IsNumeric(this Type type)
        {
            Type t = Nullable.GetUnderlyingType(type) ?? type;

            return IntegralNumericTypes.Contains(t) || FractionalNumericTypes.Contains(t);
        }

        /// <summary>
        /// Extension method to determine if a type if integral numeric.
        /// </summary>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <returns>
        /// True if the type is integral numeric, otherwise false.
        /// </returns>
        public static bool IsIntegralNumeric(this Type type)
        {
            Type t = Nullable.GetUnderlyingType(type) ?? type;

            return IntegralNumericTypes.Contains(t);
        }


        /// <summary>
        /// Extension method to determine if a type if fractional numeric.
        /// </summary>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <returns>
        /// True if the type is fractional numeric, otherwise false.
        /// </returns>
        public static bool IsFractionallNumeric(this Type type)
        {
            Type t = Nullable.GetUnderlyingType(type) ?? type;

            return FractionalNumericTypes.Contains(t);
        }

        public static bool IsDateTime(this Type type)
        {
            Type t = Nullable.GetUnderlyingType(type) ?? type;

            return t == typeof(DateTime);
        }


        public static bool IsBoolean(this Type type)
        {
            Type t = Nullable.GetUnderlyingType(type) ?? type;

            return t == typeof(bool);
        }


        public static Type ExtractGenericInterface(this Type queryType, Type interfaceType)
        {
            Func<Type, bool> matchesInterface = t => t.IsGenericType && t.GetGenericTypeDefinition() == interfaceType;

            return (matchesInterface(queryType)) ? queryType : queryType.GetInterfaces().FirstOrDefault(matchesInterface);
        }

        public static Type[] GetTypeArgumentsIfMatch(this Type closedType, Type matchingOpenType)
        {
            if (!closedType.IsGenericType)
            {
                return null;
            }

            Type openType = closedType.GetGenericTypeDefinition();
            return (matchingOpenType == openType) ? closedType.GetGenericArguments() : null;
        }

        public static bool IsCompatibleObject(this Type type, object value)
        {
            return ((value == null && TypeAllowsNullValue(type)) || type.IsInstanceOfType(value));
        }

        public static bool IsNullableValueType(this Type type)
        {
            return Nullable.GetUnderlyingType(type) != null;
        }

        public static bool TypeAllowsNullValue(this Type type)
        {
            return (!type.IsValueType || IsNullableValueType(type));
        }

        public static bool IsTrueGenericType(this Type type)
        {
            return type.IsArray ||
                (type.IsGenericType && type.GetGenericTypeDefinition().Equals(typeof(IEnumerable<>)));
        }

    }
}
