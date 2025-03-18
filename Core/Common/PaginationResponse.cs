using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Common
{
    public class PaginationResponse<T> where T : class
    {
        public IReadOnlyList<T> Items { get; set; } = new List<T>();
        public PaginationHeader Pagination { get; set; } = null!;

        public PaginationResponse(IReadOnlyList<T> items, PaginationHeader pagination)
        {
            Items = items;
            Pagination = pagination;
        }

        public static PaginationResponse<T> FromPagedList(PagedList<T> pagedList)
        {
            var paginationHeader = new PaginationHeader(
                pagedList.PageNumber,
                pagedList.PageSize,
                pagedList.TotalCount,
                pagedList.TotalPages,
                pagedList.HasPreviousPage,
                pagedList.HasNextPage,
                pagedList.FirstItemIndex,
                pagedList.LastItemIndex
            );

            return new PaginationResponse<T>(pagedList.Items, paginationHeader);
        }
    }
}
