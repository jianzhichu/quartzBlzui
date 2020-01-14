using QM.Interface;
using QM.Model;
using ServiceStack.OrmLite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace QM.Service
{
    public abstract class BaseRepository<T> : IRepository<T> where T : BaseModel
    {
        /// <summary>
        ///  Console.WriteLine(db.GetLastSql()); 可以查看生成的SQL
        /// </summary>
        private static OrmLiteConnectionFactory _ormLiteConnection;
        public BaseRepository()
        {
            _ormLiteConnection = ConnectionFactory.BuildDbConn();
        }


        public virtual long Count(Expression<Func<T, bool>> Where = null)
        {
            if (Where == null)
                Where = p => true;
            using var db = _ormLiteConnection.Open();

            return db.Count(Where);
        }

        public virtual async Task<long> CountAsync(Expression<Func<T, bool>> Where)
        {
            if (Where == null)
                Where = p => true;
            using var db = await _ormLiteConnection.OpenAsync();
            return await db.CountAsync(Where);
        }


        public virtual bool DeleteBy(Expression<Func<T, bool>> Where)
        {
            if (Where == null)
                Where = p => true;
            using var db = _ormLiteConnection.Open();
            return db.Delete(Where) > 0;
        }

        public virtual async Task<bool> DeleteByAsync(Expression<Func<T, bool>> Where)
        {
            if (Where == null)
                Where = p => true;
            using var db = await _ormLiteConnection.OpenAsync();
            return await db.DeleteAsync(Where) > 0;

        }

        public virtual bool ExecuteSql(string sql)
        {
            if (string.IsNullOrWhiteSpace(sql))
                return false;
            using var db = _ormLiteConnection.Open();
            try
            {
                db.ExecuteNonQuery(sql);
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        public virtual async Task<bool> ExecuteSqlAsync(string sql)
        {
            if (string.IsNullOrWhiteSpace(sql))
                return false;
            using var db = await _ormLiteConnection.OpenAsync();
            try
            {
                await db.ExecuteNonQueryAsync(sql);
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }



        public virtual bool Insert(T t)
        {
            using var db = _ormLiteConnection.Open();
            return db.Insert(t) > 0;
        }

        public virtual async Task<bool> InsertAsync(T t)
        {
            using var db = await _ormLiteConnection.OpenAsync();
            return await db.InsertAsync(t) > 0;
        }

        public virtual void InsertMuch(IEnumerable<T> list)
        {
            using var db = _ormLiteConnection.Open();
            db.InsertAll(list);
        }

        public virtual async Task InsertMuchAsync(IEnumerable<T> list)
        {
            using var db = await _ormLiteConnection.OpenAsync();
            await db.InsertAllAsync(list);//SaveAllAsync  会返回最后一条新增ID--- SELECT LAST_INSERT_ID()
            //Console.WriteLine(db.GetLastSql());
        }

        public virtual IEnumerable<T> Query(Expression<Func<T, bool>> Where, Order order = Order.Descending, Expression<Func<T, object>> OrderBy = null)
        {
            if (Where == null)
                Where = p => true;
            using var db = _ormLiteConnection.Open();
            var expressionFilter = db.From<T>().Where(Where);
            if (OrderBy != null)
                expressionFilter = order == Order.Descending ? expressionFilter.OrderByDescending(OrderBy) : expressionFilter.OrderBy(OrderBy);
            else
                expressionFilter = order == Order.Descending ? expressionFilter.OrderByDescending("Id") : expressionFilter.OrderBy("Id");
            return db.Select(expressionFilter);

        }

        public virtual async Task<IEnumerable<T>> QueryAsync(Expression<Func<T, bool>> Where, Order order = Order.Descending, Expression<Func<T, object>> OrderBy = null)
        {
            if (Where == null)
                Where = p => true;
            using var db = await _ormLiteConnection.OpenAsync();
            var expressionFilter = db.From<T>().Where(Where);
            if (OrderBy != null)
                expressionFilter = order == Order.Descending ? expressionFilter.OrderByDescending(OrderBy) : expressionFilter.OrderBy(OrderBy);
            else
                expressionFilter = order == Order.Descending ? expressionFilter.OrderByDescending("Id") : expressionFilter.OrderBy("Id");

            return await db.SelectAsync(expressionFilter);
        }

        public virtual T QuerySingle(Expression<Func<T, bool>> Where = null)
        {
            using var db = _ormLiteConnection.Open();
            //return db.SingleById<T>(Id);
            return db.Single<T>(Where);
        }

        /// <summary>
        /// SingleById 有坑。。ID不是根据主键特性匹配的，直接用了实体类第一个属性
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public virtual T QueryById(int Id)
        {
            using var db = _ormLiteConnection.Open();
            //return db.SingleById<T>(Id);
            return db.Single<T>(p => p.Id == Id);
        }
        /// <summary>
        /// SingleByIdAsync 有坑。。ID不是根据主键特性匹配的，直接用了实体类第一个属性
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public virtual async Task<T> QueryByIdAsync(int Id)
        {
            using var db = await _ormLiteConnection.OpenAsync();
            var data = await db.SingleAsync<T>(p => p.Id == Id);
            Console.WriteLine(db.GetLastSql());
            return data;
        }

        public virtual T QuerySingleBySql(string sql)
        {
            if (!string.IsNullOrWhiteSpace(sql))
                return default;
            using var db = _ormLiteConnection.Open();
            return db.SqlScalar<T>(sql);
        }

        public async Task<T> QuerySingleBySqlAsync(string sql)
        {
            if (!string.IsNullOrWhiteSpace(sql))
                return default;
            using var db = await _ormLiteConnection.OpenAsync();
            return await db.SqlScalarAsync<T>(sql);
        }

        public virtual async Task<T> QuerySingleAsync(Expression<Func<T, bool>> Where = null)
        {
            using var db = await _ormLiteConnection.OpenAsync();
            //return db.SingleById<T>(Id);
            return await db.SingleAsync<T>(Where);
        }

        public virtual IEnumerable<T> QueryListBySql(string sql)
        {
            using var db = _ormLiteConnection.Open();

            return db.Select<T>(sql);
        }
        public virtual async Task<IEnumerable<T>> QueryListBySqlAsync(string sql)
        {
            using var db = await _ormLiteConnection.OpenAsync();
            return await db.SqlListAsync<T>(sql);
        }
        public virtual PageData<T> QueryPage(PageInfo page, Expression<Func<T, bool>> Where, Order order = Order.Descending, Expression<Func<T, object>> OrderBy = null)
        {
            if (Where == null)
                Where = p => true;
            using var db = _ormLiteConnection.Open();
            var expressionFilter = db.From<T>().Where(Where);
            if (OrderBy != null)
                expressionFilter = order == Order.Descending ? expressionFilter.OrderByDescending(OrderBy) : expressionFilter.OrderBy(OrderBy);
            else
                expressionFilter = order == Order.Descending ? expressionFilter.OrderByDescending("Id") : expressionFilter.OrderBy("Id");

            expressionFilter = expressionFilter.Limit((page.PageIndex - 1) * page.PageSize, page.PageSize);
            PageData<T> pageData = new PageData<T>
            {
                Total = db.Count(Where),
                Rows = db.Select(expressionFilter)
            };
            return pageData;
        }

        public virtual async Task<PageData<T>> QueryPageAsync(PageInfo page, Expression<Func<T, bool>> Where, Order order = Order.Descending, Expression<Func<T, object>> OrderBy = null)
        {
            var s = OrmLiteConfig.DialectProvider.SqlExpression<T>();
            if (Where == null)
                Where = p => true;
            using var db = await _ormLiteConnection.OpenAsync();
            var expressionFilter = s.Where(Where);
            if (OrderBy != null)
                expressionFilter = order == Order.Descending ? expressionFilter.OrderByDescending(OrderBy) : expressionFilter.OrderBy(OrderBy);
            else
                expressionFilter = order == Order.Descending ? expressionFilter.OrderByDescending("Id") : expressionFilter.OrderBy("Id");

            expressionFilter = expressionFilter.Limit((page.PageIndex - 1) * page.PageSize, page.PageSize);

            PageData<T> pageData = new PageData<T>
            {
                Total = await db.CountAsync(Where),
                Rows = await db.SelectAsync(expressionFilter)
            };
            //Console.WriteLine(db.GetLastSql());
            return pageData;

        }


        public virtual bool Update(T t)
        {
            using var db = _ormLiteConnection.Open();
            return db.Update(t) > 0;
        }

        public virtual async Task<bool> UpdateAsync(T t)
        {
            using var db = await _ormLiteConnection.OpenAsync();
            return await db.UpdateAsync(t) > 0;
        }

        public virtual bool UpdateMuch(IEnumerable<T> list)
        {
            using var db = _ormLiteConnection.Open();
            return db.UpdateAll(list) > 0;
        }

        public virtual async Task<bool> UpdateMuchAsync(IEnumerable<T> list)
        {
            using var db = await _ormLiteConnection.OpenAsync();
            return await db.UpdateAllAsync(list) > 0;
        }



        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <param name="funcs"></param>
        /// <returns></returns>
        public bool ExecuteActions(params Action[] actions)
        {
            if (actions.Any())
            {
                using var db = _ormLiteConnection.Open();
                using IDbTransaction trans = db.OpenTransaction(IsolationLevel.ReadCommitted);
                foreach (var action in actions)
                {
                    try
                    {
                        action.Invoke();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                        trans.Rollback();
                        return false;
                    }
                }
                return true;
            }
            return false;
        }

        public virtual bool ExecuteSqlWithTransactions(params string[] sqls)
        {
            if (!sqls.Any())
                return false;
            //Direct access to System.Data.Transactions:
            using var db = _ormLiteConnection.Open();
            using IDbTransaction trans = db.OpenTransaction(IsolationLevel.ReadCommitted);
            foreach (var sql in sqls)
            {
                try
                {
                    db.ExecuteSql(sql);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    trans.Rollback();
                    return false;
                }
            }
            trans.Commit();
            return true;
        }
    }
}
