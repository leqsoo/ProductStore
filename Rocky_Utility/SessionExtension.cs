using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Rocky_Utility
{
    public static class SessionExtension
    {
        public static void Set<T>(this ISession sesion, string key, T value)
        {
            sesion.SetString(key, JsonSerializer.Serialize(value));
        }
        public static T Get<T>(this ISession sesion, string key)
        {
            var value = sesion.GetString(key);

            return value == null ? default : JsonSerializer.Deserialize<T>(value);
        }
    }
}
