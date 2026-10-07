using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.EnterpriseLibrary.Data;

namespace PCI.Service
{
    partial class DAL
    {
        public static List<Client> GetAllClientAppDetail()
        {
            var accessor = db.CreateSprocAccessor<Client>("usp_GetAllClientAppDetail");
            return accessor.Execute().ToList<Client>();
        }

        public static int DeleteInCompleteBatchById(int batchId)
        {            
            return db.ExecuteSqlNonQuery("DELETE FROM [PnS].[BatchSummary] WHERE PrintBatch=@p1",batchId);
        }
        
    }
}
