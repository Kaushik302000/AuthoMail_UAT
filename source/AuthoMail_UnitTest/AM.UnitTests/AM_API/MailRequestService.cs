using AM.Entities;
using Refit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.UnitTests
{
    public interface IMailRequestService
    {
        [Post("/api/mailrequest")]
        [Headers("Authorization: Bearer")]
        Task<MailAPIResponse> CreateMailRequest(MailPiece request);
        [Get("/api/mailresponse")]
        [Headers("Authorization: Bearer")]
        Task<List<MailAPIResponse>> MailAPIResponse(List<string> mailPieceIds);
    }
}
