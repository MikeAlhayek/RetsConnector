using System;
using System.ComponentModel;
using System.Reflection;

namespace CrestApps.RetsSdk.Helpers.Extensions
{
    public static class EnumExtensions
    {
        public static string GetDescription(this Enum value)
        {
            Type type = value.GetType();

            string name = Enum.GetName(type, value);

            if (name != null)
            {
                FieldInfo field = type.GetField(name);
                if (field != null)
                {
                    if (Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) is DescriptionAttribute attr)
                    {
                        return attr.Description;
                    }
                }
            }

            return name;
        }

        public static string GetCategory(this Enum value)
        {
            Type type = value.GetType();

            string name = Enum.GetName(type, value);

            if (name != null)
            {
                FieldInfo field = type.GetField(name);
                if (field != null)
                {
                    if (Attribute.GetCustomAttribute(field, typeof(CategoryAttribute)) is CategoryAttribute attr)
                    {
                        return attr.Category;
                    }
                }
            }

            return null;
        }
    }
}
