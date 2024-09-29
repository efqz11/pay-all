using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Payroll.Models
{
    public class CompanyFile : Audit
    {
        public int Id { get; set; }
        

        public int CompanyFolderId { get; set; }
        public virtual CompanyFolder CompanyFolder { get; set; }

        [Required]
        [Display(Name = "Document name")]
        public string Name { get; set; }
        public string Description { get; set; }

        public string FileUrl { get; set; }
        public double FileSizeInMb { get; set; }


        public string FileName { get; set; }

        [StringLength(10)]
        public string FileExtension { get; set; }


        public CompanyFileType CompanyFileType { get; set; }
        public string ContentType { get; set; }

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

        public int ContentLength { get; set; }

        public string HtmlTemplate { get; set; }

        public string SignitoryTitle { get; set; }
        public string SignitorySignature { get; set; }

        public bool IsSignatureAvailable { get; set; }
        [Display(Name = "Does this document need to be filled out?")]
        public bool DoesRequireToBeFilled { get; set; }
        public bool IsSignatureSetupCompleted { get; set; }

        public CompanyFileShareType CompanyFileShareType { get; set; }

        [Display(Name = "Apply for Future hires?")]
        public bool ApplyForFutureHires { get; set; }

        [NotMapped]
        public bool ApplyToAll { get; set; }
        [NotMapped]
        public IList<string> ApplyToSelected { get; set; }

        public IList<FillableConfiguration> FillableConfiguration { get; set; }
        public List<CompanyFileShare> CompanyFileShares { get; set; }
        public RecordStatus RecordStatus { get; set; }

        public CompanyFile()
        {
            CompanyFileShares = new List<CompanyFileShare>();
        }
    }

    public class FillableConfiguration
    {
        // element type
        public string elementType { get; set; }
        public int page { get; set; }
        public string name { get; set; }

        public float height { get; set; }
        public float width { get; set; }

        public float x { get; set; }
        public float y { get; set; }

        public string value { get; set; }

        public string placeholder { get; set; }
        public bool required { get; set; }
        public bool multiline { get; set; }
        public bool deleted { get; set; }
        public string icon { get; set; }

        /// <summary>
        /// Client Values (displayed on canvas)
        /// </summary>
        public float canvas_height { get; set; }
        public float canvas_width { get; set; }

        public float canvas_x { get; set; }
        public float canvas_y { get; set; }

    }

}
