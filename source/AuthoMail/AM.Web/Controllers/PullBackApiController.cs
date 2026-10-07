using AM.Services;
using AM.Web.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Web.Http;
using Microsoft.AspNet.Identity;
using AM.Entities;
using System.Web.Hosting;
using System.Threading;
using System.Web.Http.Cors;
using System.Threading.Tasks;

namespace AM.Web.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    [Authorize(Roles = AMRoles.Admin + "," + AMRoles.BA + "," + AMRoles.Client + "," + AMRoles.ClientUserManager + "," + AMRoles.CoreLogicUserManager + "," + AMRoles.PnSOperator + "," + AMRoles.API + "," + AMRoles.BAOffShore)]
    [RoutePrefix("api")]
    public class PullBackApiController : ApiController
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        IPullBackService pullBackSvc;
        IUSPSService uspsSvc;
        public PullBackApiController(IPullBackService pullBackSvc, IUSPSService uspsSvc)
        {
            this.pullBackSvc = pullBackSvc;
            this.uspsSvc = uspsSvc;
        }
        [Route("GetDateTime")]
        [HttpPost]
        public object GetDateTime()
        {
            return DateTime.Now;
        }

        [Route("RequestPullBack")]
        [HttpPost]
        public async Task<List<PullBackResponse>> GeneratePullBackRequest([FromBody] PullBackRequestInfo pullBackInfo)
        {
            var pullBackReason = pullBackInfo.PBReason;
            var mailPieceIds = pullBackInfo.MailPieceIds;

            if (mailPieceIds == null || mailPieceIds.Count == 0) throw new HttpResponseException(HttpStatusCode.BadRequest);

            try
            {
                USPSApiController uSPSApi = new USPSApiController(uspsSvc);
                //Dictionary<string, string> mailPieceResponse = new Dictionary<string, string>();
                List<PullBackResponse> mailPieceResponse = new List<PullBackResponse>();
                List<string> mailPieceIdsToPullback = new List<string>();

                //Check if mailpiece id is in database or not
                foreach (var mailPieceId in mailPieceIds)
                {
                    using (ICommonService commonSvc = new CommonService())
                    {
                        if (!commonSvc.CheckForDuplicateMailPieceUniqueId(Guid.Parse(mailPieceId)))
                        {
                            mailPieceResponse.Add(new PullBackResponse { MailPieceId = mailPieceId, MailingStatus = ShippingStatus.NotFound.ToString(), Message = Config.Web_Msg_API_PB_BatchNotValidated });
                        }
                    }
                }

                var mailPieceData = pullBackSvc.GetMailPieceData(mailPieceIds);

                if (mailPieceData == null || mailPieceData.Count == 0)
                {
                    mailPieceResponse.Add(new PullBackResponse { Message = Config.Web_Msg_API_PB_DataNull, MailingStatus = ShippingStatus.NotFound.ToString() });
                    return mailPieceResponse;
                }
                foreach (var item in mailPieceData)
                {
                    USPSLib.eVSCancelLabelRequest eVSCancelLabelRequest = new USPSLib.eVSCancelLabelRequest();
                    if (!string.IsNullOrEmpty(item.TrackingNo))
                    {
                        eVSCancelLabelRequest.BarcodeNumber = item.TrackingNo;
                    }
                    if (item.AMMailPieceStatus == "PulledBack" || item.PnSMailPieceStatus == "PulledBack")
                    {
                        mailPieceResponse.Add(new PullBackResponse { MailPieceId = item.MailPieceUniqueId.ToString(), MailingStatus = ShippingStatus.PulledBack.ToString(), Message = Config.Web_Msg_API_PB_PulledBack, PullBackRequestedOn = item.PullBackRequestedOn, PulledBackRequestedBy = item.UserName, Success = false });
                    }
                    else if (Convert.ToBoolean(item.IsPullBackRequested))
                    {
                        mailPieceResponse.Add(new PullBackResponse { MailPieceId = item.MailPieceUniqueId.ToString(), MailingStatus = item.AMMailPieceStatus, Message = Config.Web_Msg_API_PB_PullBackAlreadyRequested, PullBackRequestedOn = item.PullBackRequestedOn, PulledBackRequestedBy = item.UserName, Success = false });
                    }
                    else if (item.AMBatchPipelineStatus == "Received" || item.AMBatchPipelineStatus == "Downloaded")
                    {
                        mailPieceResponse.Add(new PullBackResponse { MailPieceId = item.MailPieceUniqueId.ToString(), MailingStatus = item.AMMailPieceStatus, Message = Config.Web_Msg_API_PB_BatchNotValidated, Success = false });
                    }
                    else if (item.AMMailPieceStatus == "Aborted" || item.PnSMailPieceStatus == "Aborted")
                    {
                        mailPieceResponse.Add(new PullBackResponse { MailPieceId = item.MailPieceUniqueId.ToString(), MailingStatus = item.AMMailPieceStatus, Message = Config.Web_Msg_API_PB_Aborted, Success = false });
                    }
                    else if (item.AMMailPieceStatus == "Error" || item.PnSMailPieceStatus == "Error")
                    {
                        mailPieceResponse.Add(new PullBackResponse { MailPieceId = item.MailPieceUniqueId.ToString(), MailingStatus = ShippingStatus.Error.ToString(), Message = Config.Web_Msg_API_PB_Error, ErrorDescription = !string.IsNullOrEmpty(item.PnSErrorDescription) ? item.PnSErrorDescription.ToString() : item.AMErrorDescription.ToString(), Success = false });
                    }
                    else if (item.AMMailPieceStatus == "Shipped" || item.PnSMailPieceStatus == "Shipped")
                    {
                        mailPieceResponse.Add(new PullBackResponse { MailPieceId = item.MailPieceUniqueId.ToString(), MailingStatus = ShippingStatus.Shipped.ToString(), Message = Config.Web_Msg_API_PB_Shipped, Success = false });
                    }
                    else if (item.AMMailPieceStatus == "eDelivered")
                    {
                        mailPieceResponse.Add(new PullBackResponse { MailPieceId = item.MailPieceUniqueId.ToString(), MailingStatus = item.AMMailPieceStatus, Message = Config.Web_Msg_API_PB_EDelivered, Success = false });
                    }
                    else if (item.AMMailPieceStatus == "InProgress" && (item.AMBatchPipelineStatus == "Validated" || item.AMBatchPipelineStatus == "Acknowledged" || item.AMBatchPipelineStatus == "Held" || item.AMBatchPipelineStatus == "Prepped" || item.AMBatchPipelineStatus == "Suspended"))
                    {
                        int row1 = pullBackSvc.InsertPullBackRequestData(item.MailPieceUniqueId.ToString(), User.Identity.GetClientId().Value, "PulledBack", string.IsNullOrEmpty(User.Identity.GetUserId().ToString()) ? string.Empty : User.Identity.GetUserId().ToString(), pullBackReason);
                        int row = pullBackSvc.UpdatePullBackRequestedOn(item.MailPieceUniqueId.ToString(), string.IsNullOrEmpty(User.Identity.GetUserId().ToString()) ? string.Empty : User.Identity.GetUserId().ToString(), "PulledBack");

                        if (!string.IsNullOrEmpty(eVSCancelLabelRequest.BarcodeNumber) && (item.ShippingServiceType == "PRIORITY" || item.ShippingServiceType == "PRIORITY_EXPRESS"))
                        {
                            var uspsCancelResponse = uSPSApi.RequesteVSLabel(eVSCancelLabelRequest);
                            int updatedRow = pullBackSvc.UpdateUSPSApiCancelErrorDesc(item.MailPieceUniqueId.ToString());
                        }

                        mailPieceResponse.Add(new PullBackResponse { MailPieceId = item.MailPieceUniqueId.ToString(), MailingStatus = "PulledBack", Message = Config.Web_Msg_API_PB_PulledBackSuccess, PullBackRequestedOn = Common.Now, PulledBackRequestedBy = (User.Identity) == null ? string.Empty : User.Identity.Name, Success = true });
                    }
                    else if (item.AMMailPieceStatus == "InProgress" && item.AMBatchPipelineStatus == "SubmittedToEDelivery" && item.IsEDeliveryRequested)
                    {
                        #region If signingroom is created and user has still not downloaded/reviewed the document then delete signing room and update pullback status.
                        //if (!string.IsNullOrWhiteSpace(item.SigningRoomName))
                        //{
                        //    IEDeliveryAPIService eDeliveryAPISvc = new EDeliveryAPIService();

                        //    var activityLogs = await eDeliveryAPISvc.GetSigningRoomActivityLog(item.SigningRoomName);

                        //    foreach (var actlog in activityLogs)
                        //    {
                        //        switch (actlog.EDeliveryStatus)
                        //        {
                        //            case ActivityStatus.DocumentReviewed:
                        //                item.IsDocReviewed = true;
                        //                break;
                        //            case ActivityStatus.DocumentDownloaded:
                        //                item.IsDocDownloaded = true;
                        //                break;
                        //            default:
                        //                break;
                        //        }
                        //    }

                        //    if (!item.IsDocReviewed && !item.IsDocDownloaded)
                        //    {
                        //        var isSigningRoomDeleted = await eDeliveryAPISvc.DeleteSigningRoom(item.SigningRoomId);
                        //        if (isSigningRoomDeleted)
                        //        {
                        //            int row1 = pullBackSvc.InsertPullBackRequestData(item.MailPieceUniqueId.ToString(), User.Identity.GetClientId().Value, "PulledBack", string.IsNullOrEmpty(User.Identity.GetUserId().ToString()) ? string.Empty : User.Identity.GetUserId().ToString());
                        //            int row = pullBackSvc.UpdatePullBackRequestedOn(item.MailPieceUniqueId.ToString(), string.IsNullOrEmpty(User.Identity.GetUserId().ToString()) ? string.Empty : User.Identity.GetUserId().ToString(), "PulledBack");

                        //            mailPieceResponse.Add(new PullBackResponse { MailPieceId = item.MailPieceUniqueId.ToString(), Message = Config.Web_Msg_API_PB_PulledBackSuccess, MailPieceStatus = "PulledBack", PullBackRequestedOn = Common.Now, PulledBackRequestedBy = (User.Identity) == null ? string.Empty : User.Identity.Name, Success = true });
                        //        }
                        //    }
                        //    else
                        //    {
                        //        mailPieceResponse.Add(new PullBackResponse { MailPieceId = item.MailPieceUniqueId.ToString(), Message = "Mailpiece is already Downloaded/Reviewed by Borrower, pullback cannot be requested.", Success = false });
                        //    }
                        //}
                        #endregion
                        #region If invitation is not sent then only allow to pulledback 
                        if (!item.IsInvitationSent)
                        {
                            IEDeliveryAPIService eDeliveryAPISvc = new EDeliveryAPIService();
                            var isSigningRoomDeleted = await eDeliveryAPISvc.DeleteSigningRoom(item.SigningRoomId, item.ClientId);
                            int row1 = pullBackSvc.InsertPullBackRequestData(item.MailPieceUniqueId.ToString(), User.Identity.GetClientId().Value, "PulledBack", string.IsNullOrEmpty(User.Identity.GetUserId().ToString()) ? string.Empty : User.Identity.GetUserId().ToString(), pullBackReason);
                            int row = pullBackSvc.UpdatePullBackRequestedOn(item.MailPieceUniqueId.ToString(), string.IsNullOrEmpty(User.Identity.GetUserId().ToString()) ? string.Empty : User.Identity.GetUserId().ToString(), "PulledBack");

                            mailPieceResponse.Add(new PullBackResponse { MailPieceId = item.MailPieceUniqueId.ToString(), MailingStatus = "PulledBack", Message = Config.Web_Msg_API_PB_PulledBackSuccess, PullBackRequestedOn = Common.Now, PulledBackRequestedBy = (User.Identity) == null ? string.Empty : User.Identity.Name, Success = true });
                        }
                        else
                        {
                            mailPieceResponse.Add(new PullBackResponse { MailPieceId = item.MailPieceUniqueId.ToString(), MailingStatus = item.AMMailPieceStatus, Message = "Invitation for signing room is already sent to Borrower, pullback cannot be requested.", Success = false });
                        }
                        #endregion
                    }
                    else if (item.AMMailPieceStatus == "InProgress" && item.AMBatchPipelineStatus == "SubmittedToShipping" && !item.PCIBatchPrepEnd.HasValue)
                    {
                        int row1 = pullBackSvc.InsertPullBackRequestData(item.MailPieceUniqueId.ToString(), User.Identity.GetClientId().Value, "PulledBack", string.IsNullOrEmpty(User.Identity.GetUserId().ToString()) ? string.Empty : User.Identity.GetUserId().ToString(), pullBackReason);
                        int row = pullBackSvc.UpdatePullBackRequestedOn(item.MailPieceUniqueId.ToString(), string.IsNullOrEmpty(User.Identity.GetUserId().ToString()) ? string.Empty : User.Identity.GetUserId().ToString(), "PulledBack");

                        if (!string.IsNullOrEmpty(eVSCancelLabelRequest.BarcodeNumber) && (item.ShippingServiceType == "PRIORITY" || item.ShippingServiceType == "PRIORITY_EXPRESS"))
                        {
                            var uspsCancelResponse = uSPSApi.RequesteVSLabel(eVSCancelLabelRequest);
                            int updatedRow = pullBackSvc.UpdateUSPSApiCancelErrorDesc(item.MailPieceUniqueId.ToString());
                        }

                        mailPieceResponse.Add(new PullBackResponse { MailPieceId = item.MailPieceUniqueId.ToString(), MailingStatus = "PulledBack", Message = Config.Web_Msg_API_PB_PulledBackSuccess, PullBackRequestedOn = Common.Now, PulledBackRequestedBy = (User.Identity) == null ? string.Empty : User.Identity.Name, Success = true });
                    }
                    else if (item.AMBatchPipelineStatus == "SubmittedToShipping" && item.PCIBatchPrepEnd.HasValue)
                    {
                        if (item.PrintTrailBatchId == null)
                        {
                            int row1 = pullBackSvc.InsertPullBackRequestData(item.MailPieceUniqueId.ToString(), User.Identity.GetClientId().Value, "PulledBack", string.IsNullOrEmpty(User.Identity.GetUserId().ToString()) ? string.Empty : User.Identity.GetUserId().ToString(), pullBackReason);
                            int row = pullBackSvc.UpdatePullBackRequestedOn(item.MailPieceUniqueId.ToString(), string.IsNullOrEmpty(User.Identity.GetUserId().ToString()) ? string.Empty : User.Identity.GetUserId().ToString(), "PulledBack");

                            if (!string.IsNullOrEmpty(eVSCancelLabelRequest.BarcodeNumber) && (item.ShippingServiceType == "PRIORITY" || item.ShippingServiceType == "PRIORITY_EXPRESS"))
                            {
                                var uspsCancelResponse = uSPSApi.RequesteVSLabel(eVSCancelLabelRequest);
                                int updatedRow = pullBackSvc.UpdateUSPSApiCancelErrorDesc(item.MailPieceUniqueId.ToString());
                            }

                            mailPieceResponse.Add(new PullBackResponse { MailPieceId = item.MailPieceUniqueId.ToString(), MailingStatus = "PulledBack", Message = Config.Web_Msg_API_PB_PulledBackSuccess, PullBackRequestedOn = Common.Now, PulledBackRequestedBy = (User.Identity) == null ? string.Empty : User.Identity.Name, Success = true });
                        }
                        else if (item.PnSMailPieceStatus == "InProgress" && (item.PrintTrailBatchId != null))
                        {
                            mailPieceIdsToPullback.Add(item.MailPieceUniqueId.ToString());
                            if (!string.IsNullOrEmpty(eVSCancelLabelRequest.BarcodeNumber) && (item.ShippingServiceType == "PRIORITY" || item.ShippingServiceType == "PRIORITY_EXPRESS"))
                            {
                                var uspsCancelResponse = uSPSApi.RequesteVSLabel(eVSCancelLabelRequest);
                                int updatedRow = pullBackSvc.UpdateUSPSApiCancelErrorDesc(item.MailPieceUniqueId.ToString());
                            }
                        }
                    }
                }

                if (mailPieceIdsToPullback.Count > 0)
                {
                    var pullBackData = pullBackSvc.GetPullBackData(mailPieceIdsToPullback);

                    bool isGenerated = pullBackSvc.GeneratePullBackForm(pullBackData);
                    if (isGenerated)
                    {
                        foreach (var item in mailPieceIdsToPullback)
                        {
                            int row1 = pullBackSvc.InsertPullBackRequestData(item.ToString(), User.Identity.GetClientId().Value, "Requested", string.IsNullOrEmpty(User.Identity.GetUserId().ToString()) ? string.Empty : User.Identity.GetUserId().ToString(), pullBackReason);
                            int row = pullBackSvc.UpdatePullBackRequestedOn(item.ToString(), string.IsNullOrEmpty(User.Identity.GetUserId().ToString()) ? string.Empty : User.Identity.GetUserId().ToString(), string.Empty);
                            if (row > 0)
                            {
                                mailPieceResponse.Add(new PullBackResponse { MailPieceId = item.ToString(), MailingStatus = "InProgress", Message = Config.Web_Msg_API_PB_Success, PullBackRequestedOn = Common.Now, PulledBackRequestedBy = (User.Identity) == null ? string.Empty : User.Identity.Name, Success = true });
                            }
                        }
                    }
                    else
                    {
                        mailPieceResponse.Add(new PullBackResponse { Message = Config.Web_Msg_API_PB_PBReqFormErr });
                        return mailPieceResponse;
                    }
                }

                return mailPieceResponse;
            }
            catch (Exception ex)
            {
                string msg = string.Format("Unexpected error occured while processing your request.");
                log.Error(msg, ex);
                return new List<PullBackResponse>() { new PullBackResponse { MailingStatus = ShippingStatus.Error.ToString(), ErrorCode = OutComeCodes.Unknown_Error, ErrorDescription = msg } };
            }

        }

        [Route("pullback/pullbackold")]
        [HttpPost]
        public async Task<object> GeneratePullBackRequestOld([FromBody] PullBackRequestInfo pullBackInfo)
        {
            var pullBackReason = pullBackInfo.PBReason;
            var mailPieceIds = pullBackInfo.MailPieceIds;
            if (mailPieceIds == null || mailPieceIds.Count == 0)
            {
                return JsonResponseFactory.ErrorResponse(Config.Web_Msg_API_PB_Empty);
            }
            try
            {
                USPSApiController uSPSApi = new USPSApiController(uspsSvc);
                //Dictionary<string, string> mailPieceResponse = new Dictionary<string, string>();
                List<PullBackResponseOld> mailPieceResponse = new List<PullBackResponseOld>();
                List<string> mailPieceIdsToPullback = new List<string>();

                var mailPieceData = pullBackSvc.GetMailPieceData(mailPieceIds);

                if (mailPieceData == null || mailPieceData.Count == 0)
                {
                    return JsonResponseFactory.ErrorResponse(Config.Web_Msg_API_PB_DataNull);
                }
                foreach (var item in mailPieceData)
                {
                    USPSLib.eVSCancelLabelRequest eVSCancelLabelRequest = new USPSLib.eVSCancelLabelRequest();
                    if (!string.IsNullOrEmpty(item.TrackingNo))
                    {
                        eVSCancelLabelRequest.BarcodeNumber = item.TrackingNo;
                    }
                    if (item.AMMailPieceStatus == "PulledBack" || item.PnSMailPieceStatus == "PulledBack")
                    {
                        mailPieceResponse.Add(new PullBackResponseOld { MailPieceId = item.MailPieceUniqueId.ToString(), Message = Config.Web_Msg_API_PB_PulledBack, PullBackRequestedOn = item.PullBackRequestedOn, PulledBackRequestedBy = item.UserName, MailPieceStatus = item.AMMailPieceStatus, Success = false });
                    }
                    else if (Convert.ToBoolean(item.IsPullBackRequested))
                    {
                        mailPieceResponse.Add(new PullBackResponseOld { MailPieceId = item.MailPieceUniqueId.ToString(), Message = Config.Web_Msg_API_PB_PullBackAlreadyRequested, PullBackRequestedOn = item.PullBackRequestedOn, PulledBackRequestedBy = item.UserName, MailPieceStatus = item.AMMailPieceStatus, Success = false });
                    }
                    else if (item.AMBatchPipelineStatus == "Received" || item.AMBatchPipelineStatus == "Downloaded")
                    {
                        mailPieceResponse.Add(new PullBackResponseOld { MailPieceId = item.MailPieceUniqueId.ToString(), Message = Config.Web_Msg_API_PB_BatchNotValidated, Success = false });
                    }
                    else if (item.AMMailPieceStatus == "Aborted" || item.PnSMailPieceStatus == "Aborted")
                    {
                        mailPieceResponse.Add(new PullBackResponseOld { MailPieceId = item.MailPieceUniqueId.ToString(), Message = Config.Web_Msg_API_PB_Aborted, Success = false });
                    }
                    else if (item.AMMailPieceStatus == "Error" || item.PnSMailPieceStatus == "Error")
                    {
                        mailPieceResponse.Add(new PullBackResponseOld { MailPieceId = item.MailPieceUniqueId.ToString(), Message = Config.Web_Msg_API_PB_Error, ErrorDescription = !string.IsNullOrEmpty(item.PnSErrorDescription) ? item.PnSErrorDescription.ToString() : item.AMErrorDescription.ToString(), Success = false });
                    }
                    else if (item.AMMailPieceStatus == "Shipped" || item.PnSMailPieceStatus == "Shipped")
                    {
                        mailPieceResponse.Add(new PullBackResponseOld { MailPieceId = item.MailPieceUniqueId.ToString(), Message = Config.Web_Msg_API_PB_Shipped, Success = false });
                    }
                    else if (item.AMMailPieceStatus == "eDelivered")
                    {
                        mailPieceResponse.Add(new PullBackResponseOld { MailPieceId = item.MailPieceUniqueId.ToString(), Message = Config.Web_Msg_API_PB_EDelivered, Success = false });
                    }
                    else if (item.AMMailPieceStatus == "InProgress" && (item.AMBatchPipelineStatus == "Validated" || item.AMBatchPipelineStatus == "Acknowledged" || item.AMBatchPipelineStatus == "Prepped" || item.AMBatchPipelineStatus == "Held" || item.AMBatchPipelineStatus == "Suspended"))
                    {
                        int row1 = pullBackSvc.InsertPullBackRequestData(item.MailPieceUniqueId.ToString(), User.Identity.GetClientId().Value, "PulledBack", string.IsNullOrEmpty(User.Identity.GetUserId().ToString()) ? string.Empty : User.Identity.GetUserId().ToString(), pullBackReason);
                        int row = pullBackSvc.UpdatePullBackRequestedOn(item.MailPieceUniqueId.ToString(), string.IsNullOrEmpty(User.Identity.GetUserId().ToString()) ? string.Empty : User.Identity.GetUserId().ToString(), "PulledBack");

                        if (!string.IsNullOrEmpty(eVSCancelLabelRequest.BarcodeNumber) && (item.ShippingServiceType == "PRIORITY" || item.ShippingServiceType == "PRIORITY_EXPRESS"))
                        {
                            var uspsCancelResponse = uSPSApi.RequesteVSLabel(eVSCancelLabelRequest);
                            int updatedRow = pullBackSvc.UpdateUSPSApiCancelErrorDesc(item.MailPieceUniqueId.ToString());
                        }

                        mailPieceResponse.Add(new PullBackResponseOld { MailPieceId = item.MailPieceUniqueId.ToString(), Message = Config.Web_Msg_API_PB_PulledBackSuccess, MailPieceStatus = "PulledBack", PullBackRequestedOn = Common.Now, PulledBackRequestedBy = (User.Identity) == null ? string.Empty : User.Identity.Name, Success = true });
                    }
                    else if (item.AMMailPieceStatus == "InProgress" && item.AMBatchPipelineStatus == "SubmittedToEDelivery" && item.IsEDeliveryRequested)
                    {
                        #region If signingroom is created and user has still not downloaded/reviewed the document then delete signing room and update pullback status.
                        //if (!string.IsNullOrWhiteSpace(item.SigningRoomName))
                        //{
                        //    IEDeliveryAPIService eDeliveryAPISvc = new EDeliveryAPIService();

                        //    var activityLogs = await eDeliveryAPISvc.GetSigningRoomActivityLog(item.SigningRoomName);

                        //    foreach (var actlog in activityLogs)
                        //    {
                        //        switch (actlog.EDeliveryStatus)
                        //        {
                        //            case ActivityStatus.DocumentReviewed:
                        //                item.IsDocReviewed = true;
                        //                break;
                        //            case ActivityStatus.DocumentDownloaded:
                        //                item.IsDocDownloaded = true;
                        //                break;
                        //            default:
                        //                break;
                        //        }
                        //    }

                        //    if (!item.IsDocReviewed && !item.IsDocDownloaded)
                        //    {
                        //        var isSigningRoomDeleted = await eDeliveryAPISvc.DeleteSigningRoom(item.SigningRoomId);
                        //        if (isSigningRoomDeleted)
                        //        {
                        //            int row1 = pullBackSvc.InsertPullBackRequestData(item.MailPieceUniqueId.ToString(), User.Identity.GetClientId().Value, "PulledBack", string.IsNullOrEmpty(User.Identity.GetUserId().ToString()) ? string.Empty : User.Identity.GetUserId().ToString());
                        //            int row = pullBackSvc.UpdatePullBackRequestedOn(item.MailPieceUniqueId.ToString(), string.IsNullOrEmpty(User.Identity.GetUserId().ToString()) ? string.Empty : User.Identity.GetUserId().ToString(), "PulledBack");

                        //            mailPieceResponse.Add(new PullBackResponse { MailPieceId = item.MailPieceUniqueId.ToString(), Message = Config.Web_Msg_API_PB_PulledBackSuccess, MailPieceStatus = "PulledBack", PullBackRequestedOn = Common.Now, PulledBackRequestedBy = (User.Identity) == null ? string.Empty : User.Identity.Name, Success = true });
                        //        }
                        //    }
                        //    else
                        //    {
                        //        mailPieceResponse.Add(new PullBackResponse { MailPieceId = item.MailPieceUniqueId.ToString(), Message = "Mailpiece is already Downloaded/Reviewed by Borrower, pullback cannot be requested.", Success = false });
                        //    }
                        //}
                        #endregion
                        #region If invitation is not sent then only allow to pulledback 
                        if (!item.IsInvitationSent)
                        {
                            IEDeliveryAPIService eDeliveryAPISvc = new EDeliveryAPIService();
                            var isSigningRoomDeleted = await eDeliveryAPISvc.DeleteSigningRoom(item.SigningRoomId, item.ClientId);
                            int row1 = pullBackSvc.InsertPullBackRequestData(item.MailPieceUniqueId.ToString(), User.Identity.GetClientId().Value, "PulledBack", string.IsNullOrEmpty(User.Identity.GetUserId().ToString()) ? string.Empty : User.Identity.GetUserId().ToString(), pullBackReason);
                            int row = pullBackSvc.UpdatePullBackRequestedOn(item.MailPieceUniqueId.ToString(), string.IsNullOrEmpty(User.Identity.GetUserId().ToString()) ? string.Empty : User.Identity.GetUserId().ToString(), "PulledBack");

                            mailPieceResponse.Add(new PullBackResponseOld { MailPieceId = item.MailPieceUniqueId.ToString(), Message = Config.Web_Msg_API_PB_PulledBackSuccess, MailPieceStatus = "PulledBack", PullBackRequestedOn = Common.Now, PulledBackRequestedBy = (User.Identity) == null ? string.Empty : User.Identity.Name, Success = true });
                        }
                        else
                        {
                            mailPieceResponse.Add(new PullBackResponseOld { MailPieceId = item.MailPieceUniqueId.ToString(), Message = "Invitation for signing room is already sent to Borrower, pullback cannot be requested.", Success = false });
                        }
                        #endregion
                    }
                    else if (item.AMMailPieceStatus == "InProgress" && item.AMBatchPipelineStatus == "SubmittedToShipping" && !item.PCIBatchPrepEnd.HasValue)
                    {
                        int row1 = pullBackSvc.InsertPullBackRequestData(item.MailPieceUniqueId.ToString(), User.Identity.GetClientId().Value, "PulledBack", string.IsNullOrEmpty(User.Identity.GetUserId().ToString()) ? string.Empty : User.Identity.GetUserId().ToString(), pullBackReason);
                        int row = pullBackSvc.UpdatePullBackRequestedOn(item.MailPieceUniqueId.ToString(), string.IsNullOrEmpty(User.Identity.GetUserId().ToString()) ? string.Empty : User.Identity.GetUserId().ToString(), "PulledBack");

                        if (!string.IsNullOrEmpty(eVSCancelLabelRequest.BarcodeNumber) && (item.ShippingServiceType == "PRIORITY" || item.ShippingServiceType == "PRIORITY_EXPRESS"))
                        {
                            var uspsCancelResponse = uSPSApi.RequesteVSLabel(eVSCancelLabelRequest);
                            int updatedRow = pullBackSvc.UpdateUSPSApiCancelErrorDesc(item.MailPieceUniqueId.ToString());
                        }

                        mailPieceResponse.Add(new PullBackResponseOld { MailPieceId = item.MailPieceUniqueId.ToString(), Message = Config.Web_Msg_API_PB_PulledBackSuccess, MailPieceStatus = "PulledBack", PullBackRequestedOn = Common.Now, PulledBackRequestedBy = (User.Identity) == null ? string.Empty : User.Identity.Name, Success = true });
                    }
                    else if (item.AMBatchPipelineStatus == "SubmittedToShipping" && item.PCIBatchPrepEnd.HasValue)
                    {
                        if (item.PrintTrailBatchId == null)
                        {
                            int row1 = pullBackSvc.InsertPullBackRequestData(item.MailPieceUniqueId.ToString(), User.Identity.GetClientId().Value, "PulledBack", string.IsNullOrEmpty(User.Identity.GetUserId().ToString()) ? string.Empty : User.Identity.GetUserId().ToString(), pullBackReason);
                            int row = pullBackSvc.UpdatePullBackRequestedOn(item.MailPieceUniqueId.ToString(), string.IsNullOrEmpty(User.Identity.GetUserId().ToString()) ? string.Empty : User.Identity.GetUserId().ToString(), "PulledBack");

                            if (!string.IsNullOrEmpty(eVSCancelLabelRequest.BarcodeNumber) && (item.ShippingServiceType == "PRIORITY" || item.ShippingServiceType == "PRIORITY_EXPRESS"))
                            {
                                var uspsCancelResponse = uSPSApi.RequesteVSLabel(eVSCancelLabelRequest);
                                int updatedRow = pullBackSvc.UpdateUSPSApiCancelErrorDesc(item.MailPieceUniqueId.ToString());
                            }

                            mailPieceResponse.Add(new PullBackResponseOld { MailPieceId = item.MailPieceUniqueId.ToString(), Message = Config.Web_Msg_API_PB_PulledBackSuccess, MailPieceStatus = "PulledBack", PullBackRequestedOn = Common.Now, PulledBackRequestedBy = (User.Identity) == null ? string.Empty : User.Identity.Name, Success = true });
                        }
                        else if (item.PnSMailPieceStatus == "InProgress" && (item.PrintTrailBatchId != null))
                        {
                            mailPieceIdsToPullback.Add(item.MailPieceUniqueId.ToString());
                            if (!string.IsNullOrEmpty(eVSCancelLabelRequest.BarcodeNumber) && (item.ShippingServiceType == "PRIORITY" || item.ShippingServiceType == "PRIORITY_EXPRESS"))
                            {
                                var uspsCancelResponse = uSPSApi.RequesteVSLabel(eVSCancelLabelRequest);
                                int updatedRow = pullBackSvc.UpdateUSPSApiCancelErrorDesc(item.MailPieceUniqueId.ToString());
                            }
                        }
                    }
                }

                if (mailPieceIdsToPullback.Count > 0)
                {
                    var pullBackData = pullBackSvc.GetPullBackData(mailPieceIdsToPullback);

                    bool isGenerated = pullBackSvc.GeneratePullBackForm(pullBackData);
                    if (isGenerated)
                    {
                        foreach (var item in mailPieceIdsToPullback)
                        {
                            int row1 = pullBackSvc.InsertPullBackRequestData(item.ToString(), User.Identity.GetClientId().Value, "Requested", string.IsNullOrEmpty(User.Identity.GetUserId().ToString()) ? string.Empty : User.Identity.GetUserId().ToString(), pullBackReason);
                            int row = pullBackSvc.UpdatePullBackRequestedOn(item.ToString(), string.IsNullOrEmpty(User.Identity.GetUserId().ToString()) ? string.Empty : User.Identity.GetUserId().ToString(), string.Empty);
                            if (row > 0)
                            {
                                mailPieceResponse.Add(new PullBackResponseOld { MailPieceId = item.ToString(), Message = Config.Web_Msg_API_PB_Success, PullBackRequestedOn = Common.Now, PulledBackRequestedBy = (User.Identity) == null ? string.Empty : User.Identity.Name, Success = true });
                            }
                        }
                    }
                    else
                    {
                        return JsonResponseFactory.ErrorResponse(Config.Web_Msg_API_PB_PBReqFormErr);
                    }
                }

                return JsonResponseFactory.SuccessResponse(mailPieceResponse);
            }
            catch (Exception ex)
            {
                return JsonResponseFactory.ErrorResponse(ex.Message);
            }

        }
        //Test Methods
        //[AllowAnonymous]
        //[Route("testpostusps")]
        //public string TestPostUSPS([FromBody] cls p)
        //{
        //    if (p == null && string.IsNullOrWhiteSpace(p.p)) return "String is Null";
        //    return "USPS_Value of P is:" + p.p;
        //}
        //[Route("testpostapi")]
        //public string TestPostAPI([FromBody] cls p)
        //{
        //    if (p == null && string.IsNullOrWhiteSpace(p.p)) return "String is Null";
        //    return "API_Value of P is:" + p.p;
        //}
    }

    public class PullBackRequestInfo
    {
        public List<string> MailPieceIds { get; set; }
        public string PBReason { get; set; }
    }

    [Authorize(Roles = AMRoles.APIUSPSeVS + "," + AMRoles.API), RoutePrefix("api/pullback")]
    public class TestController : ApiController
    {
        [Authorize(Roles = AMRoles.APIUSPSeVS), Route("testpostuspstest")]
        public string TestPostUSPS([FromBody] cls p)
        {
            if (p == null && string.IsNullOrWhiteSpace(p.p)) return "String is Null";
            return "TEST_USPS_Value of P is:" + p.p;
        }
        [Authorize(Roles = AMRoles.API), Route("testpostapitest")]
        public string TestPostAPI([FromBody] cls p)
        {
            if (p == null && string.IsNullOrWhiteSpace(p.p)) return "String is Null";
            return "TEST_API_Value of P is:" + p.p;
        }
    }

    public class cls
    {
        public string p { get; set; }
    }
}
