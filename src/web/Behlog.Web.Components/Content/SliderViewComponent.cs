﻿using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Behlog.Core.Extensions;
using Behlog.Web.ViewModels.Content;
using Behlog.Services.Contracts.Content;

namespace Behlog.Web.Components.Content {

    public class SliderViewComponent: ViewComponent {

        private readonly IPostService _postService;

        public SliderViewComponent(IPostService postService) {
            postService.CheckArgumentIsNull(nameof(postService));
            _postService = postService;
        }

        public async Task<IViewComponentResult> InvokeAsync(
            string lang,
            string postType,
            string slug
            ) {

            var post = await _postService.GetPostFileGalleryAsync(
                lang, 
                postType, 
                slug, 
                isComponent: true);

            if (post == null)
                return await Task.FromResult(
                    Content(string.Empty));

            var result = new SliderViewModel {
                Images = post.Files.Select(_ => new SliderImageViewModel {
                    Title = _.Title,
                    FileId = _.Id,
                    ImagePath = _.FilePath
                }).ToList()
            };

            if(!string.IsNullOrWhiteSpace(post.ViewPath)) 
                return await Task.FromResult(
                    View(post.ViewPath, result));

            return await Task.FromResult(
                View(result));
        }
    }
}
