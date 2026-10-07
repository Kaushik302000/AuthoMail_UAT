using AM.Services;
using AM.Web.Models;
using AM.Web.USPSIMService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AM.Web.Controllers
{
    public class TrackController : BaseController
    {
        // GET: Track
        private ISupportService supportSvc;
        public TrackController(ISupportService supportSvc)
        {
            this.supportSvc = supportSvc;
        }

        public ActionResult Index(string id = null, bool isEDelivery = false)
        {
            TrackingInfoViewModel model = new TrackingInfoViewModel();

            if (isEDelivery)
            {
                TrackingDetailViewModel trackingModel = new TrackingDetailViewModel();
                trackingModel.IsError = false;
                model.TrackingDetailViewModel = trackingModel;
                model.EDeliveryStatusInfoModel = supportSvc.GetEDeliveryHistoryByMailBatchId(int.Parse(id));
            }
            else
            {
                model.TrackingDetailViewModel = supportSvc.GetTrackingDetailByTrackingNo(id);
                if (model.TrackingDetailViewModel.ShippingCarrier == "USPS" && model.TrackingDetailViewModel.ShippingServiceType == "IMB")
                {
                    USPSIMClient uspsClient = new USPSIMClient();
                    model.USPSTrackingInfo = uspsClient.GetShipmentStatus(id);
                }
            }

            return View(model);
        }
        public ActionResult _GetTrackingInfo()
        {

            return PartialView();
        }

        public ActionResult _GetEDeliveryTrackingInfo()
        {
            return PartialView();
        }
    }
}