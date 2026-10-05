namespace Cessa.Application.Common.Pagination
{
    using System;
    using System.Collections.Generic;

    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
        public bool HasPreviousPage => Page > 1;
        public bool HasNextPage => Page < TotalPages;
        public int FirstItemOnPage => (Page - 1) * PageSize + 1;
        public int LastItemOnPage => Math.Min(Page * PageSize, TotalCount);
    }
}
