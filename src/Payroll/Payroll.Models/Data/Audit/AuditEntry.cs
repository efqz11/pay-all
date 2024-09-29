using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Newtonsoft.Json;
using Payroll.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Payroll.Models
{
    /// <summary>
    /// Data Changes To Audit Table
    /// An Entity Changes To An Audit Table Entity
    /// Creating a helper class to map all data changes from a DB entity and create an Audit log entity using those change pieces of information.Here, we are using JSON serializer to specify column value related changes.
    /// </summary>
    public class AuditEntry
    {
        public readonly UserResolverService userResolverService;

        public EntityEntry Entry { get; }
        public AuditAction AuditAction { get; set; }
        public string TableName { get; set; }
        public string FullContext { get; set; }
        public string Context => FullContext.Replace("Payroll.Database.", "");

        public string FullModelName { get; set; }
        public string ModelName => FullModelName.Replace("Payroll.Models.", "");
        public string Message { get; set; }
        public string Status { get; set; }
        public string KeyId { get; set; }
        public Dictionary<string, object> KeyValues { get; } = new Dictionary<string, object>();
        public Dictionary<string, object> OldValues { get; } = new Dictionary<string, object>();
        public Dictionary<string, object> NewValues { get; } = new Dictionary<string, object>();
        public List<string> ChangedColumns { get; } = new List<string>();

        public AuditEntry(EntityEntry entry, UserResolverService userResolverService)
        {
            Entry = entry;
            this.userResolverService = userResolverService;
            SetChanges();
        }

        private void SetChanges()
        {
            TableName = Entry.Metadata.GetTableName(); //.Relational().TableName;
            FullModelName = Entry.Entity.ToString();
            FullContext = Entry.Context.ToString();

            foreach (PropertyEntry property in Entry.Properties)
            {
                string propertyName = property.Metadata.Name;
                string dbColumnName = property.Metadata.GetColumnName(); //.Relational().ColumnName;

                if (propertyName == "Status")
                    Status = property.CurrentValue?.ToString() ?? "";


                //if (property.Metadata.IsShadowProperty)
                //    continue;
                if (property.Metadata.IsPrimaryKey())
                {
                    KeyValues[propertyName] = property.CurrentValue;
                    KeyId = property.CurrentValue.ToString();
                    continue;
                }


                switch (Entry.State)
                {
                    case EntityState.Added:
                        NewValues[propertyName] = property.CurrentValue;
                        AuditAction = AuditAction.Create;
                        Message = $"New {ModelName} was Created";
                        break;

                    case EntityState.Deleted:
                        OldValues[propertyName] = property.OriginalValue;
                        AuditAction = AuditAction.Delete;
                        Message = $"{ModelName} was Removed";
                        break;

                    case EntityState.Modified:
                        if (property.IsModified && property.OriginalValue != null && property.CurrentValue != null 
                            &&  !property.OriginalValue.Equals(property.CurrentValue))
                        {
                            ChangedColumns.Add(dbColumnName);

                            OldValues[propertyName] = property.OriginalValue;
                            NewValues[propertyName] = property.CurrentValue;
                            AuditAction = AuditAction.Update;

                            if (NewValues.ContainsKey("Status"))
                                Message = $"{ModelName} was {NewValues["Status"]}";
                        }

                        var actionDesc = userResolverService.GeAuditTrailtActionDescription();
                        if (!string.IsNullOrEmpty(actionDesc))
                            Message = $"{ModelName} {actionDesc} was Updated";

                        else if(string.IsNullOrWhiteSpace(Message))
                            Message = $"{ModelName} was Updated";

                        
                        break;
                }
            }
        }

        public AuditLog ToAudit()
        {
            var audit = new AuditLog();
            //audit.Id = Guid.NewGuid();
            audit.AuditDateTimeUtc = DateTime.UtcNow;
            audit.AuditAction = AuditAction;
            audit.AuditUser = userResolverService.GetUserName();
            audit.AuditUserId = userResolverService.GetUserId();
            audit.AuditUserRoles = userResolverService.Get("AccessGrant.Roles");
            audit.TableName = TableName;
            audit.ModelName = ModelName;
            audit.FullModelName = FullModelName;

            audit.FullContextName = FullContext;
            audit.ContextName = Context;
            audit.Message = Message;
            audit.Status = Status;

            audit.CompanyId = userResolverService.GetCompanyId();
            audit.EmployeeId = userResolverService.GetEmployeeId();

            audit.KeyId = KeyId;
            audit.KeyValues = JsonConvert.SerializeObject(KeyValues);
            audit.OldValues = OldValues.Count == 0 ?
                              null : JsonConvert.SerializeObject(OldValues);
            audit.NewValues = NewValues.Count == 0 ?
                              null : JsonConvert.SerializeObject(NewValues);
            audit.ChangedColumns = ChangedColumns.Count == 0 ?
                                   null : JsonConvert.SerializeObject(ChangedColumns);

            return audit;
        }
    }

}
