﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Behlog.Web.ViewModels.Settings
{
    public class DistributedSqlServerCacheOptions
    {
        public string ConnectionString { set; get; }
        public string TableName { set; get; }
        public string SchemaName { set; get; }
    }
}
