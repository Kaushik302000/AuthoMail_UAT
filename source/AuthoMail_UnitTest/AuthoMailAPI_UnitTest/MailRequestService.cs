using Refit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthoMailAPI_UnitTest
{
    public interface IMailRequestService
    {
        [Post("/api/mailrequest")]
        [Headers("Authorization: Bearer")]
        Task<MailAPIResponse> CreateMailRequest(MailPiece request);
        [Get("/api/mailresponse")]
        [Headers("Authorization: Bearer")]
        Task<List<MailAPIResponse>> MailAPIResponse(string mailPieceIds);
        [Post("/api/emresponse")]
        [Headers("Authorization: Bearer")]
        Task<string> SendResponse(MailAPIResponse response);
    }
}
