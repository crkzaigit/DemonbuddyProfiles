﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace QuestTools.Helpers
{
    /// <summary>
    /// This class well get and set static Trinity Properties and Fields. It does not support instances.
    /// </summary>
    class TrinityApi
    {
        // Trinity_635346394618921335.dll
        private const string AssemblyName = "Trinity";
        // namespace Trinity, public class Trinity
        private const string MainClass = "Trinity.Trinity";

        private static Assembly _mainAssembly;
        private static Dictionary<Tuple<string, Type>, PropertyInfo> _propertyInfoDictionary = new Dictionary<Tuple<string, Type>, PropertyInfo>();
        private static Dictionary<Tuple<string, Type>, FieldInfo> _fieldInfoDictionary = new Dictionary<Tuple<string, Type>, FieldInfo>();
        private static Dictionary<string, Type> _typeDictionary = new Dictionary<string, Type>();

        public static Dictionary<Tuple<string, Type>, PropertyInfo> PropertyInfoDictionary
        {
            get { return _propertyInfoDictionary; }
            set { _propertyInfoDictionary = value; }
        }

        public static Dictionary<Tuple<string, Type>, FieldInfo> FieldInfoDictionary
        {
            get { return _fieldInfoDictionary; }
            set { _fieldInfoDictionary = value; }
        }

        public static Dictionary<string, Type> TypeDictionary
        {
            get { return _typeDictionary; }
            set { _typeDictionary = value; }
        }

        /// <summary>
        /// Sets a property with a given value
        /// </summary>
        /// <param name="typeName">The Type Name</param>
        /// <param name="memberName">The Member Name</param>
        /// <param name="value">The Value</param>
        /// <returns>If the Property was successfully set</returns>
        internal static bool SetProperty(string typeName, string memberName, object value)
        {
            try
            {
                Type targetType;
                if (!SetType(typeName, out targetType))
                {
                    // Type or Assembly not found
                    return false;
                }

                // Make sure TargetType is valid
                if (targetType == null)
                    return false;

                PropertyInfo propertyInfo;
                // Grab cached PropertyInfo object
                if (_propertyInfoDictionary.TryGetValue(new Tuple<string, Type>(memberName, targetType), out propertyInfo))
                {
                    if (propertyInfo != null)
                    {
                        // We have a valid PropertyInfo object, set the value
                        propertyInfo.SetValue(null, value, null);
                        return true;
                    }
                }

                propertyInfo = targetType.GetProperty(memberName, BindingFlags.Static | BindingFlags.Public);
                if (propertyInfo != null)
                {
                    // Upsert propertyInfo object into cache
                    var key = new Tuple<string, Type>(memberName, targetType);
                    if (_propertyInfoDictionary.ContainsKey(key))
                        _propertyInfoDictionary[key] = propertyInfo;
                    else
                        _propertyInfoDictionary.Add(key, propertyInfo);

                    // Set the value
                    propertyInfo.SetValue(null, value, null);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.Log("Error Setting Property {0} from Type {1} - {2}", memberName, typeName, ex.Message);
            }
            return false;
        }

        /// <summary>
        /// Sets a Field with a given value
        /// </summary>
        /// <param name="typeName">The Type name</param>
        /// <param name="memberName">The Member name</param>
        /// <param name="value">The value</param>
        /// <returns>If the Field was successfully set</returns>
        internal static bool SetField(string typeName, string memberName, object value)
        {
            try
            {
                Type targetType;
                if (!SetType(typeName, out targetType))
                {
                    return false;
                }
                if (targetType == null)
                    return false;

                FieldInfo fieldInfo;
                if (_fieldInfoDictionary.TryGetValue(new Tuple<string, Type>(memberName, targetType), out fieldInfo))
                {
                    fieldInfo.SetValue(null, value);
                    return true;
                }

                fieldInfo = targetType.GetField(memberName, BindingFlags.Static | BindingFlags.Public);
                if (fieldInfo != null)
                {
                    var key = new Tuple<string, Type>(memberName, targetType);
                    if (_fieldInfoDictionary.ContainsKey(key))
                        _fieldInfoDictionary[key] = fieldInfo;
                    else
                        _fieldInfoDictionary.Add(key, fieldInfo);

                    _fieldInfoDictionary.Add(new Tuple<string, Type>(memberName, targetType), fieldInfo);

                    fieldInfo.SetValue(null, value);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.Log("Error Setting Field {0} from Type {1} - {2}", memberName, typeName, ex.Message);
            }
            return false;
        }

        /// <summary>
        /// Gets a value from a Field
        /// </summary>
        /// <param name="typeName">The Type Name</param>
        /// <param name="memberName">The Member Name</param>
        /// <param name="value">The Value</param>
        /// <returns>If the field was successfully returned</returns>
        internal static bool GetField(string typeName, string memberName, out object value)
        {
            value = null;
            try
            {
                Type targetType;
                if (!SetType(typeName, out targetType))
                {
                    return false;
                }
                if (targetType == null)
                    return false;

                FieldInfo fieldInfo;
                if (_fieldInfoDictionary.TryGetValue(new Tuple<string, Type>(memberName, targetType), out fieldInfo))
                {
                    value = fieldInfo.GetValue(null);
                    return true;
                }

                fieldInfo = targetType.GetField(memberName, BindingFlags.Static | BindingFlags.Public);
                if (fieldInfo != null)
                {
                    _fieldInfoDictionary.Add(new Tuple<string, Type>(memberName, targetType), fieldInfo);

                    value = fieldInfo.GetValue(null);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.Log("Error reading Property {0} from Type {1} - {2}", memberName, typeName, ex.Message);
                return false;
            }
            return false;
        }

        /// <summary>
        /// Gets a value from a Property
        /// </summary>
        /// <param name="typeName">The Type Name</param>
        /// <param name="memberName">The Member Name</param>
        /// <param name="value">The value to set</param>
        /// <returns>If the Property was successfully returned</returns>
        internal static bool GetProperty(string typeName, string memberName, out object value)
        {
            value = null;
            try
            {
                Type targetType;
                if (!SetType(typeName, out targetType))
                {
                    return false;
                }

                if (targetType == null)
                    return false;

                PropertyInfo propertyInfo;
                if (_propertyInfoDictionary.TryGetValue(new Tuple<string, Type>(memberName, targetType), out propertyInfo))
                {
                    value = propertyInfo.GetValue(null, null);
                    return true;
                }

                propertyInfo = targetType.GetProperty(memberName, BindingFlags.Static | BindingFlags.Public);
                if (propertyInfo != null)
                {
                    _propertyInfoDictionary.Add(new Tuple<string, Type>(memberName, targetType), propertyInfo);
                    propertyInfo.GetValue(null, null);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.Log("Error reading Property {0} from Type {1} - {2}", memberName, typeName, ex.Message);
                return false;
            }
            return false;
        }

        /// <summary>
        /// Caches the given type by name
        /// </summary>
        /// <param name="name">The Type name</param>
        /// <param name="result">The Type</param>
        /// <returns>If the type was found</returns>
        private static bool SetType(string name, out Type result)
        {
            result = null;
            try
            {
                SetAssembly();

                if (_mainAssembly == null)
                {
                    return false;
                }

                if (TypeDictionary.TryGetValue(name, out result) && result != null)
                    return true;

                result = _mainAssembly.GetType(name);

                if (TypeDictionary.ContainsKey(name))
                    TypeDictionary[name] = result;
                else
                    TypeDictionary.Add(name, result);
                
                return true;

            }
            catch (Exception ex)
            {
                Logger.Log("Unable to read type {0} from Assembly: {2}", name, ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Caches the Assembly
        /// </summary>
        private static void SetAssembly()
        {
            try
            {
                if (_mainAssembly != null)
                    return;

                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies().Where(x => x.GetName().Name.StartsWith(AssemblyName)).Where(asm => asm != null))
                {
                    try
                    {
                        asm.GetType(MainClass);
                        _mainAssembly = asm;
                    }
                    catch
                    {
                        // Not the Plugin, probably the routine, keep trying
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log("Unable to set Assembly {2}", ex.Message);
            }
        }
    }
}
