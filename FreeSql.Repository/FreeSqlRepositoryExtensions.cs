﻿using FreeSql;
using System;
using System.Linq;
using System.Linq.Expressions;

public static class FreeSqlRepositoryExtensions
{
    public static IBaseRepository<TEntity> GetAggregateRootRepository<TEntity>(this IFreeSql that) where TEntity : class
    {
        return new AggregateRootRepository<TEntity>(that);
    }
}