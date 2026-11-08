namespace Banking.History.Api.Application.Common
{
        public sealed record PagedResult<T>(
        IReadOnlyList<T> Items,
        int Page,
        int PageSize,
        long TotalItems,
        long TotalPages
    );
}
