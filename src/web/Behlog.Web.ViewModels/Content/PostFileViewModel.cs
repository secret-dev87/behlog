﻿using System.Collections.Generic;
using Behlog.Core.Models.Enum;
using Behlog.Core;
using System.Linq;

namespace Behlog.Web.ViewModels.Content
{
    public class SliderViewModel
    {
        public SliderViewModel() {
            Images = new List<SliderImageViewModel>();
        }

        public IEnumerable<SliderImageViewModel> Images { get; set; }
    }

    public class SliderImageViewModel {
        public long FileId { get; set; }
        public string Title { get; set; }
        public string ImagePath { get; set; }
    }

    public class GalleryViewModel {

        public GalleryViewModel() {
            Posts = new List<PostFileGalleryViewModel>();
            Categories = new List<GalleryCategoryItemViewModel>();
        }

        public string Title { get; set; }
        public IEnumerable<PostFileGalleryViewModel> Posts { get; set; }
        public IEnumerable<GalleryCategoryItemViewModel> Categories { get; set; }
        public List<PostFileGalleryViewModel> ByCategory(int? category) => 
            Posts
            .Where(_ => _.CategoryId == category)
            .ToList();
    }

    public class PostFileGalleryViewModel: PostItemBaseViewModel {
        public PostFileGalleryViewModel() {
            Files = new List<PostFileGalleryItemViewModel>();
        }

        public string AltTitle { get; set; }
        public int PostTypeId { get; set; }
        public int LangId { get; set; }
        public int? CategoryId { get; set; }
        public IEnumerable<PostFileGalleryItemViewModel> Files { get; set; }
        public PostFileGalleryItemViewModel CoverPhotoFile 
            => Files.FirstOrDefault();
        public bool Commented { get; set; }
    }

    public class PostFileGalleryItemViewModel {
        public long Id { get; set; }
        public string Title { get; set; }
        public string FilePath { get; set; }
        public string FilePathDisplay => FilePath != null
            ? FilePath.Replace("~", AppHttpContext.BaseUrl)
            : string.Empty;
        public PostFileType FileType { get; set; }
        public string Extension { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }
    }

    public class GalleryCategoryItemViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Slug { get; set; }
        public string Description { get; set; }
    }
}
