using CloudinaryDotNet.Actions;

namespace API.Interfaces
{
    public interface IPhotoService
    {
        Task<ImageUploadResult> AddPhotoAsync(IFormFile file);//Task async memthod
        Task<DeletionResult> DeletePhotoAsync(string publicId);
    }
}