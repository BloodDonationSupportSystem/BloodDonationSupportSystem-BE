using System;

namespace Shared.Models
{
    public class PaginationParameters
    {
        private const int MaxPageSize = 50;
        private int _pageSize = 10;
        private int _pageNumber = 1;

        public int PageNumber 
        { 
            get => _pageNumber; 
            set => _pageNumber = Math.Max(1, value); 
        }

        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = Math.Min(MaxPageSize, Math.Max(1, value));
        }

        // Sort params
        public string SortBy { get; set; }
        public bool SortAscending { get; set; } = true;
    }
}