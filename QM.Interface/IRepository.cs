using QM.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace QM.Interface
{
    public interface IRepository<T> where T : BaseModel
    {


        /// <summary>
        /// 新增一条
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        bool Insert(T t);
        /// <summary>
        /// 异步新增一条
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        Task<bool> InsertAsync(T t);
        /// <summary>
        /// 批量新增
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        void InsertMuch(IEnumerable<T> list);
        /// <summary>
        /// 异步新增
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        Task InsertMuchAsync(IEnumerable<T> list);

        /// <summary>
        /// 根据条件删除数据
        /// </summary>
        /// <param name="Where"></param>
        /// <returns></returns>
        bool DeleteBy(Expression<Func<T, bool>> Where = null);
        /// <summary>
        /// 异步根据条件删除数据
        /// </summary>
        /// <param name="Where"></param>
        /// <returns></returns>
        Task<bool> DeleteByAsync(Expression<Func<T, bool>> Where = null);
        /// <summary>
        /// 更新一条数据
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        bool Update(T t);
        /// <summary>
        /// 异步更新一条数据
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        Task<bool> UpdateAsync(T t);
        /// <summary>
        /// 批量更新数据
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        bool UpdateMuch(IEnumerable<T> list);
        /// <summary>
        /// 异步批量更新数据
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        Task<bool> UpdateMuchAsync(IEnumerable<T> list);

        /// <summary>
        /// 根据主键ID查询
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        T QueryById(int Id);
        /// <summary>
        /// 异步根据主键查询
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name=""></param>
        /// <param name=""></param>
        /// <returns></returns>
        Task<T> QueryByIdAsync(int Id);
        /// <summary>
        /// 根据条件查询数据集合
        /// </summary>
        /// <param name="Where"></param>
        /// <param name="OrderBy"></param>
        /// <param name="GroupBy"></param>
        /// <returns></returns>
        IEnumerable<T> Query(Expression<Func<T, bool>> Where = null, Order order = Order.Descending, Expression<Func<T, object>> OrderBy = null);
        /// <summary>
        /// 根据条件异步查询数据集合
        /// </summary>
        /// <param name="Where"></param>
        /// <param name="OrderBy"></param>
        /// <param name="GroupBy"></param>
        /// <returns></returns>
        Task<IEnumerable<T>> QueryAsync(Expression<Func<T, bool>> Where = null, Order order = Order.Descending, Expression<Func<T, object>> OrderBy = null);
        /// <summary>
        /// 根据条件分页查询数据集合
        /// </summary>
        /// <param name="page"></param>
        /// <param name="Where"></param>
        /// <param name="OrderBy"></param>
        /// <param name="GroupBy"></param>
        /// <returns></returns>
        Task<PageData<T>> QueryPageAsync(PageInfo page, Expression<Func<T, bool>> Where = null, Order order = Order.Descending, Expression<Func<T, object>> OrderBy = null);
        /// <summary>
        /// 异步根据条件分页查询数据集合
        /// </summary>
        /// <param name="page"></param>
        /// <param name="Where"></param>
        /// <param name="OrderBy"></param>
        /// <param name="GroupBy"></param>
        /// <returns></returns>
        PageData<T> QueryPage(PageInfo page, Expression<Func<T, bool>> Where = null, Order order = Order.Descending, Expression<Func<T, object>> OrderBy = null);
        /// <summary>
        /// 根据条件查询数量
        /// </summary>
        /// <param name="Where"></param>
        /// <returns></returns>
        long Count(Expression<Func<T, bool>> Where = null);
        /// <summary>
        /// 异步根据条件查询数量
        /// </summary>
        /// <param name="Where"></param>
        /// <returns></returns>
        Task<long> CountAsync(Expression<Func<T, bool>> Where = null);

        /// <summary>
        /// 执行SQL查询
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        IEnumerable<T> QueryListBySql(string sql);

        /// <summary>
        /// 执行SQL查询
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        Task<IEnumerable<T>> QueryListBySqlAsync(string sql);
        /// <summary>
        /// 执行SQL查询
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        Task<bool> ExecuteSqlAsync(string sql);
        /// <summary>
        /// 执行SQL查询
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        bool ExecuteSql(string sql);
        /// <summary>
        /// 执行SQL查询
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        T QuerySingleBySql(string sql);
        /// <summary>
        /// 执行SQL查询
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        Task<T> QuerySingleBySqlAsync(string sql);

        /// <summary>
        /// 将SQL丢到事务里面执行
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        bool ExecuteSqlWithTransactions(params string[] sql);


        /// <summary>
        /// 执行SQL查询
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        T QuerySingle(Expression<Func<T, bool>> Where = null);
        /// <summary>
        /// 执行SQL查询
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        Task<T> QuerySingleAsync(Expression<Func<T, bool>> Where = null);








        bool ExecuteActions(params Action[] actions);
    }
}
