using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.Services
{
    public interface IDemo : IDisposable
    {
        List<string> GetData();
    }
}
