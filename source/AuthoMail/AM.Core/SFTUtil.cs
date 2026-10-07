using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Renci.SshNet;
using Renci.SshNet.Sftp;
using Renci.SshNet.Common;
using log4net;
using AM.Lib;

namespace AM.Core
{
    public class SFTUtil
    {
        private NetworkCredential Credentials { get; set; }
        private static readonly ILog log = LogManager.GetLogger(typeof(SFTUtil));

        public SFTUtil(SFT_ProtocolType protocol, string hostURL, string username, string password, int keepAliveInterval)
        {
            Protocol = protocol;
            HostURL = hostURL;
            Username = username;
            Password = password;
            Credentials = new NetworkCredential(username, password);
            KeepAliveInterval = keepAliveInterval;
        }

        public SFT_ProtocolType Protocol { get; private set; }
        public string HostURL { get; private set; }
        public string Username { get; private set; }
        public string Password { get; private set; }
        public int KeepAliveInterval { get; set; }

        private string GetUrl(string givenUrl)
        {
            if (givenUrl.IsNullOrWhiteSpace())
            {
                throw new Exception("Invalid URL.");
            }

            if (Protocol == SFT_ProtocolType.ftp)
            {
                if (givenUrl.ToLower().StartsWith("ftp://"))
                {
                    return givenUrl;
                }
                else if (givenUrl.ToLower().StartsWith("ftp:/"))
                {
                    return "ftp://{0}".Fill(givenUrl.Split(":/".ToCharArray())[1]);
                }
                else if (givenUrl.ToLower().StartsWith("//"))
                {
                    return "ftp:{0}".Fill(givenUrl);
                }
                else if (givenUrl.ToLower().StartsWith("/"))
                {
                    return "ftp://{0}{1}".Fill(HostURL, givenUrl);
                }
                else
                {
                    return "ftp://{0}{1}".Fill(HostURL, givenUrl);
                }
            }
            else if (Protocol == SFT_ProtocolType.sftp)
            {
                if (givenUrl.ToLower().StartsWith("//"))
                {
                    return givenUrl.Remove(0, 1);
                }
                else if (givenUrl.ToLower().StartsWith("/"))
                {
                    return givenUrl;
                }
                else
                {
                    throw new Exception("Invalid URL.");
                }
            }
            else
            {
                return givenUrl;
            }
        }

        private FtpWebRequest FTP_WebRequest { get; set; }

        private SftpClient SFTP_Client { get; set; }

        public void Connect()
        {
            if (Protocol == SFT_ProtocolType.ftp)
            {
                //FTP_WebRequest = (FtpWebRequest)FtpWebRequest.CreateDefault(new Uri(GetUrl(HostURL)));
                //FTP_WebRequest.Credentials = Credentials;
                //FTP_WebRequest.UseBinary = true;
                //FTP_WebRequest.UsePassive = true;
            }
            else if (Protocol == SFT_ProtocolType.unc)
            {
                try
                {
                    if (HostURL.IsNullOrWhiteSpace() && !CreateTestFileOnUNCPath())
                    {
                        throw new Exception("Please provide valid Host or root UNC path.");
                    }
                }
                catch (Exception err)
                {
                    throw new Exception("Unable to connect to provided UNC path. \n{0}".Fill(err.ToString()));
                }
            }
            else
            {
                SFTP_Client = new SftpClient(HostURL, Username, Password);
                SFTP_Client.KeepAliveInterval = TimeSpan.FromSeconds(KeepAliveInterval);
                SFTP_Client.ConnectionInfo.Timeout = TimeSpan.FromSeconds(KeepAliveInterval);

                if (!SFTP_Client.IsConnected)
                {
                    try
                    {
                        SFTP_Client.Connect();
                    }
                    catch (SshOperationTimeoutException err)
                    {
                        log.Error(err);
                        SFTP_Client = new SftpClient(HostURL, Username, Password);
                        SFTP_Client.KeepAliveInterval = TimeSpan.FromSeconds(KeepAliveInterval);
                        SFTP_Client.ConnectionInfo.Timeout = TimeSpan.FromSeconds(KeepAliveInterval);
                        SFTP_Client.Connect();
                    }
                }
            }
        }

        private bool CreateTestFileOnUNCPath()
        {
            string randomTestFilePath = Path.Combine(HostURL, Path.GetRandomFileName());
            File.AppendAllText(randomTestFilePath, string.Empty);
            File.Delete(randomTestFilePath);
            return true;
        }

        public bool IsConnected
        {
            get
            {
                if (Protocol == SFT_ProtocolType.sftp)
                    return SFTP_Client.IsConnected;
                else if (Protocol == SFT_ProtocolType.unc)
                    return CreateTestFileOnUNCPath();
                else return true;
            }
        }

        public void DeleteFile(string remoteFilePath)
        {
            if (Protocol == SFT_ProtocolType.ftp)
            {
                FTP_WebRequest = (FtpWebRequest)WebRequest.Create(GetUrl(remoteFilePath));

                FTP_WebRequest.Credentials = Credentials;

                FTP_WebRequest.UseBinary = true;
                FTP_WebRequest.UsePassive = true;

                FTP_WebRequest.Method = WebRequestMethods.Ftp.DeleteFile;

                using (FtpWebResponse FTP_WebResponse = (FtpWebResponse)FTP_WebRequest.GetResponse())
                {
                    string response = FTP_WebResponse.StatusDescription;
                    if (response.Contains("250 File deleted successfully"))
                    {
                        return;
                    }
                    else
                    {
                        throw new Exception("Unable to delete the remote file; please try again.");
                    }
                }
            }
            else if (Protocol == SFT_ProtocolType.unc)
            {
                File.Delete(remoteFilePath);
            }
            else
            {
                if (!SFTP_Client.IsConnected)
                {
                    SFTP_Client.Connect();
                }
                SFTP_Client.DeleteFile(GetUrl(remoteFilePath));
            }
        }

        public bool IsDownloadDirectoryAvailable(string downloadDirectoryPath)
        {
            if (Protocol == SFT_ProtocolType.ftp)
            {
                FTP_WebRequest = (FtpWebRequest)FtpWebRequest.Create(GetUrl(downloadDirectoryPath));
                FTP_WebRequest.Credentials = Credentials;
                FTP_WebRequest.Method = WebRequestMethods.Ftp.ListDirectory;

                try
                {
                    using (FtpWebResponse FTP_WebResponse = (FtpWebResponse)FTP_WebRequest.GetResponse())
                    {
                        return true;
                    }
                }
                catch (WebException)
                {
                    return false;
                }
            }
            else if (Protocol == SFT_ProtocolType.unc)
            {
                return Directory.Exists(downloadDirectoryPath);
            }
            else
            {
                return SFTP_Client.Exists(downloadDirectoryPath);
            }
        }

        public bool IsFileAvailable(string serverFilePath)
        {
            if (Protocol == SFT_ProtocolType.ftp)
            {
                FTP_WebRequest = (FtpWebRequest)FtpWebRequest.Create(GetUrl(serverFilePath));
                FTP_WebRequest.Credentials = Credentials;
                FTP_WebRequest.Method = WebRequestMethods.Ftp.DownloadFile;

                try
                {
                    using (FtpWebResponse FTP_WebResponse = (FtpWebResponse)FTP_WebRequest.GetResponse())
                    {
                        return true;
                    }
                }
                catch (WebException)
                {
                    return false;
                }
            }
            else if (Protocol == SFT_ProtocolType.unc)
            {
                return File.Exists(serverFilePath);
            }
            else
            {
                return SFTP_Client.Exists(serverFilePath);
            }
        }

        public void UploadFile(string localFilePath, string remoteFilePath, bool deleteLocalFile = false, bool closeConnection = false)
        {
            if (Protocol == SFT_ProtocolType.ftp)
            {
                FTP_WebRequest = (FtpWebRequest)FtpWebRequest.Create(GetUrl(remoteFilePath));
                FTP_WebRequest.Credentials = Credentials;
                FTP_WebRequest.Method = WebRequestMethods.Ftp.UploadFile;
                FTP_WebRequest.UseBinary = true;
                FTP_WebRequest.UsePassive = true;

                byte[] data = File.ReadAllBytes(Path.GetFullPath(localFilePath));
                FTP_WebRequest.ContentLength = data.Length;

                Stream stream = FTP_WebRequest.GetRequestStream();
                stream.Write(data, 0, data.Length);
                stream.Close();

                using (FtpWebResponse FTP_WebResponse = (FtpWebResponse)FTP_WebRequest.GetResponse())
                {
                    if (FTP_WebResponse.StatusCode != FtpStatusCode.ClosingData)
                    {
                        Stream dataStream = FTP_WebResponse.GetResponseStream();
                        StreamReader reader = new StreamReader(dataStream);
                        string responseContent = reader.ReadToEnd();
                        throw new Exception(responseContent);
                    }
                }
            }
            else if (Protocol == SFT_ProtocolType.unc)
            {
                File.Copy(localFilePath, remoteFilePath, true);
            }
            else
            {
                if (!SFTP_Client.IsConnected)
                {
                    SFTP_Client.Connect();
                }

                FileStream fsOfFile = File.OpenRead(localFilePath);
                SFTP_Client.UploadFile(fsOfFile, remoteFilePath, true);
                fsOfFile.Close();

                if (closeConnection)
                {
                    SFTP_Client.Disconnect();
                }
            }

            if (deleteLocalFile)
            {
                File.Delete(localFilePath);
            }
        }

        public void DownloadFile(string remoteFilePath, string localFilePath, bool deleteRemoteFile = false, bool closeConnection = false)
        {
            if (Protocol == SFT_ProtocolType.ftp)
            {
                FTP_WebRequest = (FtpWebRequest)FtpWebRequest.Create(GetUrl(remoteFilePath));
                FTP_WebRequest.Credentials = Credentials;
                FTP_WebRequest.Method = WebRequestMethods.Ftp.DownloadFile;

                using (Stream ftpStream = FTP_WebRequest.GetResponse().GetResponseStream())
                using (Stream fileStream = File.Create(localFilePath))
                {
                    ftpStream.CopyTo(fileStream);
                }

                using (FtpWebResponse FTP_WebResponse = (FtpWebResponse)FTP_WebRequest.GetResponse())
                {
                    if (FTP_WebResponse.StatusCode != FtpStatusCode.ClosingData)
                    {
                        Stream dataStream = FTP_WebResponse.GetResponseStream();
                        StreamReader reader = new StreamReader(dataStream);
                        string responseContent = reader.ReadToEnd();
                        throw new Exception(responseContent);
                    }
                }
            }
            else if (Protocol == SFT_ProtocolType.unc)
            {
                File.Copy(remoteFilePath, localFilePath, true);
            }
            else
            {
                if (!SFTP_Client.IsConnected)
                {
                    SFTP_Client.Connect();
                }

                if (SFTP_Client.Get(GetUrl(remoteFilePath)).IsRegularFile)
                {
                    Stream localFileStrm = File.Create(localFilePath);
                    SFTP_Client.DownloadFile(GetUrl(remoteFilePath), localFileStrm);
                    localFileStrm.Close();
                }
                if (closeConnection)
                {
                    SFTP_Client.Disconnect();
                }
            }

            if (deleteRemoteFile)
            {
                DeleteFile(GetUrl(remoteFilePath));
            }
        }

        public void DownloadDirectory(string localDirPath, string remoteDirPath, bool deleteRemoteFile = false, bool closeConnection = false)
        {
            localDirPath.CreateDirectoryIfNotExist();

            if (Protocol == SFT_ProtocolType.ftp)
            {
                DownloadAllFiles(localDirPath, remoteDirPath, deleteRemoteFile);
            }
            else if (Protocol == SFT_ProtocolType.unc)
            {
                List<string> remoteFiles = Directory.GetFiles(remoteDirPath).ToList();
                foreach (string remoteFile in remoteFiles)
                {
                    DownloadFile(remoteFile, Path.Combine(localDirPath, Path.GetFileName(remoteFile)), deleteRemoteFile);
                }
                foreach (string remoteFile in remoteFiles)
                {
                    if (deleteRemoteFile)
                    {
                        File.Delete(remoteFile);
                    }
                }
            }
            else
            {
                if (!SFTP_Client.IsConnected)
                {
                    SFTP_Client.Connect();
                }

                DownloadAllFiles(localDirPath, remoteDirPath, deleteRemoteFile);

                if (closeConnection)
                {
                    SFTP_Client.Disconnect();
                }
            }
        }

        private void DownloadAllFiles(string localDirPath, string remoteDirPath, bool deleteRemoteFile)
        {
            List<string> availableFiles = ListDirectory(remoteDirPath);
            //log.DebugFormat("Downloading started : {0}", DateTime.Now);
            foreach (string remoteFile in availableFiles)
            {
                string localFilePath = Path.Combine(localDirPath, Path.GetFileName(remoteFile));
                DownloadFile(GetUrl("{0}/{1}".Fill(remoteDirPath, remoteFile)), localFilePath, deleteRemoteFile);
            }
            //log.DebugFormat("Downloading completed: {0}", DateTime.Now);
        }

        public DateTime LastModifiedTimeOfFile(string remoteFilePath)
        {
            if (Protocol == SFT_ProtocolType.ftp)
            {
                FTP_WebRequest = (FtpWebRequest)FtpWebRequest.Create(GetUrl(remoteFilePath));
                FTP_WebRequest.Credentials = Credentials;
                FTP_WebRequest.Method = WebRequestMethods.Ftp.GetDateTimestamp;

                using (FtpWebResponse FTP_WebResponse = (FtpWebResponse)FTP_WebRequest.GetResponse())
                {
                    if (FTP_WebResponse.StatusCode != FtpStatusCode.FileStatus)
                    {
                        Stream dataStream = FTP_WebResponse.GetResponseStream();
                        StreamReader reader = new StreamReader(dataStream);
                        string responseContent = reader.ReadToEnd();
                        throw new Exception("Unable to locate the file; please try again.");
                    }

                    return FTP_WebResponse.LastModified.ToUniversalTime();
                }
            }
            else if (Protocol == SFT_ProtocolType.sftp)
            {
                if (!SFTP_Client.IsConnected)
                {
                    SFTP_Client.Connect();
                }

                return SFTP_Client.GetLastWriteTimeUtc(remoteFilePath);
            }
            else
            {
                return File.GetLastWriteTimeUtc(remoteFilePath);
            }
        }

        public List<string> ListDirectory(string remoteUrl)
        {
            if (Protocol == SFT_ProtocolType.ftp)
            {
                FTP_WebRequest = (FtpWebRequest)FtpWebRequest.Create(GetUrl(remoteUrl));
                FTP_WebRequest.Credentials = Credentials;
                FTP_WebRequest.Method = WebRequestMethods.Ftp.ListDirectory;

                using (FtpWebResponse FTP_WebResponse = (FtpWebResponse)FTP_WebRequest.GetResponse())
                {
                    StreamReader streamReader = new StreamReader(FTP_WebResponse.GetResponseStream());
                    List<string> directoryContent = new List<string>();

                    string line = streamReader.ReadLine();
                    while (!string.IsNullOrEmpty(line))
                    {
                        if (line.Contains('/'))
                        {
                            directoryContent.Add(line.Split('/')[1]);
                        }
                        else
                        {
                            directoryContent.Add(line);
                        }
                        line = streamReader.ReadLine();
                    }
                    streamReader.Close();

                    return directoryContent;
                }
            }
            else if (Protocol == SFT_ProtocolType.sftp)
            {
                if (!SFTP_Client.IsConnected)
                {
                    SFTP_Client.Connect();
                }

                List<string> directoryContent = SFTP_Client.ListDirectory(GetUrl(remoteUrl)).Where(f => f.IsRegularFile || f.IsDirectory).Select(f => f.Name).ToList();

                return directoryContent;
            }
            else
            {
                return Directory.GetFileSystemEntries(remoteUrl).Select(f => Path.GetFileName(f)).ToList();
            }
        }

        public void MoveFile(string sourceRemotePath, string destinationRemotePath)
        {
            if (Protocol == SFT_ProtocolType.ftp)
            {
                sourceRemotePath = GetUrl(sourceRemotePath);
                FTP_WebRequest = (FtpWebRequest)FtpWebRequest.Create(sourceRemotePath);
                FTP_WebRequest.UseBinary = true;
                FTP_WebRequest.Method = WebRequestMethods.Ftp.Rename;
                FTP_WebRequest.Credentials = Credentials;
                FTP_WebRequest.RenameTo = destinationRemotePath;

                using (FtpWebResponse FTP_WebResponse = (FtpWebResponse)FTP_WebRequest.GetResponse())
                {
                    if (FTP_WebResponse.StatusCode != FtpStatusCode.FileActionOK)
                    {
                        throw new Exception("Failed to move the remote file.");
                    }
                }
            }
            else if (Protocol == SFT_ProtocolType.unc)
            {
                string destinationRemoteDir = Path.GetDirectoryName(destinationRemotePath);
                if (!Directory.Exists(destinationRemotePath))
                {
                    Directory.CreateDirectory(destinationRemoteDir);
                }
                File.Move(sourceRemotePath, destinationRemotePath);
            }
            else
            {
                if (!SFTP_Client.IsConnected)
                {
                    SFTP_Client.Connect();
                }

                SftpFile sourceFile = SFTP_Client.Get(GetUrl(sourceRemotePath));
                if (sourceFile != null && sourceFile.IsRegularFile)
                {
                    sourceFile.MoveTo(GetUrl(destinationRemotePath));
                }
            }
        }

        public void RenameFile(string OldRemotePath, string NewRemotePath, bool closeConnection = false)
        {
            if (Protocol == SFT_ProtocolType.ftp)
            {
                FTP_WebRequest = (FtpWebRequest)FtpWebRequest.Create(GetUrl(OldRemotePath));
                FTP_WebRequest.Credentials = Credentials;
                FTP_WebRequest.Method = WebRequestMethods.Ftp.Rename;
                FTP_WebRequest.RenameTo = Path.GetFileName(NewRemotePath);
                FTP_WebRequest.UseBinary = true;
                using (FtpWebResponse FTP_WebResponse = (FtpWebResponse)FTP_WebRequest.GetResponse())
                {
                    if (FTP_WebResponse.StatusCode != FtpStatusCode.ClosingData)
                    {
                        Stream dataStream = FTP_WebResponse.GetResponseStream();
                        StreamReader reader = new StreamReader(dataStream);
                        string responseContent = reader.ReadToEnd();
                        dataStream.Close();
                        throw new Exception(responseContent);
                    }
                }
            }
            else if (Protocol == SFT_ProtocolType.unc)
            {
                File.Move(OldRemotePath, NewRemotePath);
            }
            else
            {
                if (!SFTP_Client.IsConnected)
                {
                    SFTP_Client.Connect();
                }

                SFTP_Client.RenameFile(OldRemotePath, NewRemotePath);

                if (closeConnection)
                {
                    SFTP_Client.Disconnect();
                }
            }
        }
        public void Disconnect()
        {
            if (Protocol == SFT_ProtocolType.sftp)
            {
                if (SFTP_Client.IsConnected)
                {
                    SFTP_Client.Disconnect();
                }
            }
        }

    }

    public enum SFT_ProtocolType
    {
        ftp, sftp, unc
    }

}
