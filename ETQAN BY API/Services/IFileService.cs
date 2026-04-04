using Microsoft.AspNetCore.Http;

namespace ETQAN_BY_API.Services
{
    public interface IFileService
    {
        // دالة لرفع الصورة وترجع مسارها
        Task<string> UploadImageAsync(IFormFile file, string folderName);

        // دالة لحذف الصورة من الهارد ديسك
        void DeleteImage(string imagePath);
    }
}