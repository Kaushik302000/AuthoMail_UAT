using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using AM.Services.Properties;
using AM.Lib;

namespace AM.Services
{
    public interface IConfigService : IDisposable
    {
        List<AMSetting> GetSystemSettings();
        int InsertSystemSetting(AMSetting amSetting);
        int UpdateSystemSetting(AMSetting amSetting);
        void SeedAMSettings();
        int UpdateValueOrDescription(string userId, string settingId, string value, string description);
        int ResetValueSystemSetting(string userId, string settingId);
    }
    public class ConfigService : ServiceBase, IConfigService
    {
        public List<AMSetting> GetSystemSettings()
        {
            string qry = string.Format("SELECT [Id],[DataType],[Value],[Description],[IsSystem],[DefaultValue],[ModifiedBy],[ModifiedOn] FROM [Common].[SystemSettings]");
            return db.Query<AMSetting>(qry).ToList();
        }

        public int InsertSystemSetting(AMSetting amSetting)
        {
            var query = "INSERT INTO [Common].[SystemSettings] ([Id],[DataType],[Value],[Description],[IsSystem],[DefaultValue],[ModifiedBy],[ModifiedOn]) VALUES (@Id,@DataType,@Value,@Description,@IsSystem,@DefaultValue,(select Id from [Profile].Users where UserName = 'system@mtc.com'),getutcdate())";
            return db.Execute(query, amSetting);
        }

        public int UpdateSystemSetting(AMSetting amSetting)
        {
            var query = "UPDATE [Common].[SystemSettings] SET [DataType] = @DataType,[Value] = @Value,[Description] = @Description,[ModifiedOn] = @ModifiedOn WHERE [Id] = @Id";
            return db.Execute(query, amSetting);
        }

        public void SeedAMSettings()
        {
            db.Execute(Resources.SeedAMSeetings);
        }

        public int UpdateValueOrDescription(string userId, string settingId, string value, string description)
        {
            if (!value.IsNullOrWhiteSpace() && !description.IsNullOrWhiteSpace())
            {
                string query = "UPDATE [Common].[SystemSettings] SET Value=@value,[Description]=@description,ModifiedBy=@modifiedBy,ModifiedOn=@modifiedOn WHERE Id=@id";
                return db.Execute(query, new { id = @settingId, value = value, description = description, modifiedBy = userId, modifiedOn = DateTime.UtcNow });
            }
            if (!value.IsNullOrWhiteSpace())
            {
                string query = "UPDATE [Common].[SystemSettings] SET Value=@value,ModifiedBy=@modifiedBy,ModifiedOn=@modifiedOn WHERE Id=@id";
                return db.Execute(query, new { id = @settingId, value = value, modifiedBy = userId, modifiedOn = DateTime.UtcNow });
            }
            if (!description.IsNullOrWhiteSpace())
            {
                string query = "UPDATE [Common].[SystemSettings] SET [Description]=@description,ModifiedBy=@modifiedBy,ModifiedOn=@modifiedOn WHERE Id=@id";
                return db.Execute(query, new { id = @settingId, description = description, modifiedBy = userId, modifiedOn = DateTime.UtcNow });
            }
            return 0;
        }

        public int ResetValueSystemSetting(string userId, string settingId)
        {
            string query = "UPDATE [Common].[SystemSettings] SET Value=DefaultValue,ModifiedBy=@modifiedBy,ModifiedOn=@modifiedOn WHERE Id=@id";
            return db.Execute(query, new { id = @settingId, modifiedBy = userId, modifiedOn = DateTime.UtcNow });
        }

    }
}
