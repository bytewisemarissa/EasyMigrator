using System;
using System.Collections.Generic;
using System.Text;

namespace EasyMigrator.Configuration
{
    public class ApplicationConfiguration
    {
        public ApplicationSettings ApplicationSettings { get; set; }
        public ConnectionStrings ConnectionStrings { get; set; }
    }
}
