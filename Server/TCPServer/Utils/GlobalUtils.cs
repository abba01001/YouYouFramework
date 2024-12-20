using Protocols.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TCPServer.Utils
{
    public class GlobalUtils
    {
        // 用静态构造函数来初始化 playerDataKey，只执行一次
        private static Dictionary<Type, List<string>> playerDataKeyCache = new Dictionary<Type, List<string>>();

        // 校验方法，确保 updatedAttrs 中的所有键都存在于指定类型的属性中
        public static bool ValidateKey<T>(Dictionary<string, object> updatedAttrs)
        {
            if (updatedAttrs == null || updatedAttrs.Count == 0)
                return false;

            // 获取目标类型的属性名
            List<string> propertyNames = GetPropertyNames<T>();

            // 检查 updatedAttrs 中的所有键是否都存在于目标类型的属性中
            foreach (var item in updatedAttrs)
            {
                if (!propertyNames.Contains(item.Key))
                    return false;
            }
            return true;
        }

        // 获取指定类型 T 的所有属性名
        private static List<string> GetPropertyNames<T>()
        {
            // 如果已经缓存了属性名，则直接返回
            Type type = typeof(T);
            if (playerDataKeyCache.ContainsKey(type))
            {
                return playerDataKeyCache[type];
            }

            // 否则通过反射获取属性名，并缓存
            List<string> propertyNames = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                              .Select(prop => prop.Name)
                                              .ToList();
            playerDataKeyCache[type] = propertyNames;
            return propertyNames;
        }
    }
}
