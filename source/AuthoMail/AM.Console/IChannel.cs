using AM.Entities;
using System;

namespace AM.Console
{
    interface IChannel : IDisposable
    {
        SFTChannel SFTChannel { get; }

        void ProcessBatches();
    }
}