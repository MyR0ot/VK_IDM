using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace VK_IDM
{
    public class CaseInsensitiveConverter<T> : JsonConverter where T : class
    {
        public override bool CanRead => true;

        public override bool CanWrite => false;

        public override bool CanConvert(Type objectType) => typeof(T).IsAssignableFrom(objectType);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var targetObject = existingValue as T ?? Activator.CreateInstance<T>();

            foreach (var jsonProp in JObject.Load(reader).Properties())
            {
                var targetProp = typeof(T).GetProperty(
                        jsonProp.Name.ToCamelCaseFormats(),
                        BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance
                        );
                    
                if (targetProp != null && targetProp.CanWrite)
                {
                    var value = jsonProp.Value.ToObject(targetProp.PropertyType);
                    var setter = CreateSetter(targetProp);
                    setter(targetObject, value);
                }
            }

            return targetObject;
        }

        private static Action<object, object> CreateSetter(PropertyInfo property)
        {
            var target = Expression.Parameter(typeof(object), "target");
            var value = Expression.Parameter(typeof(object), "value");

            var targetCast = property.DeclaringType.IsValueType
                ? Expression.Unbox(target, property.DeclaringType)
                : Expression.Convert(target, property.DeclaringType);

            var valueCast = property.PropertyType.IsValueType
                ? Expression.Unbox(value, property.PropertyType)
                : Expression.Convert(value, property.PropertyType);

            var setterCall = Expression.Call(targetCast, property.GetSetMethod(), valueCast);

            return Expression.Lambda<Action<object, object>>(setterCall, target, value).Compile();
        }


        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotSupportedException();
        }

    }


    public static class StringUtils
    {
        public static string ToCamelCaseFormats(this string text)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));

            if (text.Length <= 1)
                return text.ToLower();

            var sb = new StringBuilder();
            sb.Append(char.ToLowerInvariant(text[0]));
            bool isLastSnake = false;
            for (int i = 1; i < text.Length; i++)
            {
                char c = text[i];
                if (c == '_')
                {
                    isLastSnake = true;
                }
                else
                {
                    sb.Append(isLastSnake ? char.ToUpperInvariant(c) : c);
                    isLastSnake = false;

                }
            }
            return sb.ToString();
        }

        public static string ToSnakeCaseFormat(this string text)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));

            if (text.Length <= 1)
                return text.ToLower();

            var sb = new StringBuilder();
            sb.Append(char.ToLowerInvariant(text[0]));
            for (int i = 1; i < text.Length; i++)
            {
                char c = text[i];
                if (char.IsUpper(c))
                {
                    sb.Append('_');
                    sb.Append(char.ToLowerInvariant(c));
                }
                else
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }
    }


}
