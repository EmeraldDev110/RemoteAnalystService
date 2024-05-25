using NHibernate;
using NHibernate.Context;
using NHibernate.SqlCommand;
using RemoteAnalyst.Repository.Domain;
using RemoteAnalyst.Repository.Models;
using RemoteAnalyst.Repository.Repositories;
namespace RemoteAnalyst.Repository.Interceptors
{
    internal class DynamicTableInterceptor : EmptyInterceptor
    {
        /*public override SqlString OnPrepareStatement(SqlString sql)
        {
            // Assuming Date is part of your entity and determines the table to be used
            if (NHibernateHelper.get is QNMTCPPacketsDetail packetDetail)
            {
                string tableName = $"dbo.qnm_tcppacketsdetail_{packetDetail.DateTime:yyyy_M_dd}";
                sql = sql.Replace("QNMTCPPacketsDetail", tableName);
            }
            return base.OnPrepareStatement(sql);
        }*/
        
           /* public override SqlString OnPrepareStatement(SqlString sql)
            {
                var sessionImpl = CurrentSessionContext.Unbind(NHibernateHelper.SessionFactory);
                if (sessionImpl != null)
                {
                    foreach (var entry in sessionImpl.PersistenceContext.EntityEntries)
                    {
                        var entity = entry.Key;
                        if (entity is ITableNameProvider tableProvider)
                        {
                            var tableName = tableProvider.GetTableName();
                            sql = sql.Replace("TcpPacketDetail", tableName);
                        }
                    }
                }

                return sql;
            }*/
        
    }
}
