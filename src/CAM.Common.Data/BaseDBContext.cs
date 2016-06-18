

namespace CAM.Common.Data
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.ModelConfiguration.Conventions;
    using Migrations;

    public class BaseDBContext<T> : DbContext, IDisposable
       where T : DbContext
    {
        public BaseDBContext()
            : base("name=DBConnectionString")
        {

            Database.SetInitializer<T>(new MigrateDatabaseToLatestVersion<T, Configuration<T>>());
            //Database.SetInitializer<BaseDBContext>(new MigrateDatabaseToLatestVersion<BaseDBContext, Configuration>());
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>(); //表名为类名，不是上面带s的名字  //移除复数表名的契约
            //modelBuilder.Conventions.Remove<IncludeMetadataConvention>();     //不创建EdmMetadata表  //防止黑幕交易 要不然每次都要访问 EdmMetadata这个表
        }


    }
}
