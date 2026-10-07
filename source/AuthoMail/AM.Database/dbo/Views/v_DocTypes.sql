

CREATE VIEW [dbo].[v_DocTypes]
AS
SELECT D.Id, D.ClientId, D.QCPercent, D.IsReturnInfoSpecified, D.RetRcptPrefix, D.RetRcptFirstName, D.RetRcptMiddleName, D.RetRcptLastName
, D.RetRcptSuffix, D.RetRcptPhone, D.RetRcptOrganization, D.RetRcptAddrOptionalLine1AboveStreet, D.RetRcptAddrOptionalLine2AboveStreet
, D.RetRcptAddressStreet, D.RetRcptAddressAptOrSuiteOrOther, D.RetRcptAddrOptionalLine1BelowStreet, D.RetRcptAddrOptionalLine2BelowStreet
, D.RetRcptAddressCity, D.RetRcptAddressState, D.RetRcptAddressZIP, D.RetRcptAddressCountry, D.ClientEmails, D.ClientContactNo, D.SupportEmails
, D.SupportContactNo, D.IsActive, D.ModifiedOn, D.CreatedOn, D.CreatedBy, D.ModifiedBy, D.IsBRE, D.NoOfCopies, PD.IsPrintReady
, D.DocTypeName, D.PnSConfigId, D.PaperType, PD.Id AS PnSConfigDetailId, PD.PrintSize, PD.ContentScalePercent, D.PrintFormat, PD.ShippingCarrier
, PD.ShippingServiceType, PD.ShippingAccountCode, PD.ShippingSpecialService, PD.MinPage, PD.MaxPage, PD.EnvelopeId, PD.IsFallBack
, PD.BizdayCutOff, PD.BizdaySLAType, PD.BizdayTimeSpan, PD.SatCutOff, PD.SatSLAType, PD.SatTimeSpan, PD.SunCutOff, PD.SunSLAType
, PD.SunTimeSpan, PD.BizDayStartTime, PD.BizDayEndTime, PD.SatStartTime, PD.SatEndTime, PD.SunStartTime, PD.SunEndTime, PD.PrintTrailId
, D.AllowInsertsOnly, P.ConfigName, P.DocumentCode, PT.PrintVendorCode, D.IsChecks, D.HasReturnLabel
, D.ReturnShippingCarrier, D.ReturnShippingServiceType, D.ReturnShippingAccountCode
FROM AM.DocTypes D 
INNER JOIN Common.PnSConfig P ON D.PnSConfigId=P.Id
INNER JOIN Common.PnSConfigDetail PD ON P.Id=PD.PnSConfigId
LEFT OUTER JOIN Common.PrintTrails PT ON PD.PrintTrailId = PT.Id
