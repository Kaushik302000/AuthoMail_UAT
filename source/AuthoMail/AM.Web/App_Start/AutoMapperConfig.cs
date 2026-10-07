using AM.Services;
using AM.Web.Models;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AM.Web
{
    public class AutoMapperConfig
    {
        public static void ConfigureMappings()
        {
            Mapper.Initialize(cfg =>
           {
               //cfg.CreateMap<MailPieces, SearchViewModel>()
               //    .ForMember(d => d.ReceivedOn, o => o.MapFrom(s => s.MailBatches != null ? (s.MailBatches.ReceivedOn != null ? s.MailBatches.ReceivedOn : null) : null))
               //    .ForMember(d => d.ShippingCarrier, o => o.MapFrom(s => s.DocTypes != null ? (s.DocTypes.ShippingCarrier != null ? s.DocTypes.ShippingCarrier : "") : null))
               //    .ForMember(d => d.ShippingMethod, o => o.MapFrom(s => s.DocTypes != null ? (s.DocTypes.ShippingServiceType != null ? s.DocTypes.ShippingServiceType : "") : null))
               //    .ForMember(d => d.Channel, o => o.MapFrom(s => s.MailBatches != null ? (s.MailBatches.SFTChannels.Title != null ? s.MailBatches.SFTChannels.Title : "") : null))
               //    .ForMember(d => d.FirstName, o => o.MapFrom(s => s.RcptFirstName))
               //    .ForMember(d => d.LastName, o => o.MapFrom(s => s.RcptLastName))
               //    .ForMember(d => d.ErrorDesc, o => o.MapFrom(s => s.ErrorDescription))
               //    .ForMember(d => d.Status, o => o.MapFrom(s => s.ShippingStatus))
               //    .ForMember(d => d.BusinessIdValue, o => o.MapFrom(s => s.BusinessFriendlyIdValue))
               //    .ForMember(d => d.DocType, o => o.MapFrom(s => s.DocumentOrPackageType))
               //    .ForMember(d => d.City, o => o.MapFrom(s => s.RcptAddressCity))
               //    .ForMember(d => d.ZIP, o => o.MapFrom(s => s.RcptAddressZIP))
               //.ForMember(d => d.State, o => o.MapFrom(s => s.RcptAddressState));

               cfg.CreateMap<MailPieces, MailPieceViewModel>()
                   .ForMember(d => d.ReceivedOn, o => o.MapFrom(s => s.MailBatches != null ? (s.MailBatches.ReceivedOn != null ? s.MailBatches.ReceivedOn : null) : null))
                   .ForMember(d => d.Address, o => o.MapFrom(s => s.RcptAddressStreet))
                   .ForMember(d => d.ErrorDesc, o => o.MapFrom(s => s.ErrorDescription))
                   .ForMember(d => d.Name, o => o.MapFrom(s => string.Format("{0} {1}", s.RcptFirstName, s.RcptLastName)))
                   .ForMember(d => d.Status, o => o.MapFrom(s => s.ShippingStatus))
                   .ForMember(d => d.BusinessIdValue, o => o.MapFrom(s => s.BusinessFriendlyIdValue))
                   .ForMember(d => d.DocType, o => o.MapFrom(s => s.DocumentOrPackageType))
                   .ForMember(d => d.AppCode, o => o.MapFrom(s => s.Clients.AppCode));
               //Above code is commented

               cfg.CreateMap<MailPieces, DailyDetailReportViewModel>()
                  .ForMember(d => d.ReceivedOn, o => o.MapFrom(s => s.MailBatches != null ? (s.MailBatches.ReceivedOn != null ? s.MailBatches.ReceivedOn : null) : null))
                  .ForMember(d => d.Address, o => o.MapFrom(s => s.RcptAddressStreet))
                  .ForMember(d => d.FirstName, o => o.MapFrom(s => s.RcptFirstName))
                  .ForMember(d => d.LastName, o => o.MapFrom(s => s.RcptLastName))
                  .ForMember(d => d.State, o => o.MapFrom(s => s.RcptAddressState))
                  .ForMember(d => d.City, o => o.MapFrom(s => s.RcptAddressCity))
                  .ForMember(d => d.ZIP, o => o.MapFrom(s => s.RcptAddressZIP))
                  .ForMember(d => d.Channel, o => o.MapFrom(s => s.MailBatches != null ? (s.MailBatches.SFTChannels.Title != null ? s.MailBatches.SFTChannels.Title : "") : null))
                  .ForMember(d => d.BatchId, o => o.MapFrom(s => s.MailBatches != null ? s.MailBatches.Id : 0))
                  .ForMember(d => d.ClientCode, o => o.MapFrom(s => s.Clients != null ? (s.Clients.AppCode != null ? s.Clients.AppCode : "") : ""))
                  .ForMember(d => d.ClientBatchId, o => o.MapFrom(s => s.MailBatches != null ? (s.MailBatches.ClientBatchId != null ? s.MailBatches.ClientBatchId : "") : ""))
                  .ForMember(d => d.ShippingCarrier, o => o.MapFrom(s => s.PnSConfigDetails != null ? (s.PnSConfigDetails.ShippingCarrier != null ? s.PnSConfigDetails.ShippingCarrier : "") : null))
                  .ForMember(d => d.ShippingMethod, o => o.MapFrom(s => s.PnSConfigDetails != null ? (s.PnSConfigDetails.ShippingServiceType != null ? s.PnSConfigDetails.ShippingServiceType : "") : null))
                  .ForMember(d => d.Status, o => o.MapFrom(s => s.ShippingStatus))
                  .ForMember(d => d.BusinessIdValue, o => o.MapFrom(s => s.BusinessFriendlyIdValue))
                  .ForMember(d => d.DocType, o => o.MapFrom(s => s.DocumentOrPackageType));

               cfg.CreateMap<MailPieces, ErrorSLAReportViewModel>()
                  .ForMember(d => d.ReceivedOn, o => o.MapFrom(s => s.MailBatches != null ? (s.MailBatches.ReceivedOn != null ? s.MailBatches.ReceivedOn : null) : null))
                  .ForMember(d => d.ErrorDesc, o => o.MapFrom(s => s.ErrorDescription))
                  .ForMember(d => d.Status, o => o.MapFrom(s => s.ShippingStatus))
                  .ForMember(d => d.Address, o => o.MapFrom(s => s.RcptAddressStreet))
                  .ForMember(d => d.FirstName, o => o.MapFrom(s => s.RcptFirstName))
                  .ForMember(d => d.LastName, o => o.MapFrom(s => s.RcptLastName))
                  .ForMember(d => d.State, o => o.MapFrom(s => s.RcptAddressState))
                  .ForMember(d => d.ShippedOn, o => o.MapFrom(s => s.ShippedOn))
                  .ForMember(d => d.City, o => o.MapFrom(s => s.RcptAddressCity))
                  .ForMember(d => d.ZIP, o => o.MapFrom(s => s.RcptAddressZIP))
                  .ForMember(d => d.Channel, o => o.MapFrom(s => s.MailBatches != null ? (s.MailBatches.SFTChannels.Title != null ? s.MailBatches.SFTChannels.Title : "") : null))
                  .ForMember(d => d.BatchId, o => o.MapFrom(s => s.MailBatches != null ? s.MailBatches.Id : 0))
                  .ForMember(d => d.BusinessIdValue, o => o.MapFrom(s => s.BusinessFriendlyIdValue))
                  .ForMember(d => d.DocType, o => o.MapFrom(s => s.DocumentOrPackageType))
                  .ForMember(d => d.ShippingCarrier, o => o.MapFrom(s => s.PnSConfigDetails != null ? (s.PnSConfigDetails.ShippingCarrier != null ? s.PnSConfigDetails.ShippingCarrier : "") : null))
                  .ForMember(d => d.ShippingMethod, o => o.MapFrom(s => s.PnSConfigDetails != null ? (s.PnSConfigDetails.ShippingServiceType != null ? s.PnSConfigDetails.ShippingServiceType : "") : null));

               //cfg.CreateMap<MailPieces, MonthlyReconReportViewModel>()
               //   .ForMember(d => d.ReceivedOn, o => o.MapFrom(s => s.MailBatches != null ? (s.MailBatches.ReceivedOn != null ? s.MailBatches.ReceivedOn : null) : null))
               //   .ForMember(d => d.Channel, o => o.MapFrom(s => s.MailBatches != null ? (s.MailBatches.SFTChannels.Title != null ? s.MailBatches.SFTChannels.Title : "") : null))
               //   .ForMember(d => d.Address, o => o.MapFrom(s => s.RcptAddressStreet))
               //   .ForMember(d => d.Status, o => o.MapFrom(s => s.ShippingStatus))
               //   .ForMember(d => d.FirstName, o => o.MapFrom(s => s.RcptFirstName))
               //   .ForMember(d => d.LastName, o => o.MapFrom(s => s.RcptLastName))
               //   .ForMember(d => d.State, o => o.MapFrom(s => s.RcptAddressState))
               //   .ForMember(d => d.City, o => o.MapFrom(s => s.RcptAddressCity))
               //   .ForMember(d => d.ZIP, o => o.MapFrom(s => s.RcptAddressZIP))
               //   .ForMember(d => d.ShippingMethod, o => o.MapFrom(s => s.DocTypes != null ? (s.DocTypes.ShippingServiceType != null ? s.DocTypes.ShippingServiceType : "") : null))
               //   .ForMember(d => d.BusinessIdValue, o => o.MapFrom(s => s.BusinessFriendlyIdValue))
               //   .ForMember(d => d.DocType, o => o.MapFrom(s => s.DocumentOrPackageType));

               cfg.CreateMap<MailPieces, SummaryControlViewModel>()
                  .ForMember(d => d.ReceivedOn, o => o.MapFrom(s => s.MailBatches != null ? (s.MailBatches.ReceivedOn != null ? s.MailBatches.ReceivedOn : null) : null))
                  .ForMember(d => d.Address, o => o.MapFrom(s => s.RcptAddressStreet))
                  .ForMember(d => d.ErrorDesc, o => o.MapFrom(s => s.ErrorDescription))
                  .ForMember(d => d.FirstName, o => o.MapFrom(s => s.RcptFirstName))
                  .ForMember(d => d.LastName, o => o.MapFrom(s => s.RcptLastName))
                  .ForMember(d => d.State, o => o.MapFrom(s => s.RcptAddressState))
                  .ForMember(d => d.City, o => o.MapFrom(s => s.RcptAddressCity))
                  .ForMember(d => d.ZIP, o => o.MapFrom(s => s.RcptAddressZIP))
                  .ForMember(d => d.Channel, o => o.MapFrom(s => s.MailBatches != null ? (s.MailBatches.SFTChannels.Title != null ? s.MailBatches.SFTChannels.Title : "") : null))
                  .ForMember(d => d.BatchId, o => o.MapFrom(s => s.MailBatches != null ? s.MailBatches.Id : 0))
                  .ForMember(d => d.ClientCode, o => o.MapFrom(s => s.Clients != null ? (s.Clients.AppCode != null ? s.Clients.AppCode : "") : ""))
                  .ForMember(d => d.ClientBatchId, o => o.MapFrom(s => s.MailBatches != null ? (s.MailBatches.ClientBatchId != null ? s.MailBatches.ClientBatchId : "") : ""))
                  .ForMember(d => d.ShippingMethod, o => o.MapFrom(s => s.PnSConfigDetails != null ? (s.PnSConfigDetails.ShippingServiceType != null ? s.PnSConfigDetails.ShippingServiceType : "") : null))
                  .ForMember(d => d.ShippingCarrier, o => o.MapFrom(s => s.PnSConfigDetails != null ? (s.PnSConfigDetails.ShippingCarrier != null ? s.PnSConfigDetails.ShippingCarrier : "") : null))
                  .ForMember(d => d.Status, o => o.MapFrom(s => s.ShippingStatus))
                  .ForMember(d => d.BusinessIdValue, o => o.MapFrom(s => s.BusinessFriendlyIdValue))
                  .ForMember(d => d.DocType, o => o.MapFrom(s => s.DocumentOrPackageType));

               cfg.CreateMap<MailPieces, ErrorReportViewModel>()
                  .ForMember(d => d.ReceivedOn, o => o.MapFrom(s => s.MailBatches != null ? (s.MailBatches.ReceivedOn != null ? s.MailBatches.ReceivedOn : null) : null))
                  .ForMember(d => d.Address, o => o.MapFrom(s => s.RcptAddressStreet))
                  .ForMember(d => d.FirstName, o => o.MapFrom(s => s.RcptFirstName))
                  .ForMember(d => d.ErrorDesc, o => o.MapFrom(s => s.ErrorDescription))
                  .ForMember(d => d.LastName, o => o.MapFrom(s => s.RcptLastName))
                  .ForMember(d => d.State, o => o.MapFrom(s => s.RcptAddressState))
                  .ForMember(d => d.City, o => o.MapFrom(s => s.RcptAddressCity))
                  .ForMember(d => d.ZIP, o => o.MapFrom(s => s.RcptAddressZIP))
                  .ForMember(d => d.Channel, o => o.MapFrom(s => s.MailBatches != null ? (s.MailBatches.SFTChannels.Title != null ? s.MailBatches.SFTChannels.Title : "") : null))
                  .ForMember(d => d.BatchId, o => o.MapFrom(s => s.MailBatches != null ? s.MailBatches.Id : 0))
                    .ForMember(d => d.ClientBatchId, o => o.MapFrom(s => s.MailBatches != null ? (s.MailBatches.ClientBatchId != null ? s.MailBatches.ClientBatchId : "") : ""))
                  .ForMember(d => d.ShippingMethod, o => o.MapFrom(s => s.PnSConfigDetails != null ? (s.PnSConfigDetails.ShippingServiceType != null ? s.PnSConfigDetails.ShippingServiceType : "") : null))
                  .ForMember(d => d.ShippingCarrier, o => o.MapFrom(s => s.PnSConfigDetails != null ? (s.PnSConfigDetails.ShippingCarrier != null ? s.PnSConfigDetails.ShippingCarrier : "") : null))
                  .ForMember(d => d.BusinessIdValue, o => o.MapFrom(s => s.BusinessFriendlyIdValue))
                  .ForMember(d => d.DocType, o => o.MapFrom(s => s.DocumentOrPackageType));

               cfg.CreateMap<MailPieces, MailPieceDetailsViewModel>()
                   .ForMember(d => d.BatchName, o => o.MapFrom(s => s.MailBatches == null ? null : s.MailBatches.BatchName))
                   .ForMember(d => d.AppCode, o => o.MapFrom(s => s.Clients == null ? null : s.Clients.AppCode))
                   .ForMember(d => d.ReceivedOn, o => o.MapFrom(s => s.MailBatches == null ? null : s.MailBatches.ReceivedOn))
                   .ForMember(d => d.SFTChannels, o => o.MapFrom(s => s.MailBatches != null ? (s.MailBatches.SFTChannels.Title != null ? s.MailBatches.SFTChannels.Title : "") : null))
                   .ForMember(d => d.PrintFileName, o => o.MapFrom(s => s.MRDFRequests != null ? s.MRDFRequests.PrintFileName : null))
                   .ForMember(d => d.DocTypeCode, o => o.MapFrom(s => s.DocTypes == null ? null : s.DocTypes.DocTypeCode))
                   .ForMember(d => d.PrintBatch, o => o.MapFrom(s => s.MRDFRequests != null && s.MRDFRequests.JobId != 0 ? s.MRDFRequests.JobId.ToString() : null))
                   .ForMember(d => d.MailPieceId, o => o.MapFrom(s => s.Id))
                   .ForMember(d => d.MailBatchId, o => o.MapFrom(s => s.MailBatchId));

               cfg.CreateMap<Envelopes, EnvelopesListVM>()
                   .ForMember(d => d.AlertLevel, o => o.Ignore());
           });
        }
    }
}