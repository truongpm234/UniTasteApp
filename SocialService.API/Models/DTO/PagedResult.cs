﻿using NPOI.SS.Formula.Functions;

namespace SocialService.API.Models.DTO
{
    public class PagedResult<T>
    {
        public IEnumerable<T> Items { get; set; } = new List<T>(); 
        public int TotalCount { get; set; } 
        public int Page { get; set; } 
        public int PageSize { get; set; } 

        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize); 
    }
}
