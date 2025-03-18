using Core.Common;
using Core.Entities;
using Microsoft.AspNetCore.Http;
using System.Linq.Expressions;
using System.Net.Http;
using System.Reflection;

namespace Application.Extensions
{
    public static class EnumExtensions
    {
        public static string GetDisplayName(this Enum value)
        {
            var field = value.GetType().GetField(value.ToString());

            if (field?.GetCustomAttributes(typeof(System.ComponentModel.DisplayNameAttribute), false) is System.ComponentModel.DisplayNameAttribute[] attributes && attributes.Length > 0)
            {
                return attributes[0].DisplayName;
            }

            return value.ToString();
        }
    }

    public static class QueryableExtensions
    {
        public static IQueryable<T> ApplyPaging<T>(this IQueryable<T> query, PagingParameters pagingParameters)
        {
            return query
                .Skip((pagingParameters.PageNumber - 1) * pagingParameters.PageSize)
                .Take(pagingParameters.PageSize);
        }

        public static IQueryable<T> ApplyOrderBy<T>(this IQueryable<T> query, string propertyName, bool isDescending = false)
        {
            if (string.IsNullOrEmpty(propertyName))
                return query;

            var entityType = typeof(T);
            var property = entityType.GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

            if (property == null)
                return query;

            var parameter = Expression.Parameter(entityType, "x");
            var propertyAccess = Expression.MakeMemberAccess(parameter, property);
            var orderByExp = Expression.Lambda(propertyAccess, parameter);

            var resultExp = isDescending ?
                Expression.Call(typeof(Queryable), "OrderByDescending", new Type[] { entityType, property.PropertyType }, query.Expression, Expression.Quote(orderByExp)) :
                Expression.Call(typeof(Queryable), "OrderBy", new Type[] { entityType, property.PropertyType }, query.Expression, Expression.Quote(orderByExp));

            return query.Provider.CreateQuery<T>(resultExp);
        }
    }

    public static class HttpContextExtensions
    {
        public static string GetIpAddress(this HttpContext context)
        {
            var ipAddress = context.Connection.RemoteIpAddress?.ToString();

            // Check for forwarded IP (if behind a load balancer or proxy)
            var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                // The X-Forwarded-For header can contain multiple IP addresses in a comma-separated list
                // The leftmost IP address is the original client IP
                ipAddress = forwardedFor.Split(',')[0].Trim();
            }

            return ipAddress ?? "Unknown";
        }
    }
}