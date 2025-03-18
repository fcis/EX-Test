using Core.Common;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Application.Helpers
{
    public static class PaginationHelper
    {
        public static void AddPaginationHeader(this HttpResponse response, PaginationHeader header)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            response.Headers.Append("X-Pagination", JsonSerializer.Serialize(header, options));
            response.Headers.Append("Access-Control-Expose-Headers", "X-Pagination");
        }

        public static PaginationHeader CreatePaginationHeader<T>(PagedList<T> pagedList) where T : class
        {
            return new PaginationHeader(
                pagedList.PageNumber,
                pagedList.PageSize,
                pagedList.TotalCount,
                pagedList.TotalPages,
                pagedList.HasPreviousPage,
                pagedList.HasNextPage,
                pagedList.FirstItemIndex,
                pagedList.LastItemIndex
            );
        }
    }

    public static class SortingHelper
    {
        public static IQueryable<T> ApplySorting<T>(this IQueryable<T> query, string sortColumn, string sortOrder)
        {
            if (string.IsNullOrEmpty(sortColumn))
                return query;

            // This is a simplified version. In a real application, you'd use a more dynamic approach
            // such as Expression trees or a dictionary mapping of property names

            // Example for a specific entity type (not generic)
            /*
            if (typeof(T) == typeof(Framework))
            {
                return sortColumn.ToLower() switch
                {
                    "name" => sortOrder.ToLower() == "desc" 
                        ? query.OrderByDescending(e => ((Framework)(object)e).Name) 
                        : query.OrderBy(e => ((Framework)(object)e).Name),
                    "creationdate" => sortOrder.ToLower() == "desc" 
                        ? query.OrderByDescending(e => ((Framework)(object)e).CreationDate)
                        : query.OrderBy(e => ((Framework)(object)e).CreationDate),
                    _ => query
                };
            }
            */

            return query;
        }
    }

    public static class SearchHelper
    {
        public static IQueryable<T> ApplySearch<T>(this IQueryable<T> query, string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
                return query;

            // This is a simplified version. In a real application, you'd use a more dynamic approach
            // such as Expression trees to build a search predicate

            return query;
        }
    }
}
