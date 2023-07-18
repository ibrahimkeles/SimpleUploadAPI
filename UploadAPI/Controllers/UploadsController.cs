using Microsoft.AspNetCore.Mvc;
using UploadAPI.Models.Dto;
using UploadAPI.Infrastructure;

namespace UploadAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UploadsController : ControllerBase
    {
        #region DI
        private readonly IUploadService _uploadService;
        public UploadsController(IUploadService uploadService)
        {
            _uploadService = uploadService;
        }
        #endregion
       
        [HttpPost("UploadFile")]
        public IActionResult UploadFile([FromForm] UploadPostDto uploadPostDto)
        {
            var response = _uploadService.UploadFile(uploadPostDto);
            return Ok(response);
        }
    }
}
