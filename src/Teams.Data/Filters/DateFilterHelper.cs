using Teams.Data.Models;
using Teams.Domain.Interfaces;

namespace Teams.Data.Filters;

public static class DateFilterHelper
{
    public static IQueryable<T> ApplyBaseEntityDateFilters<T>(this IQueryable<T> queryable, DateFilter? value)
        where T : IHasCreatedTimestamp, IHasModifiedTimestamp
        => value == null
            ? queryable
            : queryable
                .ApplyCreatedFromFilter(value.Created?.From)
                .ApplyCreatedToFilter(value.Created?.To)
                .ApplyModifiedFromFilter(value.Modified?.From)
                .ApplyModifiedToFilter(value.Modified?.To);
}