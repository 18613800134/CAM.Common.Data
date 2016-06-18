using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Transactions;

namespace CAM.Common.Data
{
    public class UnitOfWork : IUnitOfWork
    {

        /*
        定义4个操作容器，记录当前事务期间的所有数据操作命令，当Commit时一起提交到数据库执行
        */
        private Dictionary<object, IRepositoryForUnitOfWork> _addDicts;
        private Dictionary<object, IRepositoryForUnitOfWork> _updateDicts;
        private Dictionary<object, IRepositoryForUnitOfWork> _deleteDicts;
        private Dictionary<object, IRepositoryForUnitOfWork> _sqlCommandDicts;

        public UnitOfWork()
        {
            _addDicts = new Dictionary<object, IRepositoryForUnitOfWork>();
            _updateDicts = new Dictionary<object, IRepositoryForUnitOfWork>();
            _deleteDicts = new Dictionary<object, IRepositoryForUnitOfWork>();
            _sqlCommandDicts = new Dictionary<object, IRepositoryForUnitOfWork>();
        }

        public void addTrans(object entity, IRepositoryForUnitOfWork repository)
        {
            if (!this._addDicts.ContainsKey(entity))
            {
                this._addDicts.Add(entity, repository);
            }
        }

        public void updateTrans(object entity, IRepositoryForUnitOfWork repository)
        {
            if (!this._updateDicts.ContainsKey(entity))
            {
                this._updateDicts.Add(entity, repository);
            }
        }

        public void deleteTrans(object entity, IRepositoryForUnitOfWork repository)
        {
            if (!this._deleteDicts.ContainsKey(entity))
            {
                this._deleteDicts.Add(entity, repository);
            }
        }

        public void executeSqlCommandTrans(string SQLCommand, IRepositoryForUnitOfWork repository)
        {
            if (!this._sqlCommandDicts.ContainsKey(SQLCommand))
            {
                this._sqlCommandDicts.Add(SQLCommand, repository);
            }
        }

        public bool commit()
        {
            bool result = false;
            using (TransactionScope scope = new TransactionScope())
            {
                try
                {
                    foreach (object entity in this._addDicts.Keys)
                    {
                        this._addDicts[entity].executeAdd(entity);
                    }
                    foreach (object entity in this._updateDicts.Keys)
                    {
                        this._updateDicts[entity].executeUpdate(entity);
                    }
                    foreach (object entity in this._deleteDicts.Keys)
                    {
                        this._deleteDicts[entity].executeDelete(entity);
                    }
                    foreach (string sqlCommand in this._sqlCommandDicts.Keys)
                    {
                        this._sqlCommandDicts[sqlCommand].executeSQLCommand(sqlCommand);
                    }
                    scope.Complete();
                    result = true;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    scope.Dispose();
                }
            }
            this.clearWork();
            return result;
        }

        public void clearWork()
        {
            this._addDicts.Clear();
            this._updateDicts.Clear();
            this._deleteDicts.Clear();
            this._sqlCommandDicts.Clear();
        }

        public void Dispose()
        {
            this.clearWork();
            this._addDicts = null;
            this._updateDicts = null;
            this._deleteDicts = null;
            this._sqlCommandDicts = null;
        }
    }
}
