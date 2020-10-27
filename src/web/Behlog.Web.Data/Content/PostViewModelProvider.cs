﻿using System;
using System.Threading.Tasks;
using Behlog.Core.Extensions;
using Behlog.Core.Models.Enum;
using Behlog.Core.Models.System;
using Behlog.Factories.Contracts.System;
using Behlog.Services.Contracts.Content;
using Behlog.Web.Data.System;
using Behlog.Web.ViewModels.Content;
using Mapster;

namespace Behlog.Web.Data.Content
{
    public class PostViewModelProvider
    {
        private readonly LanguageViewModelProvider _languageViewModelProvider;
        private readonly IPostTypeService _postTypeService;
        private readonly IPostService _postService;
        private readonly CategoryViewModelProvider _categoryProvider;

        public PostViewModelProvider(
            LanguageViewModelProvider languageProvider,
            IPostTypeService postTypeService,
            IPostService postService,
            CategoryViewModelProvider categoryProvider
        ) {

            postTypeService.CheckArgumentIsNull();
            _postTypeService = postTypeService;

            languageProvider.CheckArgumentIsNull();
            _languageViewModelProvider = languageProvider;

            postService.CheckArgumentIsNull();
            _postService = postService;

            categoryProvider.CheckArgumentIsNull();
            _categoryProvider = categoryProvider;
        }

        /// <summary>
        /// Set base collection's data for <see cref="PostEditViewModel"/> model.
        /// </summary>
        /// <param name="model"></param
        private async Task setBaseDataAsync(PostEditViewModel model) {
            model.LanguageSelectList = await _languageViewModelProvider
                .GetSelectListAsync(model.LangId);
            model.CategorySelectList = await _categoryProvider
                .GetSelectListAsync(model.PostTypeId,
                    model.LangId,
                    model.CategoryId
                );
        }

        /// <summary>
        /// Set base collection's data for <see cref="PostCreateViewModel"/> model.
        /// </summary>
        /// <param name="model"></param>
        private async Task setBaseDataAsync(PostCreateViewModel model) {
            int? langId = null;
            if (model.LangId > 0)
                langId = model.LangId;
            model.LanguageSelectList = await _languageViewModelProvider
                .GetSelectListAsync(langId);
            if(!langId.HasValue) {
                model.LangId = int.Parse(model.LanguageSelectList[0].Value);
            }

            int? categoryId = null;
            if (model.CategoryId > 0)
                categoryId = model.CategoryId;
            model.CategorySelectList = await _categoryProvider
                .GetSelectListAsync(
                    model.PostTypeId, 
                    model.LangId, 
                    selectedCategoryId: categoryId
                );
        }

        /// <summary>
        /// Build <see cref="PostCreateViewModel"/> base on PostType.
        /// </summary>
        /// <param name="postType">PostType to load base data for Post.</param>
        /// <returns>A <see cref="PostCreateViewModel"/> populated with base data.</returns>
        public async Task<PostCreateViewModel> BuildCreateViewModelAsync(
            string postType
        ) {
            var postTypeRes = await _postTypeService.GetBySlugAsync(postType);
            if (postTypeRes == null)
                throw new NullReferenceException(nameof(PostType));

            var model = new PostCreateViewModel {
                Status = PostStatus.Draft,
                //PostTypeTitle = postTypeRes.Title,
                PostTypeSlug = postTypeRes.Slug,
                PostTypeId = postTypeRes.Id
            };

            await setBaseDataAsync(model);
            model.CategorySelectList = await _categoryProvider
                .GetSelectListAsync(postTypeRes.Id, model.LangId);

            return await Task.FromResult(model);
        }

        /// <summary>
        /// Build <see cref="PostCreateViewModel"/> when post back to Create page.
        /// </summary>
        /// <param name="model"></param>
        public async Task<PostCreateViewModel> BuildCreateViewModelAsync(PostCreateViewModel model) {
            model.LanguageSelectList = await _languageViewModelProvider
                .GetSelectListAsync(model.LangId);

            model.CategorySelectList = await _categoryProvider
                .GetSelectListAsync(
                    model.PostTypeId, 
                    model.LangId, 
                    selectedCategoryId: model.CategoryId
                );

            return await Task.FromResult(model);
        }

        /// <summary>
        /// Build <see cref="PostEditViewModel"/> when loading data.
        /// </summary>
        /// <param name="postId">PostId to load.</param>
        public async Task<PostEditViewModel> BuildEditViewModelAsync(int postId) {
            var post = await _postService.GetResultByIdAsync(id: postId);
            post.CheckReferenceIsNull();
            var model = post.Adapt<PostEditViewModel>();
            await setBaseDataAsync(model);

            return await Task.FromResult(model);
        }

        /// <summary>
        /// Build <see cref="PostEditViewModel"/> when post back to the Edit page.
        /// </summary>
        /// <param name="model"></param>
        public async Task<PostEditViewModel> BuildEditViewModelAsync(PostEditViewModel model) {
            await setBaseDataAsync(model);
            return await Task.FromResult(model);
        }
        
    }
}
