using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.Web;
using WOADeviceManager.Helpers;

namespace WOADeviceManager.Managers
{
    public class ResourcesManager
    {
        public enum DownloadableComponent
        {
            DRIVERS_MIATOLL,
            PARTED,
            TWRP_MIATOLL_FBEV1,
            TWRP_MIATOLL_FBEV2,
            UEFI_MIATOLL,
            UEFI_SECUREBOOT_DISABLED_MIATOLL,
        }

        public static async Task<StorageFile> RetrieveFile(DownloadableComponent component, bool redownload = false)
        {
            string downloadPath = string.Empty;
            string fileName = string.Empty;
            string releaseVersion = string.Empty;

            switch (component)
            {
                case DownloadableComponent.PARTED:
                    downloadPath = "https://github.com/WOA-Project/SurfaceDuo-Guides/raw/main/Files/parted";
                    fileName = "parted";
                    break;

                case DownloadableComponent.TWRP_MIATOLL_FBEV1:
                    downloadPath = "https://github.com/woa-miatoll/Port-Windows-11-Redmi-Note-9-Pro/releases/download/Recoveries/OrangeFox-Miatoll-Mod-FBEv1.img";
                    fileName = "OrangeFox-Miatoll-Mod-FBEv1.img";
                    break;
                case DownloadableComponent.TWRP_MIATOLL_FBEV2:
                    downloadPath = "https://github.com/woa-miatoll/Port-Windows-11-Redmi-Note-9-Pro/releases/download/Recoveries/OrangeFox-Miatoll-Mod-FBEv2.img";
                    fileName = "OrangeFox-Miatoll-Mod-FBEv2.img";
                    break;
                case DownloadableComponent.UEFI_MIATOLL:
                    releaseVersion = await HttpsUtils.GetLatestBSPReleaseVersion();
                    downloadPath = $"https://github.com/woa-miatoll/Miatoll-Releases/releases/download/{releaseVersion}/Miatoll-UEFI-v{releaseVersion}.img";
                    fileName = $"Miatoll-UEFI-v{releaseVersion}.img";
                    break;
                case DownloadableComponent.UEFI_SECUREBOOT_DISABLED_MIATOLL:
                    releaseVersion = await HttpsUtils.GetLatestBSPReleaseVersion();
                    downloadPath = $"https://github.com/woa-miatoll/Miatoll-Releases/releases/download/{releaseVersion}/Miatoll-UEFI-v{releaseVersion}-Secure-Boot-Disabled.img";
                    fileName = $"Miatoll-UEFI-v{releaseVersion}-Secure-Boot-Disabled.img";
                    break;
                case DownloadableComponent.DRIVERS_MIATOLL:
                    releaseVersion = await HttpsUtils.GetLatestBSPReleaseVersion();
                    downloadPath = $"https://github.com/woa-miatoll/Miatoll-Releases/releases/download/{releaseVersion}/Miatoll-Drivers-v{releaseVersion}.zip";
                    fileName = $"Miatoll-Drivers.zip";
                    break;
            }
            return await RetrieveFile(downloadPath, fileName, redownload);
        }

        public static async Task<StorageFile> RetrieveFile(string path, string fileName, bool redownload = false)
        {
            if (redownload || !IsFileAlreadyDownloaded(fileName))
            {
                StorageFile file = await ApplicationData.Current.LocalFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
                using HttpClient client = new();
                using Task<Stream> webStream = client.GetStreamAsync(new Uri(path));
                using FileStream fs = new(file.Path, FileMode.OpenOrCreate);
                webStream.Result.CopyTo(fs);
                return file;
            }
            else
            {
                return await ApplicationData.Current.LocalFolder.GetFileAsync(fileName);
            }
        }

        public static bool IsFileAlreadyDownloaded(string fileName)
        {
            return File.Exists(ApplicationData.Current.LocalFolder.Path + "\\" + fileName);
        }
    }
}
