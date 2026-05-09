using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Rentora.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rentora.Infrastructure.Services
{
    public class LocalFileStorageService : IFileStorageService
    {
        private readonly IWebHostEnvironment _env;

        // IWebHostEnvironment gives us the absolute path to your local API folder
        public LocalFileStorageService(IWebHostEnvironment env)
        {
            _env = env;
        }

        public async Task<string> SaveFileAsync(IFormFile file, string folderName)
        {
            if (file == null || file.Length == 0)
                return null;

            // 1. Determine exactly where to save it (e.g., C:\YourProject\Rentora.API\wwwroot\uploads\receipts)
            string webRootPath = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            string uploadPath = Path.Combine(webRootPath, "uploads", folderName);

            // 2. If the folder doesn't exist, create it automatically
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            // 3. Generate a completely unique filename (e.g., "a8f3b-receipt.jpg")
            string fileExtension = Path.GetExtension(file.FileName);
            string uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";

            // 4. Save the actual physical file to your laptop
            string fullPhysicalPath = Path.Combine(uploadPath, uniqueFileName);
            using (var fileStream = new FileStream(fullPhysicalPath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            // 5. Return the URL so it can be saved in the SQL database
            return $"/uploads/{folderName}/{uniqueFileName}";
        }
    }
}
