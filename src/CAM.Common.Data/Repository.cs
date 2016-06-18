using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Transactions;

namespace CAM.Common.Data
{
    public class Repository<T> : IRepository<T>
        where T : class
    {
        private DbContext _dbContext = null;
        private IUnitOfWork _unitOfWork = null;

        /// <summary>
        /// 实例化数据仓储对象
        /// </summary>
        /// <param name="dbContext">注入一个数据对象上下文 DBContext</param>
        /// <param name="unitOfWork">注入一个数据执行单元 确保一批操作包含在同一个数据库事务里面</param>
        public Repository(DbContext dbContext, IUnitOfWork unitOfWork)
        {
            _dbContext = dbContext;
            _unitOfWork = unitOfWork;

        }


        public void add(T entity)
        {
            _unitOfWork.addTrans(entity, this);
        }

        public void update(T entity)
        {
            _unitOfWork.updateTrans(entity, this);
        }

        public void delete(T entity)
        {
            _unitOfWork.deleteTrans(entity, this);
        }

        public void delete(Type ObjectClass, string IdList)
        {
            //IdList = IdList.Replace(",", "','");
            //IdList = string.Format("'{0}'", IdList);
            //string SQLCommand = string.Format("delete from {0} where Id in({1})", ObjectClass.Name, IdList);
            //_unitOfWork.ExecuteCommand(SQLCommand, this);

            IdList = IdList.Replace(",", "','");
            IdList = string.Format("'{0}'", IdList);
            string SQLCommand = string.Format("update {0} set System_DeleteFlag=1, System_DeleteTime=GetDate() where Id in({1})",
                                                ObjectClass.Name, IdList);

            _unitOfWork.executeSqlCommandTrans(SQLCommand, this);
        }

        public void recover(Type ObjectClass, string IdList)
        {
            IdList = IdList.Replace(",", "','");
            IdList = string.Format("'{0}'", IdList);
            string SQLCommand = string.Format("update {0} set System_DeleteFlag=0 where Id in({1})",
                                                ObjectClass.Name, IdList);

            _unitOfWork.executeSqlCommandTrans(SQLCommand, this);
        }

        public void executeSQL(string SQLCommand)
        {
            _unitOfWork.executeSqlCommandTrans(SQLCommand, this);
        }

        public bool exists(Expression<Func<T, bool>> whereLambda)
        {
            return _dbContext.Set<T>().Any(whereLambda);
        }

        public T read(Expression<Func<T, bool>> whereLambda)
        {
            T _entity = _dbContext.Set<T>().FirstOrDefault<T>(whereLambda);
            return _entity;
        }

        public int count(Expression<Func<T, bool>> whereLambda)
        {
            return _dbContext.Set<T>().Count(whereLambda);
        }

        public IQueryable<T> readList(Expression<Func<T, bool>> whereLamdba, string orderName, bool isAsc)
        {
            return readList(whereLamdba, new string[] { orderName }, new bool[] { isAsc });
        }

        public IQueryable<T> readList(Expression<Func<T, bool>> whereLamdba, string[] orderName, bool[] isAsc)
        {
            var _list = _dbContext.Set<T>().Where<T>(whereLamdba);
            _list = orderBy(_list, orderName, isAsc);
            return _list;
        }

        public IQueryable<T> readPageList(int pageIndex, int pageSize, out int totalRecord, out int pageCount, Expression<Func<T, bool>> whereLamdba, string orderName, bool isAsc)
        {
            return readPageList(pageIndex, pageSize, out totalRecord, out pageCount, whereLamdba, new string[] { orderName }, new bool[] { isAsc });
        }

        public IQueryable<T> readPageList(int pageIndex, int pageSize, out int totalRecord, out int pageCount, Expression<Func<T, bool>> whereLamdba, string[] orderName, bool[] isAsc)
        {
            var _list = _dbContext.Set<T>().Where<T>(whereLamdba);

            totalRecord = _list.Count();
            if (pageSize == 0)
            {
                pageCount = 0;
            }
            else
            {
                pageCount = (int)Math.Ceiling((double)totalRecord / (double)pageSize);
            }

            //if (pageIndex < 1)
            //{
            //    pageIndex = 1;
            //}
            //if (pageIndex > pageCount)
            //{
            //    pageIndex = pageCount;
            //}
            _list = orderBy(_list, orderName, isAsc).Skip<T>((pageIndex - 1) * pageSize).Take<T>(pageSize);

            return _list;
        }

        private IQueryable<T> orderBy(IQueryable<T> source, string[] propertyName, bool[] isAsc)
        {
            if (source == null)
            {
                throw new Exception("未指定OrderBy的参数");
            }
            int orderCount = propertyName.Length;
            IQueryable<T> query = source;
            ParameterExpression _parameter = Expression.Parameter(source.ElementType);

            for (int i = 0; i < orderCount; i++)
            {

                if (string.IsNullOrEmpty(propertyName[i])) return source;

                //var _property = Expression.Property(_parameter, propertyName[i]);
                MemberExpression _property = null;
                string[] propertyName_Split = propertyName[i].Split('_');
                for (int j = 0; j < propertyName_Split.Length; j++)
                {
                    if (j == 0)
                    {
                        _property = Expression.PropertyOrField(_parameter, propertyName_Split[j]);
                    }
                    else
                    {
                        _property = Expression.PropertyOrField(_property, propertyName_Split[j]);
                    }
                }
                //if (propertyName[i] == "Order_Index")
                //{
                //    MemberExpression b = Expression.PropertyOrField(_parameter,"Order");
                //    _property = Expression.Property(b, "Index");

                //}
                //else
                //{
                //    _property = Expression.Property(_parameter, propertyName[i]);
                //}
                if (_property == null)
                {
                    throw new Exception("指定的OrderBy字段不存在");
                }
                var _lambda = Expression.Lambda(_property, _parameter);
                var _methodName = (i == 0) ? isAsc[i] ? "OrderBy" : "OrderByDescending" : isAsc[i] ? "ThenBy" : "ThenByDescending";

                var _resultExpression = Expression.Call(typeof(Queryable), _methodName, new Type[] { query.ElementType, _property.Type }, query.Expression, Expression.Quote(_lambda));
                query = query.Provider.CreateQuery<T>(_resultExpression);
            }

            return query;
        }


        public void executeAdd(object entity)
        {
            _dbContext.Entry<T>((T)entity).State = System.Data.Entity.EntityState.Added;
            _dbContext.SaveChanges();
        }

        public void executeUpdate(object entity)
        {
            _dbContext.Set<T>().Attach((T)entity);
            _dbContext.Entry<T>((T)entity).State = System.Data.Entity.EntityState.Modified;
            _dbContext.SaveChanges();
        }

        public void executeDelete(object entity)
        {
            _dbContext.Set<T>().Attach((T)entity);
            _dbContext.Entry<T>((T)entity).State = System.Data.Entity.EntityState.Deleted;
            _dbContext.SaveChanges();
        }

        public void executeSQLCommand(string SQLCommand)
        {
            _dbContext.Database.ExecuteSqlCommand(SQLCommand);

        }



        public DbRawSqlQuery<TType> querySQLCommand<TType>(string SQLCommand)
        {
            return _dbContext.Database.SqlQuery<TType>(SQLCommand);
        }
    }
}
