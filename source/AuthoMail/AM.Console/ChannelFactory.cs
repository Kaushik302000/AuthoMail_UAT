using AM.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.Console
{
    static class ChannelFactory
    {
        public static IChannel Create(string[] channelCommands)
        {
            string channelPrefix = channelCommands[0];
            string channelType = channelCommands[1];

            string channelCode;
            string[] channelPrefixData = channelPrefix.Split('~');

            bool isPrintReady = channelPrefixData[0] == "PRINTREADY" ? true : false;
            
            bool isAPI = channelPrefixData[0].StartsWith("API_");

            if (isPrintReady)
            {
                channelCode = channelPrefixData[1] + "_" + Config.Common_Environment;
            }
            else
                // Each channel is tied to one and ONLY one XML schema
                channelCode = channelPrefix + "_" + Config.Common_Environment;

            switch (channelPrefix)
            {
                case "AMC":
                    return new AMCChannel(channelCode, channelType);

                case "ALASKA":
                    return new AlaskaChannel(channelCode, channelType);

                case "FLOODINTEGRATION":
                    return new CapOneChannel(channelCode, channelType);

                case "HSBC":
                    return new HSBCChannel(channelCode, channelType);

                case "DELFUS":
                    return new DELFUSChannel(channelCode, channelType);

                case "TDBANK":
                    return new TDBankChannel(channelCode, channelType);

                // AuthoMail XML schema
                //Parameter for Print Ready Client should be like PRINTREADY|AppCode Sweep/Trigger
                //Parameter for regular Client should be like AppCode Sweep/Trigger
                default:
                    if (isPrintReady)
                        return new PrintReadyChannel(channelCode, channelType);
                    if (isAPI)
                        return new PAMSchemaChannel(channelCode, channelType);
                    else
                        return new AMSchemaChannel(channelCode, channelType);
            }
        }
    }
}
