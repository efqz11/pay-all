using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Payroll.Services
{
    public class FileUploadService
    {
        private string bucketName = "payrol";
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly ILogger<FileUploadService> logger;
        private readonly IAmazonS3 amazonS3;
        private readonly UserResolverService userResolverService;

        public FileUploadService(IHttpContextAccessor httpContextAccessor, ILogger<FileUploadService> logger, IAmazonS3 amazonS3, UserResolverService userResolverService)
        {
            this.httpContextAccessor = httpContextAccessor;
            this.logger = logger;
            this.amazonS3 = amazonS3;
            this.userResolverService = userResolverService;
        }

        public bool HasFilesReadyForUpload(List<IFormFile> files)
        {
            return (files?.Count > 0 && files[0].Length > 0);
        }

        public bool HasFilesReadyForUpload(string base64Image)
        {
            return !string.IsNullOrEmpty(base64Image) && base64Image.Count() > 50;
        }

        public bool HasFilesReadyForUpload(IFormFile files)
        {
            return (files?.Length > 0);
        }

        public double GetFileSizeInMb(IFormFile files)
        {
            logger.LogWarning($"File uploaded size: {(files.Length / 1024) / 1024}MB");
            return (files.Length / 1024f) / 1024f;
        }

        public double GetFileSizeInMb(string base64image)
        {
            var stringLength = base64image.Length - "data:image/png;base64,".Length;
            var sizeInBytes = 4 * Math.Ceiling((stringLength / 3f)) * 0.5624896334383812;
            return (sizeInBytes / 1024f) / 1024f;
        }

        private string GetFileExtension(string base64String)
        {
            var data = base64String.Substring(0, 5);

            switch (data.ToUpper())
            {
                case "IVBOR":
                    return ".png";
                case "/9J/4":
                    return ".jpg";
                case "AAAAF":
                    return ".mp4";
                case "JVBER":
                    return ".pdf";
                case "AAABA":
                    return ".ico";
                case "UMFYI":
                    return ".rar";
                case "E1XYD":
                    return ".rtf";
                case "U1PKC":
                    return ".txt";
                case "MQOWM":
                case "77U/M":
                    return ".srt";
                default:
                    return string.Empty;
            }
        }

        public bool IsAllowedFileType(string base64String, string types)
        {
            if (string.IsNullOrWhiteSpace(base64String))
                return false;
            var ext = GetFileExtension(base64String);
            logger.LogWarning($"File type detected is {ext}");
            return types.Split(",").Contains(ext);
        }

        public bool IsAllowedFileType(IFormFile files, string types)
        {
            if (string.IsNullOrWhiteSpace(types))
                return false;
            var ext = Path.GetExtension(files.FileName).ToLower();
            return types.Split(",").Contains(ext);
        }

        public decimal GetFileSizeInKb(IFormFile files)
        {
            logger.LogWarning($"File uploaded size: {(files.Length / 1024)}MB");
            return (files.Length / 1024);
        }

        /// <summary>
        /// Upload Fles To S3 Async
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public async Task<string> UploadFles(IFormFile file, string folder = "images")
        {
            logger.LogWarning($"AWS Client was opened in region {Amazon.RegionEndpoint.APSoutheast1.ToString()}.");
            // arn:aws:s3:::files-documents

            try
            {
                var fileName = Path.GetFileName(file.FileName);


                PutObjectRequest putRequest = new PutObjectRequest
                {
                    BucketName = bucketName,
                    //Key = "images/" + fileName,
                    Key = folder + "/" + fileName,
                    ContentType = file.ContentType
                };

                PutObjectResponse response = null;
                using (var newMemoryStream = new MemoryStream())
                {
                    await file.CopyToAsync(newMemoryStream);
                    putRequest.InputStream = newMemoryStream;
                    response = await amazonS3.PutObjectAsync(putRequest);
                }

                logger.LogWarning("Response receieved from AWS PutObjectAsync with response", response, putRequest);
                var url= "https://" + bucketName + ".s3-" + Amazon.RegionEndpoint.APSoutheast1.SystemName + ".amazonaws.com/" + putRequest.Key;
                logger.LogWarning("New url to s3 bucked image link created: ", url);

                return url;
            }
            catch (AmazonS3Exception amazonS3Exception)
            {
                logger.LogError("Amazon Exception during file upload to S3", amazonS3Exception);
                if (amazonS3Exception.ErrorCode != null &&
                    (amazonS3Exception.ErrorCode.Equals("InvalidAccessKeyId")
                    ||
                    amazonS3Exception.ErrorCode.Equals("InvalidSecurity")))
                {
                    throw new Exception("Check the provided AWS Credentials.");
                }
                else
                {
                    throw new Exception("Error occurred: " + amazonS3Exception.Message);
                }
            }

            catch (Exception ex)
            {
                logger.LogError("Exception during file upload to S3", ex);
                throw ex;
            }
        }


        /// <summary>
        /// Upload file from base64 image
        /// </summary>
        /// <param name="base64Image"></param>
        /// <param name="folder"></param>
        /// <returns></returns>
        public async Task<string> UploadFles(string base64Image, string folder = "images")
        {
            logger.LogWarning($"AWS Client was opened in region {Amazon.RegionEndpoint.APSoutheast1.ToString()}.");
            // arn:aws:s3:::files-documents

            try
            {
                base64Image = base64Image.Split(',')[1];
                byte[] bytes = Convert.FromBase64String(base64Image);
                

                var fileName = Guid.NewGuid() + ".jpg";
                

                PutObjectRequest putRequest = new PutObjectRequest
                {
                    BucketName = bucketName,
                    //Key = "images/" + fileName,
                    Key = folder + "/" + fileName,
                    ContentType = "image/jpeg",
                    
                };

                PutObjectResponse response = null;
                using (var ms = new MemoryStream(bytes, 0, bytes.Length, false))
                {
                    putRequest.InputStream = ms;
                    response = await amazonS3.PutObjectAsync(putRequest);
                }
                 

                logger.LogWarning("Response receieved from AWS PutObjectAsync with response", response, putRequest);
                var url = "https://" + bucketName + ".s3-" + Amazon.RegionEndpoint.APSoutheast1.SystemName + ".amazonaws.com/" + putRequest.Key;
                logger.LogWarning("New url to s3 bucked image link created: ", url);

                return url;
            }
            catch (AmazonS3Exception amazonS3Exception)
            {
                logger.LogError("Exception during file upload to S3", amazonS3Exception);
                if (amazonS3Exception.ErrorCode != null &&
                    (amazonS3Exception.ErrorCode.Equals("InvalidAccessKeyId")
                    ||
                    amazonS3Exception.ErrorCode.Equals("InvalidSecurity")))
                {
                    throw new Exception("Check the provided AWS Credentials.");
                }
                else
                {
                    throw new Exception("Error occurred: " + amazonS3Exception.Message);
                }
            }
            catch (Exception e)
            {
                throw new Exception(
                    $"Unknown encountered on server. Message:'{e.Message}' when writing an object");
            }

        }
    }
}
