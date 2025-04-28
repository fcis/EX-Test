using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Common
{
    public class PagingParameters
    {
        private const int MaxPageSize = 50;
        private int _pageSize = 10;

        public int PageNumber { get; set; } = 1;

        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = (value > MaxPageSize) ? MaxPageSize : value;
        }

        public string? SortColumn { get; set; }
        public string? SortOrder { get; set; } // "asc" or "desc"
        public string? SearchTerm { get; set; }
        public DateTime? SearchByCreationDate { get; set; }
        public DateTime? SearchByModifyDate { get; set; }
    }
}
