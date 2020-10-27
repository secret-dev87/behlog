﻿using System.Threading.Tasks;
using Behlog.Core.Models.Content;
using Behlog.Services.Dto.Content;

namespace Behlog.Factories.Contracts.Content
{
    public interface IFileFactory
    {
        Task<File> MakeAsync(CreateFileDto model);
    }
}
