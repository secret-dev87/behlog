﻿using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Behlog.Core.Extensions;
using Behlog.Core.Contracts;
using Behlog.Core.Models.Enum;
using Behlog.Core.Models.Security;
using Behlog.Services.Contracts.Security;
using Behlog.Storage.Core;
using Behlog.Web.ViewModels.Security;

namespace Behlog.Services.Security {


    /// <summary>
    /// More info: http://www.dotnettips.info/post/2578
    /// </summary>
    public class AppRoleManager: RoleManager<Role>, IAppRoleManager {

        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IBehlogContext _db;
        private readonly IdentityErrorDescriber _errors;
        private readonly ILookupNormalizer _keyNormalizer;
        private readonly ILogger<AppRoleManager> _logger;
        private readonly IEnumerable<IRoleValidator<Role>> _roleValidators;
        private readonly IAppRoleStore _store;
        private readonly DbSet<User> _users;

        public AppRoleManager(
            IAppRoleStore store,
            IEnumerable<IRoleValidator<Role>> roleValidators,
            ILookupNormalizer keyNormalizer,
            IdentityErrorDescriber errors,
            ILogger<AppRoleManager> logger,
            IHttpContextAccessor contextAccessor,
            IBehlogContext uow) 
            : base((RoleStore<Role, BehlogContext, Guid, UserRole, RoleClaim>)store, roleValidators, keyNormalizer, errors, logger) 
        {
            _store = store ?? throw new ArgumentNullException(nameof(_store));
            _roleValidators = roleValidators ?? throw new ArgumentNullException(nameof(_roleValidators));
            _keyNormalizer = keyNormalizer ?? throw new ArgumentNullException(nameof(_keyNormalizer));
            _errors = errors ?? throw new ArgumentNullException(nameof(_errors));
            _logger = logger ?? throw new ArgumentNullException(nameof(_logger));
            _contextAccessor = contextAccessor ?? throw new ArgumentNullException(nameof(_contextAccessor));
            _db = uow ?? throw new ArgumentNullException(nameof(_db));
            _users = _db.Set<User>();
        }

        public IList<Role> FindCurrentUserRoles() {
            var userId = getCurrentUserId();
            return FindUserRoles(userId);
        }

        public IList<Role> FindUserRoles(Guid userId) {
            var userRolesQuery = from role in Roles
                                 from user in role.Users
                                 where user.UserId == userId
                                 select role;

            return userRolesQuery.OrderBy(x => x.Name).ToList();
        }

        public Task<List<Role>> GetAllCustomRolesAsync() {
            return Roles.ToListAsync();
        }

        public IList<RoleUsersCountViewModel> GetAllCustomRolesAndUsersCountList() {
            return Roles.Select(role =>
                                    new RoleUsersCountViewModel {
                                        Role = role,
                                        UsersCount = role.Users.Count()
                                    }).ToList();
        }

        public async Task<UserListViewModel> GetUsersInRoleListAsync(
                Guid roleId,
                int pageNumber, int recordsPerPage,
                string sortByField, SortOrder sortOrder,
                bool showAllUsers) {
            var skipRecords = pageNumber * recordsPerPage;

            var roleUserIdsQuery = from role in Roles
                                   where role.Id == roleId
                                   from user in role.Users
                                   select user.UserId;
            var query = _users.Include(user => user.Roles)
                              .Where(user => roleUserIdsQuery.Contains(user.Id))
                         .AsNoTracking();

            if (!showAllUsers) {
                query = query.Where(x => x.Status == UserStatus.Enabled);
            }

            switch (sortByField) {
                case nameof(User.Id):
                    query = sortOrder == SortOrder.Descending ? query.OrderByDescending(x => x.Id) : query.OrderBy(x => x.Id);
                    break;
                default:
                    query = sortOrder == SortOrder.Descending ? query.OrderByDescending(x => x.Id) : query.OrderBy(x => x.Id);
                    break;
            }

            return new UserListViewModel {
                TotalCount = await query.CountAsync(),
                Items = await query.Skip(skipRecords)
                    .Take(recordsPerPage)
                    .ToListAsync(),
                Roles = await Roles.ToListAsync()
            };
        }

        public IList<User> GetUsersInRole(string roleName) {
            var roleUserIdsQuery = from role in Roles
                                   where role.Name == roleName
                                   from user in role.Users
                                   select user.UserId;
            return _users.Where(_ => roleUserIdsQuery.Contains(_.Id))
                         .ToList();
        }

        public IList<Role> GetRolesForCurrentUser() {
            var userId = getCurrentUserId();
            return GetRolesForUser(userId);
        }

        public IList<Role> GetRolesForUser(Guid userId) {
            var roles = FindUserRoles(userId);
            if (roles == null || !roles.Any()) {
                return new List<Role>();
            }

            return roles.ToList();
        }

        public IList<UserRole> GetUserRolesInRole(string roleName) {
            return Roles.Where(role => role.Name == roleName)
                             .SelectMany(role => role.Users)
                             .ToList();
        }

        public bool IsCurrentUserInRole(string roleName) {
            var userId = getCurrentUserId();
            return IsUserInRole(userId, roleName);
        }

        public bool IsUserInRole(Guid userId, string roleName) {
            var userRolesQuery = from role in Roles
                                 where role.Name == roleName
                                 from user in role.Users
                                 where user.UserId == userId
                                 select role;
            var userRole = userRolesQuery.FirstOrDefault();
            return userRole != null;
        }

        public Task<Role> FindRoleIncludeRoleClaimsAsync(Guid roleId) {
            return Roles.Include(x => x.Claims).FirstOrDefaultAsync(x => x.Id == roleId);
        }

        public async Task<IdentityResult> AddOrUpdateRoleClaimsAsync(
            Guid roleId,
            string roleClaimType,
            IList<string> selectedRoleClaimValues) {
            var role = await FindRoleIncludeRoleClaimsAsync(roleId);
            if (role == null) {
                return IdentityResult.Failed(new IdentityError {
                    Code = "RoleNotFound",
                    Description = "نقش مورد نظر یافت نشد."
                });
            }

            var currentRoleClaimValues = role.Claims.Where(roleClaim => roleClaim.ClaimType == roleClaimType)
                                                    .Select(roleClaim => roleClaim.ClaimValue)
                                                    .ToList();

            selectedRoleClaimValues ??= new List<string>();
            var newClaimValuesToAdd = selectedRoleClaimValues.Except(currentRoleClaimValues).ToList();
            foreach (var claimValue in newClaimValuesToAdd) {
                role.Claims.Add(new RoleClaim {
                    RoleId = role.Id,
                    ClaimType = roleClaimType,
                    ClaimValue = claimValue
                });
            }

            var removedClaimValues = currentRoleClaimValues.Except(selectedRoleClaimValues).ToList();
            foreach (var claimValue in removedClaimValues) {
                var roleClaim = role.Claims.SingleOrDefault(rc => rc.ClaimValue == claimValue &&
                                                                  rc.ClaimType == roleClaimType);
                if (roleClaim != null) {
                    role.Claims.Remove(roleClaim);
                }
            }

            return await UpdateAsync(role);
        }

        private Guid getCurrentUserId() => _contextAccessor.HttpContext.User.Identity.GetUserId();

    }
}
