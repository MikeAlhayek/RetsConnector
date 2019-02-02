using RetsSdk.Contracts;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace RetsSdk.Models
{
    public abstract class RetsCollection<T> : IMetadataCollection, IMetadataCollectionLoad, IRetsCollectionXElementLoader, IMetadataCollection<T> where T : class, new()
    {
        public string Version { get; set; }
        public DateTime Date { get; set; }

        private List<T> Items = new List<T>();
        private Type _Type;

        public void Add(T item)
        {
            if (item == null)
            {
                return;
            }

            Items.Add(item);
        }

        public IEnumerable<T> Get()
        {
            return Items;
        }

        public Type GetGenericType()
        {
            if (_Type == null)
            {
                _Type = typeof(T);
            }

            return _Type;
        }

        public void Remove(T item)
        {
            if (item == null)
            {
                return;
            }

            Items.Remove(item);
        }

        public void Load(Type type, XElement xElement)
        {
            if (type == null)
            {
                throw new NullReferenceException("{type} cannot be null");
            }

            if (xElement == null)
            {
                throw new NullReferenceException("{xElement} cannot be null");
            }

            //var collection = Activator.CreateInstance(type);

            // First, we check all attributes on the XElement
            // for any attribute that match the element, we would set the value accordingly.
            foreach (PropertyInfo property in type.GetProperties())
            {
                var attribute = xElement.Attribute(property.Name);

                if (attribute == null)
                {
                    continue;
                }

                // At this point we found attribute that match the property name
                object safeValue = GetSafeObject(property.PropertyType, attribute.Value);

                property.SetValue(this, safeValue, null);
            }

            // Second, foreach child in the XElement's children, we need to cast it into the generic model then add it to the collection
            foreach (XElement child in xElement.Elements())
            {
                Add(child);
            }
        }


        private void Add(XElement element)
        {
            T model = Cast(element);

            Add(model);
        }

        protected object GetSafeObject(Type type, string value)
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

            TypeConverter tc = TypeDescriptor.GetConverter(type);


            return tc.ConvertFromString(value);
        }

        protected T Cast(XElement element)
        {
            var entity = new T();

            // First, foreach child on the given XElement object, find a propery with the same name as the child's localname and set its value accordingly
            foreach (XElement child in element.Elements())
            {
                PropertyInfo property = GetGenericType().GetProperty(child.Name.LocalName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                if (property == null)
                {
                    continue;
                }

                object safeValue = GetSafeObject(property.PropertyType, child.Value);

                property.SetValue(entity, safeValue, null);
            }

            // Second, foreach property of the generic entity that implements IMetadataCollection<ANYTHING>
            // find the sub Element and call Load() method using different generics
            var subCollections = GetGenericType().GetProperties().Where(x => typeof(IRetsCollectionXElementLoader).IsAssignableFrom(x.PropertyType)).ToList();

            foreach (PropertyInfo subCollection in subCollections)
            {
                DescriptionAttribute attribute = subCollection.PropertyType.GetCustomAttributes(typeof(DescriptionAttribute), false)
                                                              .Select(x => x as DescriptionAttribute)
                                                              .FirstOrDefault();
                if (attribute == null)
                {
                    continue;
                }

                XElement metaDataNode = element.Descendants(attribute.Description).FirstOrDefault();

                if (metaDataNode == null)
                {
                    continue;
                }

                IRetsCollectionXElementLoader newCollection = (IRetsCollectionXElementLoader)Activator.CreateInstance(subCollection.PropertyType);
                newCollection.Load(metaDataNode);

                //MethodInfo loadMethod = subCollection.PropertyType.GetMethod("Load", new Type[] { typeof(XElement) });
                //loadMethod.Invoke(newCollection, new object[] { metaDataNode });

                subCollection.SetValue(entity, newCollection, null);

                //loadMethod.Invoke(subCollection.GetValue(subCollection, null), new object[] { metaDataNode });
                // Since subCollection type implements IMetadataCollection<ANYTHING>
                // I somehow need to call the Load(metaDataNode) method on its instance

            }


            return entity;
        }

        public abstract void Load(XElement xElement);
    }
}
