using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.Services
{
    public class Demo : ServiceBase, IDemo
    {
        public List<string> GetData()
        {
            var result = db.Query("select * from AspNetUsers");
            return result.Select(row => (string)row.UserName).ToList();
        }
    }
}
