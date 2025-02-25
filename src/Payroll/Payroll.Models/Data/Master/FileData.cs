using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Payroll.Models
{
    public class FileData : Audit
    {
        public int Id { get; set; }



        public bool IsNameChangeable { get; set; }
        public string Name { get; set; }

        public bool IsUploaded { get; set; }

        public FileType FileType { get; set; }

        //public Enums.SecondaryTypes SecondaryType { get; set; }

        /////// <summary>
        /////// We need this as images resides in the backend server not the web server and requires url resolving
        /////// </summary>
        ////[ScaffoldColumn(false)]
        ////public string FilePathResolved { get; }

        //[StringLength(500)]
        //public string ActualFileName  {get;set;}

        public string FileUrl { get; set; }

        [StringLength(10)]
        public string FileExtension { get; set; }

        public string Description { get; set; }

        public string ContentType { get; set; }

        public int ContentLength { get; set; }

        public string GetSize()
        {
            if (FileSizeInMb < 1)
                return Math.Round(ContentLength / 1024f, 2) + " KB";
            return FileSizeInMb + " MB";
        }

        public string GetRelevantIconString()
        {
            if (ContentType.Contains("image")) return "file-image";
            if (ContentType.Contains("audio")) return "file-music";
            if (ContentType.Contains("video")) return "file-video";
            if (FileExtension == ".xls") return "file-spreadsheet";
            if (FileExtension.Contains(".pdf")) return "file-pdf";
            if (FileExtension.Contains(".csv")) return "file-csv";

            return "file";
        }


        public int CompanyId { get; set; }
        public virtual Company Company { get; set; }
        

        public int? LabelId { get; set; }
        public virtual Label Label { get; set; }

        public int? WorkItemId { get; set; }
        public WorkItem WorkItem { get; set; }

        public int? RequestId { get; set; }
        public Request Request { get; set; }

        public int? EmployeeId { get; set; }
        public Employee Employee { get; set; }

        public int? AnnouncementId { get; set; }
        public Announcement Announcement { get; set; }
        

        public int DisplayOrder { get; set; }


        [NotMapped]
        public IFormFile IFormFile { get; set; }

        public bool IsSignatureAvailable { get; set; }
        public bool IsSignatureSetupCompleted { get; set; }
        public double FileSizeInMb { get;  set; }
        public string FileName { get;  set; }

        public FileData()
        {
            IsNameChangeable = true;
        }
    }
}
