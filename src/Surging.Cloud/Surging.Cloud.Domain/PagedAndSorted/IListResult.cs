using System.Collections.Generic;

namespace Surging.Cloud.Domain.PagedAndSorted
{
    public interface IListResult<T>
    {
        IReadOnlyList<T> Items { get; set; }
    }
}
