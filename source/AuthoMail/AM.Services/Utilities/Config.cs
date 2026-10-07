using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Hosting;
using AM.Lib;
using System.ComponentModel;
using AM.Entities;
using System.Text.RegularExpressions;
using System.Net;

namespace AM.Services
{
    public class Config
    {
        private static readonly Dictionary<int, int> clientGroupMap;
        private static readonly Dictionary<int, List<Holiday>> groupHolidays;
        private static readonly Dictionary<string, string> customTemplates;


        #region General
        [Description("To indicate on which environment job is running.")]
        public static string Common_Environment { get; private set; }
        public static string Common_SharedPath { get; private set; }
        public static string Common_WebUrl { get; private set; }
        public static string Common_EnvironmentInitial { get; private set; }
        public static string Common_AuthoMailRootFolder { get; private set; }
        public static string Common_DocGen_API { get; private set; }
        public static int Common_MaxProcessingLimit { get; private set; }
        public static List<Holiday> Common_FederalHolidays { get; private set; }

        public static int Common_ProcessingBufferTime { get; private set; }
        public static string Common_FedExTrackingAccountCode { get; private set; }
        public static string Common_FedExTokenUrl { get; set; }
        public static string Common_FedExShippingLabelUrl { get; set; }
        public static string Common_FedExTrackingUrl { get; private set; }
        public static string Common_ErrorSubscribedEmailSubject { get; private set; }
        public static string Common_DuplicateEmailSubject { get; private set; }
        public static string Common_DuplicateEmailTemplate { get; private set; }
        public static string Common_ErrorSubscribedEmailCc { get; private set; }
        public static string Common_ErrorSubscribedEmailTemplate { get; private set; }
        public static string Common_LandscapeUSPSLabels { get; private set; }
        public static string PnS_PCIRootFolder { get; private set; }
        public static string Common_AMTrash { get; set; }
        public static string Common_PnSTrash { get; set; }
        public static string Common_BlankPage { get; private set; }
        public static string Common_ServerIP { get; set; }
        public static int Common_DallasVolumeCap { get; private set; }
        public static string Core_ChecksKey { get; set; }
        public static string Core_NonChecksKey { get; set; }

        public static List<Holiday> GetHolidaysByClientId(int clientId)
        {
            if (!clientGroupMap.TryGetValue(clientId, out int groupId))
            {
                groupId = 1;
            }

            return groupHolidays[groupId];
        }

        #endregion

        #region Console & Core
        public static string Monitor_AMCPulledBackDate { get; set; }
        public static string Monitor_AMC_ReportDateFormat { get; private set; }
        public static string Monitor_DSEmailTemplate { get; private set; }
        public static string Monitor_PSEmailTemplate { get; private set; }

        public static string Monitor_PSNoDataEmailSubject { get; private set; }
        public static string Monitor_PSNoDataEmailTemplate { get; private set; }

        public static string Monitor_ExceptionEmail_Subject { get; private set; }
        public static string Monitor_Exception_Email_To { get; private set; }
        public static string Monitor_Exception_Email_Cc { get; private set; }
        public static string Monitor_CustomTemplateFolder { get; private set; }
        public static List<PackagesHoldInfo> Monitor_PackagesHoldInfo { get; private set; }

        //Core
        public static string Core_PnS_SFT_RootDir { get; private set; }
        public static string Core_CoverTemplateFolder { get; private set; }
        public static string Core_Base64Folder { get; private set; }
        public static string Core_CoversheetAPI { get; private set; }
        public static string Core_CountryCode { get; private set; }
        public static int Core_NoOfAttemptOnFailure { get; private set; }
        public static int Core_FailureAttemptInterval { get; private set; }
        public static string Core_IncrementalReportingChannelCodes { get; private set; }
        public static string Core_FedExService { get; private set; }
        #endregion

        #region Reporting_ReadOnly
        public static TimeSpan Reporting_USAA_StartCutOffTime
        {
            get
            {
                return Web_StartCutOffTime.IsNullOrWhiteSpace() ? TimeSpan.Zero : TimeSpan.Parse(Web_StartCutOffTime);
            }
        }

        public static TimeSpan Reporting_USAA_EndCutOffTime
        {
            get
            {
                return Web_EndCutOffTime.IsNullOrWhiteSpace() ? TimeSpan.Zero : TimeSpan.Parse(Web_EndCutOffTime);
            }
        }

        #endregion Reporting_ReadOnly

        #region PullBackRequestProps
        [Description("When mailpiece is aborted by client or for unknown reason.")]
        public static string Web_Msg_API_PB_Aborted { get; private set; }
        public static string Web_Msg_API_PB_BatchNotValidated { get; set; }
        [Description("When no data found for specified unique id.")]
        public static string Web_Msg_API_PB_DataNull { get; private set; }
        [Description("user have select atleast one mailpiece to request pullback.")]
        public static string Web_Msg_API_PB_Empty { get; private set; }
        [Description("If mailpiece has some error.")]
        public static string Web_Msg_API_PB_Error { get; private set; }
        [Description("If mailpiece has recorded successfully.")]
        public static string Web_Msg_API_PB_Success { get; private set; }
        [Description("When error occured while generating pullback form.")]
        public static string Web_Msg_API_PB_PBReqFormErr { get; private set; }
        [Description("When pullback is already requested by some user.")]
        public static string Web_Msg_API_PB_PullBackAlreadyRequested { get; private set; }
        [Description("When mailpiece is pulledback.")]
        public static string Web_Msg_API_PB_PulledBack { get; private set; }
        public static string Web_Msg_API_PB_PulledBackSuccess { get; private set; }
        [Description("When mailpiece status is shipped then it can not be pullback.")]
        public static string Web_Msg_API_PB_Shipped { get; private set; }
        [Description("When mailpiece status is EDelivered then it can not be pullback.")]
        public static string Web_Msg_API_PB_EDelivered { get; private set; }
        [Description("To whom need to send email.")]
        public static string Web_API_PB_Email_To_PCI { get; private set; }
        [Description("To whom need to send email.")]
        public static string Web_API_PB_Email_To_NORDIS { get; private set; }
        [Description("To whom need to send email.")]
        public static string Web_API_PB_Email_Cc { get; private set; }
        public static string Web_API_PB_Email_Subject { get; private set; }
        public static string Web_API_PB_Email_BodyMsg { get; private set; }
        [Description("Define client's local timezone.")]
        public static string Web_API_PB_ClientTimeZone { get; private set; }
        [Description("Path of PCI pdf template which is used to send when pullback is requested.")]
        public static string Web_API_PB_PullBackForm_PCI { get; private set; }
        [Description("Path of NORDIS pdf template which is used to send when pullback is requested.")]
        public static string Web_API_PB_PullBackForm_NORDIS { get; private set; }
        #endregion

        #region WEB AccountController
        public static string Web_Msg_Login_InvalidUserName { get; private set; }
        public static string Web_Msg_Login_EmailConfirm { get; private set; }
        public static string Web_Msg_Login_Default { get; private set; }
        public static string Web_Msg_Login_Success { get; private set; }
        public static string Web_Msg_Lockout { get; private set; }
        public static string Web_Msg_Error { get; private set; }
        public static string Web_Msg_Inactive_Account { get; private set; }
        public static bool Web_Login_PasswordExpirationIsEnable { get; private set; }
        public static int Web_Login_PasswordExpirationDays { get; private set; }
        public static string Web_Msg_Login_IPRestricted { get; private set; }

        #endregion

        #region WEB Common Msg
        public static int Web_MaxJsonLength { get; private set; }
        public static string Web_Msg_Ajax_CommonError { get; private set; }
        public static string Web_Msg_NoRecordFound { get; private set; }
        public static string Web_Msg_PullBackConfirmation { get; private set; }
        #endregion

        #region WEB RequestPullBackController
        public static string Web_Msg_NoMailPieceSelect { get; private set; }
        public static string Web_Msg_NoErrorDescription { get; private set; }
        #endregion

        #region WEB HomeController
        public static string Web_Msg_NoMailPieceReceivedToday { get; private set; }
        public static string Web_Msg_NoRecordInTodayAndPendingBatches { get; private set; }

        public static int Web_Search_DisplayLimit { get; private set; }
        public static int Web_Search_DownloadLimit { get; private set; }
        #endregion

        #region WEB ReportController
        [Description("Start cut off time for Mailpiece")]
        public static string Web_StartCutOffTime { get; private set; }
        [Description("End cut off time for Mailpiece")]
        public static string Web_EndCutOffTime { get; private set; }
        #endregion

        #region Web Dropdown For DocType

        [Description("Print size dropdown values")]
        public static string Web_DD_PrintSizeList { get; private set; }

        [Description("Print format dropdown values")]
        public static string Web_DD_PrintFormatList { get; private set; }

        [Description("Shipping Carrier dropdown values")]
        public static string Web_DD_ShippingCarrierList { get; private set; }

        [Description("Shipping Service Type dropdown values")]
        public static string Web_DD_ShippingServiceTypeList { get; private set; }

        [Description("Shipping Special Service dropdown values")]
        public static string Web_DD_ShippingSpecialServiceList { get; private set; }

        #endregion

        #region Web Delfus Template
        [Description("Web Delfus API Url")]
        public static string Web_Delfus_API_Url { get; private set; }
        #endregion

        #region USPSIntegration

        public static bool Web_API_USPS_IsDbLoggingEnabled { get; private set; }
        public static string Web_API_USPS_UserId { get; private set; }
        public static string Web_API_USPS_URL { get; private set; }
        public static string Web_API_USPS_VerifyAPIName { get; private set; }
        public static string Web_API_USPS_MilitaryStates { get; private set; }
        public static string Web_API_USPS_MilitaryCities { get; private set; }
        public static string Web_API_USPS_Territory { get; private set; }
        public static int Web_API_USPS_AddressCount { get; private set; }
        public static string Web_API_USPS_MainlandStates { get; private set; }
        public static string Web_API_USPS_eVSAPIName { get; private set; }
        public static string Web_API_USPS_eVSCancelAPIName { get; private set; }
        public static int Web_API_USPS_TrackingCount { get; private set; }
        public static string Web_API_USPS_TrackingAPIName { get; private set; }
        public static string Web_API_USPS_TrackingSourseZipCode { get; private set; }
        public static bool Web_API_USPS_Tracking_IsDbLoggingEnabled { get; private set; }
        #endregion

        #region E-DeliveryAPI
        public static string E_API_BaseURL { get; set; }
        public static string E_API_ClientId { get; set; }
        public static string E_API_UserName { get; set; }
        public static string E_API_Password { get; set; }
        public static string E_API_CreateSigningRoomURL { get; set; }
        public static string E_API_SendInvitationURL { get; set; }
        public static string E_API_SigningRoomActivityLogSearchURL { get; set; }
        public static string E_API_SigningRoomSummaryURL { get; set; }
        public static string E_API_DeleteSigningRoomURL { get; set; }
        public static string E_API_DownloadEsignedDocURL { get; set; }

        #endregion

        #region Support
        [Description("Default emails for getting notification from registration request.")]
        public static string Web_RegistrationDefaultEmails { get; private set; }
        [Description("Default BCC emails for getting notification from registration flow.")]
        public static string Web_RegistrationBCCEmails { get; private set; }
        [Description("Notify about AuthoMail health every elapsed minutes.")]
        public static int Web_API_AlertElapsedMinute { get; set; }
        [Description("Inventory Buffer Stock Percentage")]
        public static int Common_InventoryBufferStockDays { get; set; }
        [Description("For Single Loan Entry")]
        public static int Web_SubmissionSFTChannelId { get; set; }
        #endregion Support

        #region API
        public static string MailRequestAPI_FilesDownloadPath { get; set; }
        public static int MailRequestAPI_MaxRequestSize { get; set; }
        public static int MailRequestAPI_MaxRetries { get; set; }
        public static int MailResponseAPI_MaxParallelCallBack { get; set; }
        public static string PGP_Passphrase { get; set; }

        #endregion

        #region Scanner
        public static string SP_GetScanStats { get; set; }
        public static string SP_RegisterScan { get; set; }
        public static string SP_RegisterUSPSScan { get; set; }
        public static string SP_GetScanReportData { get; set; }
        public static string SP_GetRecentBatchesStats { get; set; }
        public static string SP_GetPendingScanning { get; set; }
        public static string SP_GetScannedMailPieces { get; set; }
        #endregion

        #region HSBC_IPRestriction

        public static string HSBC_WhiteListedIPs { get; set; }

        #endregion             

        #region Web Pdf File API Url
        [Description("Web Pdf File API Url")]
        public static string Web_Pdf_API_Url { get; private set; }
        [Description("Web Reship File API Url")]
        public static string Web_Reship_API_Url { get; private set; }
        #endregion

        #region CLAuth API

        public static bool CLAuth_EnabledAxisAuthentication { get; private set; }
        [Description("CLAuth API Username")]
        public static string CLAuth_Username { get; private set; }
        [Description("CLAuth API Password")]
        public static string CLAuth_Password { get; private set; }
        [Description("CLAuth API Token Url")]
        public static string CLAuth_TokenUrl { get; private set; }
        [Description("CLAuth API Get User Url")]
        public static string CLAuth_UserDetailsUrl { get; private set; }
        [Description("CLAuth API Organization Id")]
        public static string CLAuth_OrganizationId { get; private set; }

        #endregion

        #region Web USPS Location

        public static string Web_API_USPS_AccountsByPrintLocations { get; set; }
        public static List<USPSLabelCutOffDetail> USPSLabelCutOffDetails { get; private set; }
        public static List<USPSCredentialInfo> USPSCredentialDetails { get; private set; }

        #endregion

        private static List<string> errors;

        public static string GetCustomTemplate(string key)
        {
            if (customTemplates != null && customTemplates.TryGetValue(key, out var template))
            {
                return template;
            }

            return null;
        }

        static Config()
        {
            // Initialize config settings
            InitializeProperties<Config>();
            using (ICommonService commonSvc = new CommonService())
            {
                //Common_Holidays = commonSvc.GetHolidayList();
                Common_FederalHolidays = commonSvc.GetFederalHolidayList();
                clientGroupMap = commonSvc.GetClientHolidayGroup();
                groupHolidays = commonSvc.GetHolidayList();
                Monitor_PackagesHoldInfo = commonSvc.GetPackagesHoldInfo();
                USPSLabelCutOffDetails = commonSvc.GetUSPSLabelCutOffDetails();
                USPSCredentialDetails = commonSvc.GetUSPSCredentialDetails();
                customTemplates = commonSvc.GetCustomTemplates();
            }

            string docGenServerIP = Environment.GetEnvironmentVariable("SD_IP");
            if (!docGenServerIP.IsNullOrWhiteSpace())
            {
                Common_DocGen_API = Common_DocGen_API.Replace("{DocGenServerIP}", docGenServerIP);
                Core_CoversheetAPI = Core_CoversheetAPI.Replace("{DocGenServerIP}", docGenServerIP);
            }

            string ipv4Pattern = @"^(25[0-5]|2[0-4][0-9]|[0-1]?[0-9][0-9]?)\.(
             25[0-5]|2[0-4][0-9]|[0-1]?[0-9][0-9]?)\.(
             25[0-5]|2[0-4][0-9]|[0-1]?[0-9][0-9]?)\.(
             25[0-5]|2[0-4][0-9]|[0-1]?[0-9][0-9]?)$";
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());

            foreach (var ip in ipHostInfo.AddressList)
            {
                var isValid = Regex.IsMatch(ip.ToString(), ipv4Pattern);

                if (isValid)
                {
                    Common_ServerIP = ip.ToString();
                    break;
                }
            }

        }

        private static void InitializeProperties<T>()
        {
            errors = new List<string>();

            // Load settings from database
            ConfigService cs = new ConfigService();
            var settings = cs.GetSystemSettings();

            PropertyInfo[] propInfo = typeof(T).GetProperties(BindingFlags.Static | BindingFlags.Public);

            foreach (PropertyInfo p in propInfo)
            {
                //var isRequired = p.CustomAttributes.Any(ca => ca.AttributeType == typeof(RequiredInConfigAttribute));

                var authomailSetting = settings.SingleOrDefault(s => s.Id == p.Name);// db.ERPSettings.Find(p.Name);

                // Ignore if not stored in the database
                if (authomailSetting == null) continue;


                object propValue;
                try
                {
                    propValue = p.PropertyType.GetConversionMethod().Invoke(null, new object[] { authomailSetting.Value });
                }
                catch (Exception ex)
                {
                    propValue = null;
                    errors.Add(string.Format("{0}, Type - {1} : {2}", p.Name, p.PropertyType.Name, ex.ToString()));
                }

                p.SetValue(null, propValue);
                //if (isRequired && (propValue == null || String.IsNullOrWhiteSpace(propValue.ToString())))
                //{
                //    errors.Add(p.Name);
                //}

                //else if (isRequired)
                //{
                //    errors.Add(p.Name);
                //}
            }

        }

        public static void SaveSetting(string key, string value)
        {
            SaveSetting(key, value, null, null);
        }

        public static void SaveSetting(string key, string value, bool? IsSystem, string DefaultValue)
        {

            var prop = typeof(Config).GetProperty(key, BindingFlags.Static | BindingFlags.Public);

            if (prop == null)
            {
                throw new ArgumentException("No such AM setting found. ", key);
            }

            ConfigService cs = new ConfigService();
            var settings = cs.GetSystemSettings();
            var authomailSetting = settings.SingleOrDefault(s => s.Id == key);   //db.ERPSettings.Find(key);

            var propValue = prop.PropertyType.GetConversionMethod().Invoke(null, new object[] { value });
            prop.SetValue(null, propValue);

            if (authomailSetting == null)
            {
                cs.InsertSystemSetting(new AMSetting
                {
                    Id = key,
                    Value = value,
                    IsSystem = IsSystem ?? false,
                    DefaultValue = DefaultValue,
                    Description = prop.GetDescription(),
                    DataType = prop.PropertyType.Name
                });
            }
            else
            {
                cs.UpdateSystemSetting(new AMSetting
                {
                    DataType = prop.PropertyType.Name,
                    Value = value,
                    Description = prop.GetDescription(),
                    ModifiedOn = Common.Now,
                    Id = key
                });
            }
        }

        public static void SaveAllSetting()
        {
            PropertyInfo[] propInfo = typeof(Config).GetProperties(BindingFlags.Static | BindingFlags.Public);
            ConfigService cs = new ConfigService();
            var settings = cs.GetSystemSettings();

            foreach (PropertyInfo p in propInfo)
            {
                var authomailSetting = settings.SingleOrDefault(s => s.Id == p.Name);
                if (authomailSetting != null)
                {
                    cs.UpdateSystemSetting(new AMSetting
                    {
                        DataType = p.PropertyType.Name,
                        Value = authomailSetting.Value,
                        Description = p.GetDescription(),
                        ModifiedOn = Common.Now,
                        Id = p.Name
                    });
                }
            }
        }
    }
}