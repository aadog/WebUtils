using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace WebUtils
{
    public static class EnumHelper
    {
        public static object[] ToDataSource<TEnum>() where TEnum : Enum
        {
            return Enum.GetValues(typeof(TEnum))
                .Cast<TEnum>()
                .Select(e =>
                {
                    var member = e.GetType().GetField(e.ToString());
                    var display = member.GetCustomAttribute<DisplayAttribute>()?.Name ?? e.ToString();
                    return new
                    {
                        Value = Convert.ToInt32(e), // 或者 e.ToString() 取字符串
                        Text = display
                    };
                })
                .ToArray();
        }
    }
}
