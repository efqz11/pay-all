using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Payroll.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Payroll.Models
{

    public class SearchResult
    {
        public object Id { get; set; }
        

        public string Name { get; set; }
        public string Avatar { get; set; }
        public string Icon { get; set; }
        
        public EntityType EntityType { get; set; }

        public string RelativeUrl { get; set; }
        public string Summary { get; set; }

        /// <summary>
        /// Create with fontAwesome Icons
        /// </summary>
        /// <param name="name"></param>
        /// <param name="icon"></param>
        /// <param name="EntityType"></param>
        /// <param name="relativeUrl"></param>
        public SearchResult(string name, string icon, EntityType EntityType, string relativeUrl)
        {
            this.Name = name;
            this.EntityType = EntityType;
            this.Icon = icon;
            this.RelativeUrl = relativeUrl;
        }

        /// <summary>
        /// Create with summary and photo
        /// </summary>
        /// <param name="id"></param>
        /// <param name="avatar"></param>
        /// <param name="name"></param>
        /// <param name="relativeUrl"></param>
        /// <param name="jobTitle"></param>
        /// <param name="entityType"></param>
        public SearchResult(object id, string avatar, string name, string relativeUrl, string jobTitle, EntityType entityType)
        {
            this.Id = id;
            this.Avatar = avatar;
            this.Name = name;
            this.RelativeUrl = relativeUrl;
            this.Summary = jobTitle;
            this.EntityType = entityType;
        }
    }
    
}

