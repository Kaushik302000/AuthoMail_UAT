using AM.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.EDelivery
{
    static class ChannelFactory
    {
        public static EDChannel Create()
        {
            return new EDGeneralChannel();
        }
    }
}
