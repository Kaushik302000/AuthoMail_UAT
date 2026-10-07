using AM.Entities;
using AM.Lib;
using AM.Services;
using AM.Web.USPSIMService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace AM.Web.Models
{
    public class TrackingInfoViewModel
    {
        public USPSTrackingInfo USPSTrackingInfo { get; set; }
        public TrackingDetailViewModel TrackingDetailViewModel { get; set; }
        public List<EDeliveryStatusInfoModel> EDeliveryStatusInfoModel { get; set; }
    }
}