namespace Surging.Cloud.Domain.PagedAndSorted
{
    public interface IPagedResult<T> : IListResult<T>, IHasTotalCount
    {

    }
}
