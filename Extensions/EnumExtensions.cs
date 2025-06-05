using System.ComponentModel.DataAnnotations;
using System.Reflection;

public static class EnumExtensions
{
    public static string GetDisplayName(this Enum value)
    {
        return value.GetType()
                    .GetMember(value.ToString())[0]
                    .GetCustomAttribute<DisplayAttribute>()?
                    .Name ?? value.ToString();
    }
}