
using Core.Common;
using Core.Entities;
using Core.Interfaces.Repositories;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
    {
        protected readonly ApplicationDbContext _dbContext;
        protected readonly DbSet<T> _dbSet;

        public GenericRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
            _dbSet = dbContext.Set<T>();
        }

        public async Task<T?> GetByIdAsync(long id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task<IReadOnlyList<T>> ListAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }

        public async Task<T?> FindSingleWithIncludeStringAsync(
            Expression<Func<T, bool>> predicate,
            string? includeString = null,
            bool disableTracking = true)
        {
            IQueryable<T> query = _dbSet;

            if (disableTracking)
                query = query.AsNoTracking();

            if (!string.IsNullOrEmpty(includeString))
                query = query.Include(includeString);

            return await query.SingleOrDefaultAsync(predicate);
        }

        public async Task<T?> FindSingleWithIncludesAsync(
            Expression<Func<T, bool>> predicate,
            List<Expression<Func<T, object>>>? includes = null,
            bool disableTracking = true)
        {
            IQueryable<T> query = _dbSet;

            if (disableTracking)
                query = query.AsNoTracking();

            if (includes != null)
            {
                foreach (var include in includes)
                {
                    query = query.Include(include);
                }
            }

            return await query.SingleOrDefaultAsync(predicate);
        }

        public async Task<IReadOnlyList<T>> FindAsync(
            Expression<Func<T, bool>>? predicate = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            string? includeString = null,
            bool disableTracking = true)
        {
            IQueryable<T> query = _dbSet;

            if (disableTracking)
                query = query.AsNoTracking();

            if (!string.IsNullOrEmpty(includeString))
                query = query.Include(includeString);

            if (predicate != null)
                query = query.Where(predicate);

            if (orderBy != null)
                return await orderBy(query).ToListAsync();

            return await query.ToListAsync();
        }

        public async Task<IReadOnlyList<T>> FindAsync(
            Expression<Func<T, bool>>? predicate = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            List<Expression<Func<T, object>>>? includes = null,
            bool disableTracking = true)
        {
            IQueryable<T> query = _dbSet;

            if (disableTracking)
                query = query.AsNoTracking();

            if (includes != null)
            {
                foreach (var include in includes)
                {
                    query = query.Include(include);
                }
            }

            if (predicate != null)
                query = query.Where(predicate);

            if (orderBy != null)
                return await orderBy(query).ToListAsync();

            return await query.ToListAsync();
        }

        public async Task<PagedList<T>> GetPagedListAsync(
       PagingParameters pagingParameters,
       Expression<Func<T, bool>>? predicate = null,
       Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
       List<Expression<Func<T, object>>>? includes = null,
       bool disableTracking = true)
        {
            IQueryable<T> query = _dbSet;

            if (disableTracking)
                query = query.AsNoTracking();

            if (includes != null)
            {
                foreach (var include in includes)
                {
                    query = query.Include(include);
                }
            }

            if (predicate != null)
                query = query.Where(predicate);

            // Apply sorting
            if (orderBy != null)
            {
                query = orderBy(query);
            }
            else if (!string.IsNullOrEmpty(pagingParameters.SortColumn))
            {
                // Apply dynamic sorting based on PagingParameters
                var property = typeof(T).GetProperty(pagingParameters.SortColumn,
                    System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

                if (property != null)
                {
                    var parameter = Expression.Parameter(typeof(T), "x");
                    var propertyAccess = Expression.MakeMemberAccess(parameter, property);
                    var orderByExp = Expression.Lambda(propertyAccess, parameter);

                    var isDescending = pagingParameters.SortOrder?.ToLower() == "desc";

                    var resultExp = isDescending
                        ? Expression.Call(typeof(Queryable), "OrderByDescending", new Type[] { typeof(T), property.PropertyType },
                            query.Expression, Expression.Quote(orderByExp))
                        : Expression.Call(typeof(Queryable), "OrderBy", new Type[] { typeof(T), property.PropertyType },
                            query.Expression, Expression.Quote(orderByExp));

                    query = query.Provider.CreateQuery<T>(resultExp);
                }
            }

            // Apply search if provided
            if (!string.IsNullOrEmpty(pagingParameters.SearchTerm))
            {
                query = ApplySearch(query, pagingParameters.SearchTerm);
            }

            // Get total count before pagination
            var totalCount = await query.CountAsync();

            // Apply pagination
            var items = await query
                .Skip((pagingParameters.PageNumber - 1) * pagingParameters.PageSize)
                .Take(pagingParameters.PageSize)
                .ToListAsync();

            return new PagedList<T>(items, totalCount, pagingParameters.PageNumber, pagingParameters.PageSize);
        }
        /// <summary>
        /// Applies search functionality to a query, searching across string properties
        /// </summary>
        private IQueryable<T> ApplySearch(IQueryable<T> query, string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
                return query;

            searchTerm = searchTerm.ToLower();

            // Get all string properties of the entity type that are directly on the entity (not navigation properties)
            var stringProperties = typeof(T).GetProperties()
                .Where(p =>
                    (p.PropertyType == typeof(string) || p.PropertyType == typeof(string)!) &&
                    p.CanRead &&
                    p.GetMethod?.IsPublic == true)
                .ToList();

            if (!stringProperties.Any())
                return query;

            // Create a parameter expression for the entity
            var parameter = Expression.Parameter(typeof(T), "x");

            // Build the OR expression for all string properties
            Expression? combinedExpression = null;

            foreach (var property in stringProperties)
            {
                try
                {
                    // Build property access: x.PropertyName
                    var propertyAccess = Expression.Property(parameter, property);

                    // Handle null values with a null check: x.PropertyName != null
                    var nullCheck = Expression.NotEqual(propertyAccess, Expression.Constant(null, property.PropertyType));

                    // ToLower() call: x.PropertyName.ToLower()
                    var toLowerMethodInfo = typeof(string).GetMethod("ToLower", new Type[] { });
                    var toLowerCall = Expression.Call(propertyAccess, toLowerMethodInfo);

                    // Contains() call: x.PropertyName.ToLower().Contains(searchTerm)
                    var containsMethodInfo = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                    var containsCall = Expression.Call(toLowerCall, containsMethodInfo, Expression.Constant(searchTerm));

                    // Combine with null check: x.PropertyName != null && x.PropertyName.ToLower().Contains(searchTerm)
                    var safePredicate = Expression.AndAlso(nullCheck, containsCall);

                    if (combinedExpression == null)
                    {
                        combinedExpression = safePredicate;
                    }
                    else
                    {
                        // Combine with OR: previousExpression || (x.PropertyName != null && x.PropertyName.ToLower().Contains(searchTerm))
                        combinedExpression = Expression.OrElse(combinedExpression, safePredicate);
                    }
                }
                catch
                {
                    // Skip properties that cause issues with expression building
                    continue;
                }
            }

            // If we couldn't build an expression, return the original query
            if (combinedExpression == null)
                return query;

            // Create the lambda expression: x => combined expression
            var lambda = Expression.Lambda<Func<T, bool>>(combinedExpression, parameter);

            // Apply the where clause to the query
            return query.Where(lambda);
        }

        public void Add(T entity)
        {
            _dbSet.Add(entity);
        }

        public void AddRange(IEnumerable<T> entities)
        {
            _dbSet.AddRange(entities);
        }

        public void Update(T entity)
        {
            _dbSet.Attach(entity);
            _dbContext.Entry(entity).State = EntityState.Modified;
        }

        public void UpdateRange(IEnumerable<T> entities)
        {
            foreach (var entity in entities)
            {
                Update(entity);
            }
        }

        public void Remove(T entity)
        {
            _dbSet.Remove(entity);
        }

        public void RemoveRange(IEnumerable<T> entities)
        {
            _dbSet.RemoveRange(entities);
        }

        public bool Exists(long id)
        {
            return _dbSet.Any(e => e.Id == id);
        }

        public async Task<int> CountWithDistinctAsync <TProperty>(Expression<Func<T, bool>> predicate, Expression<Func<T, TProperty>> selector)
        {
            return await _dbSet
                .Where(predicate)
                .Select(selector)
                .Distinct()
                .CountAsync();
        }
        public async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null)
        {
            if (predicate == null)
                return await _dbSet.CountAsync();

            return await _dbSet.CountAsync(predicate);
        }
    }
}
