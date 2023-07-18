using Microsoft.Extensions.Options;
using System.Net;
using System.Runtime.InteropServices;
using UploadAPI.Extensions;
using UploadAPI.Models.Configs;
using UploadAPI.Models.Dto;
using UploadAPI.Models.Results;

namespace UploadAPI.Infrastructure
{
    public class UploadService : IUploadService
    {
        #region DI
        private readonly IOptions<FtpConfig> _ftpConfig;
        private readonly IOptions<List<FileSizeConfig>> _fileSizeConfig;
        private readonly IOptions<List<string>> _acceptFileConfig;
        private readonly IOptions<List<UploadTypeConfig>> _uploadTypeConfig;
        public UploadService(
            IOptions<FtpConfig> ftpConfig,
            IOptions<List<FileSizeConfig>> fileSizeConfig,
            IOptions<List<string>> acceptFileConfig,
            IOptions<List<UploadTypeConfig>> uploadTypeConfig)
        {
            _ftpConfig = ftpConfig;
            _fileSizeConfig = fileSizeConfig;
            _acceptFileConfig = acceptFileConfig;
            _uploadTypeConfig = uploadTypeConfig;
        }
        #endregion

        public BaseResponse UploadFile(UploadPostDto dto)
        {
            BaseResponse response = CheckModelIsValid(dto);
            if (response.Success)
            {
                try
                {
                    UploadTypeConfig uploadTypeConfig = (UploadTypeConfig)response.Data;
                    string fileName = GenerateUniqueFileName.Generate(dto.File.FileName);
                    string url = $"ftp://{_ftpConfig.Value.Domain}/Uploads/{uploadTypeConfig.FolderName}/{fileName}";
                    FtpWebRequest ftpWebRequest = (FtpWebRequest)WebRequest.Create(url);
                    ftpWebRequest.Method = WebRequestMethods.Ftp.UploadFile;
                    ftpWebRequest.Credentials = new NetworkCredential(_ftpConfig.Value.UserName, _ftpConfig.Value.Password);
                    using (Stream requestStream = ftpWebRequest.GetRequestStream())
                    {
                        dto.File.CopyTo(requestStream);
                        response.Message = "Dosya yükleme işlemi başarılı!";
                        response.Data = $"/Uploads/{uploadTypeConfig.FolderName}/{fileName}";
                    }
                }
                catch (WebException ex)
                {
                    response.Success = false;
                    response.Message = $"Dosya yükleme aşamasında beklenmedik bir hata oluştu! {ex.Message}";
                }
            }
            return response;
        }

        #region Helper Methods
        /// <summary>
        /// Appsettingsden okunan değerlerin null olup olmadığının kontrolü yapılır. True/False olarak geriye dönüş sağlanır.
        /// </summary>
        /// <returns></returns>
        private bool CheckConfigValid()
        {
            if (_ftpConfig.Value is null) return false;
            if (_ftpConfig.Value.Password is null) return false;
            if (_ftpConfig.Value.UserName is null) return false;
            if (_ftpConfig.Value.Domain is null) return false;
            if (_fileSizeConfig.Value is null) return false;
            if (_acceptFileConfig.Value is null) return false;
            if (_uploadTypeConfig.Value is null) return false;
            return true;
        }

        /// <summary>
        /// Appsettingsde yer alan FileMaxSizes içerisinde uzantılara ait maximum dosya büyüklüğü alınır, yüklenecek dosyanın boyutu ile karşılaştırılır. Hatalı ise BaseResponse türünde geriye dönülür.
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        private BaseResponse CheckFileSize(UploadPostDto dto)
        {
            BaseResponse response = new() { Success = true };
            FileSizeConfig? fileSizeConfig = _fileSizeConfig.Value.FirstOrDefault(x => x.Extension == Path.GetExtension(dto.File.FileName).ToLower());

            if (fileSizeConfig == null)
            {
                response.Success = false;
                response.Message = "Beklenmedik hata! Formata uygun dosya boyutu bulunamadı!";
            }
            else
            {
                if (dto.File.Length > fileSizeConfig.Size)
                {
                    response.Success = false;
                    response.Message = "Dosya boyutu fazla lütfen daha düşük bir boyutta dosya yükleyiniz!";
                }
            }
            return response;
        }

        /// <summary>
        /// Appsettingsde yer alan AcceptFiles içerisinde yer alan uzantılar ile yükelenecek dosyanın uzantısı kontrol edilir geriye true/false dönülür.
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        private BaseResponse CheckAcceptedExtension(UploadPostDto dto)
        {
            BaseResponse response = new() { Success = true };
            string? acceptedExtension = _acceptFileConfig.Value.FirstOrDefault(x => x == Path.GetExtension(dto.File.FileName).ToLower());
            if (acceptedExtension == null) { response.Success = false; response.Message = "Yüklemek istenilen dosya desteklenmiyor."; }
            return response;
        }
        /// <summary>
        /// Appsettingsde yer alana UploadTypes içerisinde yer alan UploadType değerine karşılık yüklenecek dosyanın UploadType kontrolü yapılır. Geriye UploadTypeConfig sınıfı döndürülür.
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        private BaseResponse CheckUploadType(UploadPostDto dto)
        {
            BaseResponse response = new() { Success = true };
            UploadTypeConfig? uploadTypeConfig = _uploadTypeConfig.Value.FirstOrDefault(x => x.UploadType == dto.UploadType.ToString());
            if (uploadTypeConfig == null)
            {
                response.Success = false;
                response.Message = "Beklenmedik hata! Lütfen geçerli bir UploadType değeri gönderiniz!";
            }
            else { response.Data = uploadTypeConfig; }
            return response;
        }
        private BaseResponse CheckModelIsValid(UploadPostDto dto)
        {

            BaseResponse response = new() { Success = true };
            if (CheckConfigValid())
            {
                if (dto is null)
                {
                    response.Success = false;
                    response.Message = "Beklenmedik hata! Model boş gönderilemez!";
                }
                else
                {
                    if (string.IsNullOrEmpty(dto.UploadType.ToString()))
                    {
                        response.Success = false;
                        response.Message = "Beklenmedik hata! UploadType Boş geçilemez!";
                    }
                    else
                    {

                        if (dto.File is null)
                        {
                            response.Success = false;
                            response.Message = "Dosya Boş geçilemez!";
                        }
                        else
                        {
                            response = CheckAcceptedExtension(dto);
                            if (response.Success)
                            {
                                response = CheckFileSize(dto);
                                if (response.Success)
                                {
                                    response = CheckUploadType(dto);
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                response.Message = "Configurasyon Hatası";
                response.Success = false;
            }
            return response;
        }

        #endregion
    }
}
