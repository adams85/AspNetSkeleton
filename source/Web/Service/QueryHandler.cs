﻿using System;
using System.Collections;
using System.Linq;
using System.Linq.Expressions;
using AspNetSkeleton.Service.Contract;
using AspNetSkeleton.Common.Utils;
using System.Threading;
using System.Threading.Tasks;
using System.Data.Entity;
using Karambolo.Common;

namespace AspNetSkeleton.Service
{
    public interface IQueryHandler<TQuery, TResult>
        where TQuery : IQuery<TResult>
    {
        Task<TResult> HandleAsync(TQuery query, CancellationToken cancellationToken);
    }

    public abstract class ListQueryHandler<TQuery, TResult, T> : IQueryHandler<TQuery, TResult>
        where TQuery : ListQuery<TResult, T>
        where TResult : ListResult<T>, new()
    {
        public abstract Task<TResult> HandleAsync(TQuery query, CancellationToken cancellationToken);

        protected virtual IOrderedQueryable<T> ApplyColumnOrder(IQueryable<T> linq, string columnName, bool descending, bool nested)
        {
            return nested ? ((IOrderedQueryable<T>)linq).ThenBy(columnName, descending) : linq.OrderBy(columnName, descending);
        }

        protected void Validate(TQuery query)
        {
            if (query.IsPaged)
            {
                this.RequireValid(query.PageIndex >= 0, q => q.PageIndex);
                this.RequireValid(query.PageSize > 0, q => q.PageSize);
            }

            if (query.IsOrdered)
            {
                var count = query.OrderColumns.Length;
                for (var i = 0; i < count; i++)
                    this.RequireValid(!string.IsNullOrEmpty(query.OrderColumns[i]), q => q.OrderColumns);
            }
        }

        protected IQueryable<T> Apply(TQuery query, IQueryable<T> linq)
        {
            if (!ArrayUtils.IsNullOrEmpty(query.OrderColumns))
            {
                bool ParseOrderColumn(string value, out string cn)
                {
                    var c = value[0];
                    switch (c)
                    {
                        case '+':
                        case '-':
                            cn = value.Substring(1);
                            return c == '-';
                        default:
                            cn = value;
                            return false;
                    }
                }

                var orderColumnCount = query.OrderColumns.Length;
                for (var i = 0; i < orderColumnCount; i++)
                {
                    var descending = ParseOrderColumn(query.OrderColumns[i], out string columnName);
                    linq = ApplyColumnOrder(linq, columnName, descending, nested: i > 0);
                }
            }

            if (query.PageIndex != null)
                linq = linq.Skip(query.PageIndex.Value);

            if (query.PageSize != null)
                linq = linq.Take(query.PageSize.Value);

            return linq;
        }

        protected async Task<TResult> ResultAsync(TQuery query, IQueryable<T> linq, CancellationToken cancellationToken)
        {
            var result = new TResult();
            result.Rows = await linq.ToArrayAsync(cancellationToken).ConfigureAwait(false);
            result.TotalRowCount = query.IsPaged ? await linq.CountAsync(cancellationToken).ConfigureAwait(false) : result.Rows.Length;
            result.PageIndex = query.PageIndex ?? 0;
            result.PageSize = query.PageSize ?? 0;
            return result;
        }

        protected TResult Result(T[] rows, int totalRowCount, int pageIndex, int pageSize)
        {
            return new TResult { Rows = rows, TotalRowCount = totalRowCount, PageIndex = pageIndex, PageSize = pageSize };
        }
    }

    public abstract class ListQueryHandler<TQuery, T> : ListQueryHandler<TQuery, ListResult<T>, T>
        where TQuery : ListQuery<T>
    { }

    public static class QueryHandlerUtils
    {
        public static void Require<TQuery, TResult>(this IQueryHandler<TQuery, TResult> @this, bool condition, QueryErrorCode errorCode, Func<object[]> argsFactory = null)
            where TQuery : IQuery<TResult>
        {
            if (!condition)
                throw new QueryErrorException(errorCode, (argsFactory ?? Default<object[]>.Func)());
        }

        public static void RequireSpecified<TQuery, TResult, T>(this IQueryHandler<TQuery, TResult> @this, T @param, Expression<Func<TQuery, T>> paramPath, bool emptyAllowed = false)
            where TQuery : IQuery<TResult>
        {
            string paramString;
            ICollection paramCollection;
            @this.Require(
                @param != null &&
                (emptyAllowed || (paramString = @param as string) == null || paramString.Length > 0) &&
                (emptyAllowed || (paramCollection = @param as object[]) == null || paramCollection.Count > 0),
                QueryErrorCode.ParamNotSpecified, () => new[] { Lambda.PropertyPath(paramPath) });
        }

        public static void RequireValid<TQuery, TResult, T>(this IQueryHandler<TQuery, TResult> @this, bool condition, Expression<Func<TQuery, T>> paramPath)
            where TQuery : IQuery<TResult>
        {
            @this.Require(condition, QueryErrorCode.ParamNotValid, () => new[] { Lambda.PropertyPath(paramPath) });
        }

        public static void RequireExisting<TQuery, TResult, T>(this IQueryHandler<TQuery, TResult> @this, object entity, Expression<Func<TQuery, T>> paramPath)
            where TQuery : IQuery<TResult>
        {
            @this.Require(entity != null, QueryErrorCode.EntityNotFound, () => new[] { Lambda.PropertyPath(paramPath) });
        }
    }
}