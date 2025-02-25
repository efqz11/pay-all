using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Payroll.Filters
{
    public class UrlAttribute : ValidationAttribute
    {
        public UrlAttribute()
        {
        }

        public override bool IsValid(object value)
        {
            var text = value as string;
            Uri uri;

            if (string.IsNullOrWhiteSpace(text))
                return true;

            return (!string.IsNullOrWhiteSpace(text) && Uri.TryCreate(text, UriKind.Absolute, out uri));
        }
    }

    /// <summary>
    /// Selectable fields from Employee and Individual
    /// </summary>
    public class SelectableFieldAttribute : Attribute
    {
    }

    public class NotificaitonAvatarAttribute : Attribute
    {
    }

    public class AuditableEntityAttribute : Attribute
    {
    }

    public class AuditTrailActionAttribute : Attribute
    {
        private string description;

        public AuditTrailActionAttribute(string description)
        {
            this.description = description;
        }

        internal string GetDescription() => description;
    }
}
