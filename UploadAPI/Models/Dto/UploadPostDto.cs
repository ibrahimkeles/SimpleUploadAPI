
namespace UploadAPI.Models.Dto
{
    public class UploadPostDto
    {
        public string UploadType { get; set; }
        public IFormFile? File { get; set; }
    }
}
