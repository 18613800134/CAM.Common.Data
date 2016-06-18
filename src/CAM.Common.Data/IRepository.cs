using System;
using System.Linq;
using System.Linq.Expressions;
using System.Data.Entity.Infrastructure;

namespace CAM.Common.Data
{
    public interface IRepositoryForUnitOfWork
    {
        /// <summary>
        /// 执行新增命令
        /// </summary>
        /// <param name="entity">Entity Framework 实体对象</param>
        void executeAdd(object entity);

        /// <summary>
        /// 执行修改命令
        /// </summary>
        /// <param name="entity">Entity Framework 实体对象</param>
        /// <returns></returns>
        void executeUpdate(object entity);

        /// <summary>
        /// 执行删除命令
        /// </summary>
        /// <param name="entity">Entity Framework 实体对象</param>
        /// <returns></returns>
        void executeDelete(object entity);

        void executeSQLCommand(string SQLCommand);

        DbRawSqlQuery<TType> querySQLCommand<TType>(string SQLCommand);
    }

    public interface IRepository<T> : IRepositoryForUnitOfWork
    {
        /// <summary>
        /// 新增数据（包含在事务中）
        /// </summary>
        /// <param name="entity">数据实体类</param>
        /// <returns></returns>
        void add(T entity);
        /// <summary>
        /// 更新数据（包含在事务中）
        /// </summary>
        /// <param name="entity">数据实体类</param>
        void update(T entity);
        /// <summary>
        /// 删除数据：逻辑删除，通过删除标记位实现（包含在事务中）
        /// </summary>
        /// <param name="entity">数据实体类</param>
        void delete(T entity);
        /// <summary>
        /// 删除数据：逻辑删除，通过删除标记位实现（包含在事务中）
        /// </summary>
        /// <param name="IdList">数据索引集合：逗号间隔多个索引</param>
        void delete(Type ObjectClass, string IdList);
        /// <summary>
        /// 恢复数据：恢复被逻辑删除的数据
        /// </summary>
        /// <param name="ObjectClass"></param>
        /// <param name="IdList">数据索引集合：逗号间隔多个索引</param>
        void recover(Type ObjectClass, string IdList);
        /// <summary>
        /// 执行SQL语句（包含在事务中）
        /// </summary>
        /// <param name="SQLCommand">SQL语句</param>
        void executeSQL(string SQLCommand);
        /// <summary>
        /// 判断指定条件的数据是否存在
        /// </summary>
        /// <param name="whereLambda">查询条件</param>
        /// <returns></returns>
        bool exists(Expression<Func<T, bool>> whereLambda);
        /// <summary>
        /// 获得指定的数据实体
        /// </summary>
        /// <param name="whereLambda">条件参数</param>
        /// <returns></returns>
        T read(Expression<Func<T, bool>> whereLambda);
        /// <summary>
        /// 获得符合指定条件的数据条数
        /// </summary>
        /// <param name="whereLambda">条件参数</param>
        /// <returns></returns>
        int count(Expression<Func<T, bool>> whereLambda);
        /// <summary>
        /// 获取符合条件的数据集合
        /// </summary>
        /// <param name="whereLamdba">条件参数</param>
        /// <param name="orderName">排序列名</param>
        /// <param name="isAsc">排序方式</param>
        /// <returns></returns>
        IQueryable<T> readList(Expression<Func<T, bool>> whereLamdba, string orderName, bool isAsc);
        /// <summary>
        /// 获取符合条件的数据集合：实现多个排序定义
        /// </summary>
        /// <param name="whereLamdba">条件参数</param>
        /// <param name="orderName">排序列名</param>
        /// <param name="isAsc">排序方式</param>
        /// <returns></returns>
        IQueryable<T> readList(Expression<Func<T, bool>> whereLamdba, string[] orderName, bool[] isAsc);
        /// <summary>
        /// 获取符合条件的数据带分页集合
        /// </summary>
        /// <param name="pageIndex">获取页码</param>
        /// <param name="pageSize">页面尺寸</param>
        /// <param name="totalRecord">返回：符合条件的数据条数</param>
        /// <param name="pageCount">返回：符合条件的数据的页面数量</param>
        /// <param name="whereLamdba">条件参数</param>
        /// <param name="orderName">排序列名</param>
        /// <param name="isAsc">排序方式</param>
        /// <returns></returns>
        IQueryable<T> readPageList(int pageIndex, int pageSize, out int totalRecord, out int pageCount, Expression<Func<T, bool>> whereLamdba, string orderName, bool isAsc);
        /// <summary>
        /// 获取符合条件的数据带分页集合：实现多个排序定义
        /// </summary>
        /// <param name="pageIndex">获取页码</param>
        /// <param name="pageSize">页面尺寸</param>
        /// <param name="totalRecord">返回：符合条件的数据条数</param>
        /// <param name="pageCount">返回：符合条件的数据的页面数量</param>
        /// <param name="whereLamdba">条件参数</param>
        /// <param name="orderName">排序列名</param>
        /// <param name="isAsc">排序方式</param>
        /// <returns></returns>
        IQueryable<T> readPageList(int pageIndex, int pageSize, out int totalRecord, out int pageCount, Expression<Func<T, bool>> whereLamdba, string[] orderName, bool[] isAsc);
    }
}
