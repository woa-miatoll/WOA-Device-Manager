using SharpCompress.Archives;
using SharpCompress.Archives.SevenZip;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Windows.Storage;
using WOADeviceManager.Managers.Connectivity;

namespace WOADeviceManager.Managers
{
    public class DriverManager
    {
        public static async Task<bool> UpdateDrivers(string DriverRepo = null)
        {
            if (!DeviceManager.Device.IsInMassStorage || string.IsNullOrEmpty(DeviceManager.Device.MassStorage.Drive))
            {
                return false;
            }

            MainPage.SetStatus("Initializing", Title: "Servicing Windows Drivers", SubTitle: "WOA Device Manager is preparing your phone to be serviced with the latest drivers available for it. This may take a while.", Emoji: "🪟");

            if (string.IsNullOrEmpty(DriverRepo))
            {
                MainPage.SetStatus("Downloading latest Driver Package for your device...", Title: "Servicing Windows Drivers", SubTitle: "WOA Device Manager is preparing your phone to be serviced with the latest drivers available for it. This may take a while.", Emoji: "🪟");

                if (DeviceManager.Device.Product == DeviceProduct.Miatoll)
                {
                    StorageFile driverPackage = await ResourcesManager.RetrieveFile(ResourcesManager.DownloadableComponent.DRIVERS_MIATOLL, true);
                    if (driverPackage == null)
                    {
                        MainPage.SetStatus();
                        return false;
                    }

                    MainPage.SetStatus("Preparing to extract Driver Package...", Title: "Servicing Windows Drivers", SubTitle: "WOA Device Manager is preparing your phone to be serviced with the latest drivers available for it. This may take a while.", Emoji: "🪟");

                    string destinationPath = Path.Combine((await driverPackage.GetParentAsync()).Path, Path.GetFileNameWithoutExtension(driverPackage.Name));
                    if (Directory.Exists(destinationPath))
                    {
                        Directory.Delete(destinationPath, true);
                    }
                    Directory.CreateDirectory(destinationPath);
                    ZipFile.ExtractToDirectory(driverPackage.Path, destinationPath, true);

                    DriverRepo = destinationPath;
                }
                else
                {
                    throw new Exception("Unknown device product");
                }
            }

            MainPage.SetStatus("Preparing to install Driver Package...", Title: "Servicing Windows Drivers", SubTitle: "WOA Device Manager is preparing your phone to be serviced with the latest drivers available for it. This may take a while.", Emoji: "🪟");

            string DriverDefinitions;

            if (DeviceManager.Device.Product == DeviceProduct.Miatoll)
            {
                DriverDefinitions = $"{DriverRepo}\\definitions\\Desktop\\ARM64\\Internal\\miatoll.xml";
            }
            else
            {
                throw new Exception("Unknown device product");
            }

            string PROCESSOR_ARCHITECTURE = Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE");

            Process process = new();
            process.StartInfo.FileName = $"{DriverRepo}\\tools\\DriverUpdater\\{PROCESSOR_ARCHITECTURE}\\DriverUpdater.exe";
            process.StartInfo.UseShellExecute = true;
            process.StartInfo.Verb = "runas";
            process.StartInfo.Arguments = $"-r \"{DriverRepo}\" -d \"{DriverDefinitions}\" -p {DeviceManager.Device.MassStorage.Drive}";

            MainPage.SetStatus("Installing Driver Package...", Title: "Servicing Windows Drivers", SubTitle: "WOA Device Manager is preparing your phone to be serviced with the latest drivers available for it. This may take a while.", Emoji: "🪟");

            try
            {
                process.Start();
                process.WaitForExit();

                MainPage.SetStatus();
                return process.ExitCode == 0;
            }
            catch
            {
                MainPage.SetStatus();
                return false;
            }
        }
    }
}
