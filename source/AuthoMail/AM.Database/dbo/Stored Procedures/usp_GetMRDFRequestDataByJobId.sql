
			CREATE PROCEDURE [dbo].[usp_GetMRDFRequestDataByJobId]
			@jobId int,
			@mailPieceStatus varchar(255)
			AS
			BEGIN
				-- SET NOCOUNT ON added to prevent extra result sets from
				-- interfering with SELECT statements.
				SET NOCOUNT ON;
				If(@mailPieceStatus='ALL')
				BEGIN
					SELECT JobId, PieceId, [Date], TotalSheets, InputWeight, Feeder3_Sel, Feeder4_Sel
					, Feeder5_Sel, LowCap_Sel, NoSeal_Sel, Feeder2_Sel, ZipMark_Sel, Address1, Address2
					, Zip5, Zip4, Zip2, DivertVS2, DivertVS3
					, DivertVS4, ReturnLogoBitMap, ReturnLine1, ReturnLine2, ReturnLine3, ReturnLine4
					, ReturnLine5, ReturnLine6, GroupNumber, SocialSecNumber, CheckAmount, PassThru
					, Expansion, EndofRecord, UniqueId, PDFFileName,ErrId, ErrDesc, MailPieceStatus, IsPDFReceived 
					FROM [PnS].[MRDFRequest] WHERE JobId = @jobId
				END
				ELSE If(@mailPieceStatus='INTERNAL')--This will be used for getting only submitted records to PCI
				BEGIN
					SELECT JobId, PieceId, [Date], TotalSheets, InputWeight, Feeder3_Sel, Feeder4_Sel
					, Feeder5_Sel, LowCap_Sel, NoSeal_Sel, Feeder2_Sel, ZipMark_Sel, Address1, Address2
					, Zip5, Zip4, Zip2, DivertVS2, DivertVS3
					, DivertVS4, ReturnLogoBitMap, ReturnLine1, ReturnLine2, ReturnLine3, ReturnLine4
					, ReturnLine5, ReturnLine6, GroupNumber, SocialSecNumber, CheckAmount, PassThru
					, Expansion, EndofRecord, UniqueId, PDFFileName,ErrId, ErrDesc, MailPieceStatus, IsPDFReceived 
					FROM [PnS].[MRDFRequest] WHERE JobId = @jobId AND ErrId NOT IN (-1,-2,-3)
				END
				ELSE
				BEGIN
					SELECT JobId, PieceId, [Date], TotalSheets, InputWeight, Feeder3_Sel, Feeder4_Sel
					, Feeder5_Sel, LowCap_Sel, NoSeal_Sel, Feeder2_Sel, ZipMark_Sel, Address1, Address2
					, Zip5, Zip4, Zip2, DivertVS2, DivertVS3
					, DivertVS4, ReturnLogoBitMap, ReturnLine1, ReturnLine2, ReturnLine3, ReturnLine4
					, ReturnLine5, ReturnLine6, GroupNumber, SocialSecNumber, CheckAmount, PassThru
					, Expansion, EndofRecord, UniqueId, PDFFileName,ErrId, ErrDesc, MailPieceStatus, IsPDFReceived 
					FROM [PnS].[MRDFRequest] WHERE JobId = @jobId AND ISNULL(MailPieceStatus,'')=@mailPieceStatus
				END
			END
