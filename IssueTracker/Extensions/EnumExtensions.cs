using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace IssueTracker.Web.Extensions
{
    public static class EnumExtensions
    {
        public static string GetDisplayName(this Enum item)
        {
            return item.GetType()
                .GetMember(item.ToString()).Single()
                .GetCustomAttribute<DisplayAttribute>(false)?.Name
                    ?? item.ToString();
        }
    }
}
