using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EasyMigrator.Data.Models
{
    public class EasyMigrationLog
    {
        public int MigrationId { get; set; }
        public int Serial { get; set; }
        [MaxLength(256)]
        public string ImplementationNamespace { get; set; }
        public DateTime PerformedOn { get; set; }
    }
}
