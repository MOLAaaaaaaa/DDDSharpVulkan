using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DataCollection
{
    /// <summary>
    ///1.设置PropertyGrid的PropertySort为Categorized或者CategorizedAlphabetical
    ///2.要排序的类加注该属性[TypeConverter(typeof(PropertiesSortedByClassDefinitionConverter))]
    ///3.如果需要Category下的元素也按照类定义排序,使用[TypeConverter(typeof(CategoriesSortedByClassDefinitionConverter)),CategoriesSortedByClassDefinitionConverter.ElementsSameOrder]
    /// </summary>
    public class CategoriesSortedByClassDefinitionConverter : ExpandableObjectConverter
    {
        /// <summary>
        ///原本以为PropertySort属性为Categorized而不是CategorizedAlphabetical的话，Category下的属性会按照类定义排序。
        ///测试时发现好像有时候还是会乱序。
        /// </summary>
        [AttributeUsage(AttributeTargets.Class)]
        public class ElementsSameOrderAttribute : Attribute
        {
            public bool IsSameOrder { get; set; } = true;
            public ElementsSameOrderAttribute(bool isSameOrder = true)
            {
                IsSameOrder = isSameOrder;
            }
        }
        public override bool GetPropertiesSupported(ITypeDescriptorContext context)
        {
            return true;
        }
        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
        {
            PropertyDescriptorCollection pdcFirst = TypeDescriptor.GetProperties(value, attributes);
            List<PropertyDescriptor> pdc = new List<PropertyDescriptor>();
            for (int i = 0; i < pdcFirst.Count; i++)
            {
                pdc.Add(pdcFirst[i]);
            }

            //按照先子类后父类排序
            pdc = pdc.OrderBy(n => GetBaseTypeList(n.ComponentType).Count).ToList();

            var categoryNames = new List<string>();
            var categoryNamesOrigin = new List<string>();
            for (int i = 0; i < pdc.Count; i++)
            {
                var tempCategory = pdc[i].Category.Replace("\t", "");
                categoryNamesOrigin.Add(tempCategory);
                categoryNames.Add(tempCategory);
            }
            categoryNames = categoryNames.Distinct().ToList();
            var propertyNames = new List<string>();
            for (int i = 0; i < pdc.Count; i++)
            {
                propertyNames.Add(pdc[i].Name);
                //反射更改每个Category的名字达到效果
                SetValue(pdc[i], "category", categoryNamesOrigin[i].PadLeft(categoryNamesOrigin[i].Length + categoryNames.Count - categoryNames.IndexOf(categoryNamesOrigin[i]), '\t'));
            }

            //判断是否强制Category中的元素按照类的定义顺序进行排布
            bool bElementShouldSort = false;
            var contextAttrList = TypeDescriptor.GetAttributes(value).OfType<ElementsSameOrderAttribute>()?.ToList();
            for (int i = 0; i < contextAttrList.Count; i++)
            {
                if (contextAttrList[i] is ElementsSameOrderAttribute element)
                {
                    bElementShouldSort = element.IsSameOrder;
                }
            }

            return bElementShouldSort ? pdcFirst.Sort(propertyNames.ToArray()) : pdcFirst;
        }
        private void SetValue(object self, string fieldName, object value)
        {
            Type type = self.GetType();
            while (type != typeof(object))
            {
                var fields = type.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).ToList();
                var targetField = fields.Find(n => n.Name == fieldName || n.Name == $"<{fieldName}>k__BackingField");
                if (targetField != null)
                {
                    targetField.SetValue(self, value);
                    break;
                }
                type = type.BaseType;
            }
        }
        private List<Type> GetBaseTypeList(Type type)
        {
            List<Type> types = new List<Type>();
            if (type != null && type.BaseType != null)
            {
                types.Add(type.BaseType);
                types.AddRange(GetBaseTypeList(type.BaseType));
            }
            return types;
        }
    }
}
