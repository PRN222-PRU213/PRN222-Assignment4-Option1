using System.Collections.Generic;

namespace PRN222_Assignment4_Option1.BusinessLogic.Dtos;

public class PagedResultDto<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
}
