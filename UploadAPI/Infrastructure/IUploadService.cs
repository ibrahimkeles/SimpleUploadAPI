using UploadAPI.Models.Dto;
using UploadAPI.Models.Results;

namespace UploadAPI.Infrastructure
{
    public interface IUploadService
    {
        BaseResponse UploadFile(UploadPostDto dto);

    }
}
