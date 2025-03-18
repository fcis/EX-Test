using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Common
{
    public class PaginationHeader
    {
        public int CurrentPage { get; set; }
        public int ItemsPerPage { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
        public bool HasPrevious { get; set; }
        public bool HasNext { get; set; }
        public int FirstItemIndex { get; set; }
        public int LastItemIndex { get; set; }

        public PaginationHeader(int currentPage, int itemsPerPage, int totalItems, int totalPages,
            bool hasPrevious, bool hasNext, int firstItemIndex, int lastItemIndex)
        {
            CurrentPage = currentPage;
            ItemsPerPage = itemsPerPage;
            TotalItems = totalItems;
            TotalPages = totalPages;
            HasPrevious = hasPrevious;
            HasNext = hasNext;
            FirstItemIndex = firstItemIndex;
            LastItemIndex = lastItemIndex;
        }
    }
}
