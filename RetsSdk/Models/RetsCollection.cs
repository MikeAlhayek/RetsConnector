using CrestApps.RetsSdk.Contracts;
using CrestApps.RetsSdk.Helpers.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace CrestApps.RetsSdk.Models
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

            // First, we check all attributes on the XElement
            // for any attribute that match the element, we would set the value accordingly.
            foreach (PropertyInfo property in type.GetProperties())
            {
                var attribute = xElement.Attribute(property.Name);

                if (attribute == null)
                {
                    continue;
                }

                SetValueSafely(this, property, attribute.Value);
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


        protected T Cast(XElement element)
        {
            var entity = new T();

            XElement parent = element.Parent;

            IEnumerable<PropertyInfo> properties = typeof(T).GetProperties();


            // Set the entity properites using the current attributes
            foreach (PropertyInfo property in properties)
            {
                if (parent != null)
                {
                    // We first check for any attributes that match this property on the parent and set it
                    var parentAttribute = parent.Attribute(property.Name);

                    if (parentAttribute != null)
                    {
                        SetValueSafely(entity, property, parentAttribute.Value);
                    }
                }

                // Second, check for any attributes that match this property on element directly
                // This value will override the previous value if one already is set
                var attribute = element.Attribute(property.Name);

                if (attribute != null)
                {
                    SetValueSafely(entity, property, attribute.Value);
                }                
            }

            // First, foreach child on the given XElement object, find a propery with the same name as the child's localname and set its value accordingly
            foreach (XElement child in element.Elements())
            {
                PropertyInfo property = GetGenericType().GetProperty(child.Name.LocalName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                if (property == null)
                {
                    continue;
                }

                SetValueSafely(entity, property, child.Value);
            }

            // Second, foreach property of the generic entity that implements IMetadataCollection<ANYTHING>
            // find the sub Element and call Load() method using different generics
            var subCollections = properties.Where(x => typeof(IRetsCollectionXElementLoader).IsAssignableFrom(x.PropertyType)).ToList();

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


                subCollection.SetValue(entity, newCollection, null);
            }


            return entity;
        }

        private static void SetValueSafely(object entity, PropertyInfo property, string value)
        {
            object safeValue = property.PropertyType.GetSafeObject(value);

            property.SetValue(entity, safeValue, null);
        }

        public abstract void Load(XElement xElement);
        public abstract T Get(object value);

    }
}
