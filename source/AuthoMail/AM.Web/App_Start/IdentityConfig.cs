using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using AM.Web.Models;
using AM.Entities;
using AM.Services;
using Microsoft.Owin.Security.DataProtection;
using System.Net.Mail;
using System.IO;
using DocumentFormat.OpenXml.Spreadsheet;

namespace AM.Web
{
    public class EmailService : IIdentityMessageService
    {
        public async Task SendAsync(IdentityMessage message)
        {
            using (MailMessage msg = new MailMessage())
            {
                msg.IsBodyHtml = true;
                msg.To.Add(message.Destination);
                msg.Bcc.Add(Config.Web_RegistrationBCCEmails);
                msg.Subject = message.Subject;
                msg.Body = string.Format("{0}", message.Body);
                SmtpClient sc = new SmtpClient();
                await sc.SendMailAsync(msg);
            }
        }
    }

    public class SmsService : IIdentityMessageService
    {
        public Task SendAsync(IdentityMessage message)
        {
            // Plug in your SMS service here to send a text message.
            return Task.FromResult(0);
        }
    }
    public class InternalEmail
    {
        public static async Task SendMail(string to, string cc, string bcc, string subject, string body)
        {
            using (MailMessage msg = new MailMessage())
            {
                msg.IsBodyHtml = true;
                msg.To.Add(to);
                if (cc != null) msg.CC.Add(cc);
                if (bcc != null) msg.Bcc.Add(bcc);
                msg.Subject = subject;
                msg.Body = string.Format("{0}", body);
                SmtpClient sc = new SmtpClient();
                await sc.SendMailAsync(msg);
            }
        }

        public static async Task SendMail(string to, string subject, string body, byte[] attachmentData, string fileName)
        {
            string tempFilePath = Path.Combine(Path.GetTempPath(), fileName);

            try
            {
                File.WriteAllBytes(tempFilePath, attachmentData);

                using (MailMessage msg = new MailMessage())
                {

                    msg.IsBodyHtml = true;
                    msg.To.Add(to);
                    msg.Subject = subject;
                    msg.Body = string.Format("{0}", body);

                    Attachment attachment = new Attachment(tempFilePath);
                    msg.Attachments.Add(attachment);

                    using (SmtpClient sc = new SmtpClient())
                    {
                        await sc.SendMailAsync(msg);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("An error occurred while sending the email", ex); ;
            }
            finally
            {
                if (File.Exists(tempFilePath))
                {
                    File.Delete(tempFilePath);
                }
            }
        }



    }

    public class AMUserStore : UserStore<ProfileUser>
    {

    }
    // Configure the application user manager used in this application. UserManager is defined in ASP.NET Identity and is used by the application.
    public class AMUserManager : UserManager<ProfileUser>
    {

        public AMUserManager(IUserStore<ProfileUser> store, IDataProtectionProvider dataProtectionProvider)
            : base(store)
        {
            // Configure validation logic for usernames
            UserValidator = new UserValidator<ProfileUser>(this)
            {
                AllowOnlyAlphanumericUserNames = false,
                RequireUniqueEmail = true
            };

            // Configure validation logic for passwords
            PasswordValidator = new PasswordValidator
            {
                RequiredLength = 8,
                RequireNonLetterOrDigit = true,
                RequireDigit = true,
                RequireLowercase = true,
                RequireUppercase = true,
            };

            // Configure user lockout defaults
            UserLockoutEnabledByDefault = true;
            DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(5);
            MaxFailedAccessAttemptsBeforeLockout = 5;


            // Register two factor authentication providers. This application uses Phone and Emails as a step of receiving a code for verifying the user
            // You can write your own provider and plug it in here.
            RegisterTwoFactorProvider("Phone Code", new PhoneNumberTokenProvider<ProfileUser>
            {
                MessageFormat = "Your security code is {0}"
            });
            RegisterTwoFactorProvider("Email Code", new EmailTokenProvider<ProfileUser>
            {
                Subject = "Security Code",
                BodyFormat = "Your security code is {0}"
            });
            EmailService = new EmailService();
            SmsService = new SmsService();

            UserTokenProvider =
                new DataProtectorTokenProvider<ProfileUser>(dataProtectionProvider.Create("ASP.NET Identity"))
                {
                    TokenLifespan = TimeSpan.FromDays(30)
                };

        }
    }

    // Configure the application sign-in manager which is used in this application.
    public class AMSignInManager : SignInManager<ProfileUser, string>
    {
        public AMSignInManager(AMUserManager userManager, IAuthenticationManager authenticationManager)
            : base(userManager, authenticationManager)
        {
        }

        public override Task<ClaimsIdentity> CreateUserIdentityAsync(ProfileUser user)
        {
            return user.GenerateUserIdentityAsync((AMUserManager)UserManager);
        }

        public static AMSignInManager Create(IdentityFactoryOptions<AMSignInManager> options, IOwinContext context)
        {
            return new AMSignInManager(context.GetUserManager<AMUserManager>(), context.Authentication);
        }
        //public override async Task<SignInStatus> PasswordSignInAsync(string userName, string password, bool isPersistent, bool shouldLockout)
        //{
        //    var user = this.UserManager.FindByName(userName);

        //    var result = await base.PasswordSignInAsync(userName, password, isPersistent, shouldLockout);
        //    if (SignInStatus.Success == result)
        //    {
        //        if (DateTime.UtcNow.Subtract(user.ChangePasswordDateUTC.Date).TotalDays == 0)
        //        {
        //            return SignInStatus.Failure;
        //        }
        //    }

        //    return (result);
        //}
    }

    //public class AMRoleManager : RoleManager<IdentityRole>
    //{
    //    public AMRoleManager(IRoleStore<IdentityRole, string> roleStore)
    //        : base(roleStore)
    //    {
    //    }

    //    public static AMRoleManager Create(IdentityFactoryOptions<AMRoleManager> options, IOwinContext context)
    //    {
    //        var manager = new AMRoleManager(new RoleStore<IdentityRole>());
    //        return manager;
    //    }
    //}
}
