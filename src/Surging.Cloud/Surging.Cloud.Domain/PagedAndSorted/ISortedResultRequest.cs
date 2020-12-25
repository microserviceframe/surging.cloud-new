using System.Collections.Generic;

namespace Surging.Cloud.Domain.PagedAndSorted
{
    public interface ISortedResultRequest
    {
        IDictionary<string,SortType> Sorting { get; set; }

    }
}
