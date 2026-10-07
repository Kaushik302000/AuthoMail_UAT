using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace AM.Services
{
    public interface IProjectRequestService : IDisposable
    {
        KeyValuePair<bool, string> SaveProjectRequest(ProjectRequest projectRequest, string user);
        KeyValuePair<bool, string> SaveProjectRequestFiles(int Id, string files, string userId);
        ProjectRequest ProjectRequestFindById(int Id);
        List<ProjectRequest> GetAllProjectRequest();
        KeyValuePair<bool, string> RemoveFileName(int Id, string fileName, string userId);
    }
    public class ProjectRequestService : ServiceBase, IProjectRequestService
    {

        public KeyValuePair<bool, string> SaveProjectRequest(ProjectRequest projectRequest, string user)
        {
            //Validation
            int Id = 0;
            try
            {
                if (projectRequest.Id == 0)
                {
                    string query = @"INSERT INTO [AM].[ProjectRequest] ([ClientName] ,[SubmittedOn] ,[SubmittedBy] ,[UATStartOn] ,[ProdStartOn] ,[Description] ,[FileTransmittedVia] ,[NoOfEstSweepTimes] ,[ESTSweepTime] ,[TypeOfFiles] ,[TransmittedTime] ,[ServiceLevel] ,[EstimatedPagesPerPackage] ,[EstimatedPackagesDaily] ,[EstimatedPackagesMonthly] ,[EstimatedPackagesAnnually] ,[DailyPrintSchedule] ,[PaperPrintRequirements] ,[PackagesSentToCustomer] ,[IsSampleEnvelopeAttached] ,[Envelopes] ,[IsBRERequired] ,[BusinessReplyMailEnvelope] ,[IsCustomerSupplyEnvelope] ,[IsCustomerProvideArtworkEnvelope] ,[IsClientPostageAccount] ,[IsScanInAuthoMailDallas],[FilesTransmittedOther] ,[TypeOfFilesOther] ,[PaperAndPrintDetails] ,[ShippingMailingDetail] ,[MailingEnvelopeDetail] ,[BusinessReplyMailEnvelopeDetail] ,[ListOfDocTypesAndPages] ,[InsertRequired] ,[BookletRequired] ,[BusinessReplyMailEnvelopeRequired] ,[OtherMaterialRequired] ,[InsertMaterialOrderedBy] ,[AdditionalInfo] ,[CreatedOn] ,[CreatedBy] ,[ModifiedOn] ,[ModifiedBy],[ServiceLevelOther],[PaperPrintRequirementsOther],[PackagesSentToCustomerOther],[EnvelopesOther],[BusinessReplyMailEnvelopeOther],[FilesTransmittedDetail],[TypeOfFilesDetail]) VALUES (@ClientName  ,@SubmittedOn  ,@SubmittedBy  ,@UATStartOn  ,@ProdStartOn  ,@Description  ,@FileTransmittedVia ,@NoOfEstSweepTimes ,@ESTSweepTime ,@TypeOfFiles  ,@TransmittedTime ,@ServiceLevel ,@EstimatedPagesPerPackage ,@EstimatedPackagesDaily  ,@EstimatedPackagesMonthly ,@EstimatedPackagesAnnually ,@DailyPrintSchedule ,@PaperPrintRequirements  ,@PackagesSentToCustomer  ,@IsSampleEnvelopeAttached ,@Envelopes    ,@IsBRERequired ,@BusinessReplyMailEnvelope ,@IsCustomerSupplyEnvelope ,@IsCustomerProvideArtworkEnvelope ,@IsClientPostageAccount  ,@IsScanInAuthoMailDallas,@FilesTransmittedOther ,@TypeOfFilesOther ,@PaperAndPrintDetails ,@ShippingMailingDetail ,@MailingEnvelopeDetail ,@BusinessReplyMailEnvelopeDetail ,@ListOfDocTypesAndPages ,@InsertRequired ,@BookletRequired ,@BusinessReplyMailEnvelopeRequired ,@OtherMaterialRequired ,@InsertMaterialOrderedBy ,@AdditionalInfo ,@CreatedOn ,@CreatedBy  ,@ModifiedOn ,@ModifiedBy,@ServiceLevelOther,@PaperPrintRequirementsOther,@PackagesSentToCustomerOther,@EnvelopesOther,@BusinessReplyMailEnvelopeOther,@FilesTransmittedDetail,@TypeOfFilesDetail);SELECT CAST(SCOPE_IDENTITY() as int)";
                    projectRequest.CreatedBy = projectRequest.ModifiedBy = user;
                    projectRequest.CreatedOn = projectRequest.ModifiedOn = DateTime.UtcNow;
                    Id = db.Query<int>(query, projectRequest).Single();
                }
                else
                {
                    Id = projectRequest.Id;
                    string query = @"UPDATE [AM].[ProjectRequest] SET [ClientName] = @ClientName ,[SubmittedOn] = @SubmittedOn ,[SubmittedBy] = @SubmittedBy ,[UATStartOn] = @UATStartOn ,[ProdStartOn] = @ProdStartOn ,[Description] = @Description ,[FileTransmittedVia] = @FileTransmittedVia ,[NoOfEstSweepTimes] = @NoOfEstSweepTimes ,[ESTSweepTime] = @ESTSweepTime ,[TypeOfFiles] = @TypeOfFiles ,[TransmittedTime] = @TransmittedTime ,[ServiceLevel] = @ServiceLevel ,[EstimatedPagesPerPackage] = @EstimatedPagesPerPackage ,[EstimatedPackagesDaily] = @EstimatedPackagesDaily ,[EstimatedPackagesMonthly] = @EstimatedPackagesMonthly ,[EstimatedPackagesAnnually] = @EstimatedPackagesAnnually ,[DailyPrintSchedule] = @DailyPrintSchedule ,[PaperPrintRequirements] = @PaperPrintRequirements ,[PackagesSentToCustomer] = @PackagesSentToCustomer ,[IsSampleEnvelopeAttached] = @IsSampleEnvelopeAttached ,[Envelopes] = @Envelopes ,[IsBRERequired] = @IsBRERequired ,[BusinessReplyMailEnvelope] = @BusinessReplyMailEnvelope ,[IsCustomerSupplyEnvelope] = @IsCustomerSupplyEnvelope ,[IsCustomerProvideArtworkEnvelope] = @IsCustomerProvideArtworkEnvelope ,[IsClientPostageAccount] = @IsClientPostageAccount ,[IsScanInAuthoMailDallas] = @IsScanInAuthoMailDallas,[FilesTransmittedOther] = @FilesTransmittedOther ,[TypeOfFilesOther] = @TypeOfFilesOther ,[PaperAndPrintDetails] = @PaperAndPrintDetails ,[ShippingMailingDetail] = @ShippingMailingDetail ,[MailingEnvelopeDetail] = @MailingEnvelopeDetail ,[BusinessReplyMailEnvelopeDetail] = @BusinessReplyMailEnvelopeDetail ,[ListOfDocTypesAndPages] = @ListOfDocTypesAndPages ,[InsertRequired] = @InsertRequired ,[BookletRequired] = @BookletRequired ,[BusinessReplyMailEnvelopeRequired] = @BusinessReplyMailEnvelopeRequired ,[OtherMaterialRequired] = @OtherMaterialRequired ,[InsertMaterialOrderedBy] = @InsertMaterialOrderedBy ,[AdditionalInfo] = @AdditionalInfo ,[ModifiedOn] = @ModifiedOn ,[ModifiedBy] = @ModifiedBy,[ServiceLevelOther]=@ServiceLevelOther,[PaperPrintRequirementsOther]=@PaperPrintRequirementsOther,[PackagesSentToCustomerOther]=@PackagesSentToCustomerOther,[EnvelopesOther]=@EnvelopesOther,[BusinessReplyMailEnvelopeOther]=@BusinessReplyMailEnvelopeOther,FilesTransmittedDetail=@FilesTransmittedDetail,TypeOfFilesDetail=@TypeOfFilesDetail WHERE Id=@Id";
                    projectRequest.ModifiedBy = user;
                    projectRequest.ModifiedOn = DateTime.UtcNow;
                    db.Execute(query, projectRequest);
                }
                return new KeyValuePair<bool, string>(true, Id.ToString());
            }
            catch (Exception ex)
            {
                return new KeyValuePair<bool, string>(false, ex.Message);
            }

        }

        public ProjectRequest ProjectRequestFindById(int Id)
        {
            return db.QuerySingle<ProjectRequest>(@"select p.*,c.UserName as CreatedBy,m.UserName as ModifiedBy from AM.ProjectRequest p
inner join profile.users c on p.CreatedBy=c.Id
inner join profile.users m on p.ModifiedBy=m.Id
where p.Id=@Id", new { Id = Id });
        }


        public List<ProjectRequest> GetAllProjectRequest()
        {
            return db.Query<ProjectRequest>("select * from AM.ProjectRequest").ToList();
        }


        public KeyValuePair<bool, string> SaveProjectRequestFiles(int Id, string files, string userId)
        {
            try
            {
                string query = @"update AM.ProjectRequest
set PDFSample=@pdfSample,ModifiedOn=@modifiedOn,ModifiedBy=@modifiedBy
where Id=@id";
                int rows = db.Execute(query, new { id = Id, pdfSample = files, modifiedOn = DateTime.UtcNow, modifiedBy = userId });
                if (rows > 0)
                    return new KeyValuePair<bool, string>(true, "Saved successful.");
                return new KeyValuePair<bool, string>(false, "Services & Cost data not found.");
            }
            catch (Exception ex)
            {
                return new KeyValuePair<bool, string>(false, ex.Message);
            }
        }


        public KeyValuePair<bool, string> RemoveFileName(int Id, string fileName, string userId)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return new KeyValuePair<bool, string>(false, "Invalid file name.");
            var projectRequest = db.QuerySingleOrDefault<string>("select PDFSample from AM.ProjectRequest where Id=@Id", new { Id = Id });
            if (projectRequest == null) return new KeyValuePair<bool, string>(false, "Services & Cost data not found.");
            var fileList = projectRequest.Split(',').ToList();
            fileList.RemoveAll(f => f == fileName);
            string files = fileList == null || fileList.Count == 0 ? null : string.Join(",", fileList);
            return SaveProjectRequestFiles(Id, files, userId);
        }
    }
}
