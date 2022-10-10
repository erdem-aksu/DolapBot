using System;
using System.Reflection;
using System.Runtime.Serialization;

namespace DolapBot.Client.Extensions
{
    public static class EnumExtensions
    {
        public static string GetStringValue(this Enum enumVal)
        {
            var member = enumVal.GetType().GetMember(enumVal.ToString())[0];
            var enumMember = member.GetCustomAttribute<EnumMemberAttribute>(false);

            return enumMember == null ? member.Name : enumMember.Value;
        }
    }
}