﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Behlog.Core.Models.System;

namespace Behlog.Core.Contracts.Repository.System
{
    public interface IWebsiteOptionRepository: IBaseRepository<WebsiteOption, int> {
        Task<IEnumerable<WebsiteOption>> GetEnabledOptions(
            int websiteId, 
            string category = null);

        Task<WebsiteOption> GetEnabledByKey(
            int websiteId, 
            string key);

        Task<WebsiteOption> GetOptionAsync(
            string category,
            string key,
            int? langId);

        Task<WebsiteOption> GetByKeyAsync(
            int websiteId, 
            string key, 
            string lang = null);

    }
}
