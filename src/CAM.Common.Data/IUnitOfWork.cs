using System;

namespace CAM.Common.Data
{
    public interface IUnitOfWork : IDisposable
    {
        void addTrans(object entity, IRepositoryForUnitOfWork repository);
        void updateTrans(object entity, IRepositoryForUnitOfWork repository);
        void deleteTrans(object entity, IRepositoryForUnitOfWork repository);
        void executeSqlCommandTrans(string SQLCommand, IRepositoryForUnitOfWork repository);

        /// <summary>
        /// 执行unitofwork单元内的所有操作命令
        /// </summary>
        bool commit();

        void clearWork();
    }
}
