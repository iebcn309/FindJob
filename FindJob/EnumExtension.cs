using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FindJob
{
    /// <summary>
    /// 枚举拓展
    /// </summary>
    public static class EnumExtension
    {
        // 枚举显示字典缓存
        private static readonly ConcurrentDictionary<Type, Dictionary<int, string>> EnumDisplayValueDict = new();

        // 枚举值字典缓存
        private static readonly ConcurrentDictionary<Type, Dictionary<int, string>> EnumNameValueDict = new();
       
        /// <summary>
        /// 获取字段特性
        /// </summary>
        /// <param name="field"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetDescriptionValue<T>(this FieldInfo field) where T : Attribute
        {
            // 获取字段的指定特性，不包含继承中的特性
            object[] customAttributes = field.GetCustomAttributes(typeof(T), false);

            // 如果没有数据返回null
            return customAttributes.Length > 0 ? (T)customAttributes[0] : null;
        }
        /// <summary>
        /// 获取枚举的Description
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetDescription(this System.Enum value)
        {
            return value.GetType().GetMember(value.ToString()).FirstOrDefault()?.GetCustomAttribute<DescriptionAttribute>()
                ?.Description;
        }

        /// <summary>
        /// 获取枚举的Description
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetDescription(this object value)
        {
            return value.GetType().GetMember(value.ToString() ?? string.Empty).FirstOrDefault()
                ?.GetCustomAttribute<DescriptionAttribute>()?.Description;
        }

        /// <summary>
        /// 将枚举转成枚举信息集合
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static List<EnumEntity> EnumToList(this Type type)
        {
            if (!type.IsEnum)
                throw new ArgumentException("Type '" + type.Name + "' is not an enum.");
            var arr = System.Enum.GetNames(type);
            return arr.Select(sl =>
            {
                var item = System.Enum.Parse(type, sl);
                return new EnumEntity
                {
                    Name = item.ToString(),
                    Describe = item.GetDescription() ?? item.ToString(),
                    Value = item.GetHashCode()
                };
            }).ToList();
        }

        /// <summary>
        /// 枚举ToList
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type"></param>
        /// <returns></returns>
        public static List<T> EnumToList<T>(this Type type)
        {
            if (!type.IsEnum)
                throw new ArgumentException("Type '" + type.Name + "' is not an enum.");
            var arr = System.Enum.GetNames(type);
            return arr.Select(name => (T)System.Enum.Parse(type, name)).ToList();
        }
    }

    /// <summary>
    /// 枚举实体
    /// </summary>
    public class EnumEntity
    {
        /// <summary>
        /// 枚举的描述
        /// </summary>
        public string Describe { set; get; }

        /// <summary>
        /// 枚举名称
        /// </summary>
        public string Name { set; get; }

        /// <summary>
        /// 枚举对象的值
        /// </summary>
        public int Value { set; get; }
    }
}
