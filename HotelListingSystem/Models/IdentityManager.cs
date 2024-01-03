using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HotelListingSystem.Models
{
    public class IdentityManager
    {
        private ApplicationDbContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="IdentityManager"/> class.
        /// </summary>
        public IdentityManager()
        {
            _context = new ApplicationDbContext();
            UserManager = new UserManager<SystemUser>(new UserStore<SystemUser>(_context));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IdentityManager"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public IdentityManager(ApplicationDbContext context)
        {
            _context = context;
            UserManager = new UserManager<SystemUser>(new UserStore<SystemUser>(_context));
        }

        /// <summary>
        /// Gets or sets the user manager.
        /// </summary>
        /// <value>
        /// The user manager.
        /// </value>
        public UserManager<SystemUser> UserManager { get; set; }


        /// <summary>
        /// Roles the exists.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public bool RoleExists(string name)
        {
            var rm = new RoleManager<IdentityRole>(
                new RoleStore<IdentityRole>(_context));
            return rm.RoleExists(name);
        }

        public bool UserExists(string name)
        {
            var um = new UserManager<SystemUser>(new UserStore<SystemUser>(_context));

            return um.FindByName(name) != null;
        }

        /// <summary>
        /// Creates the role.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public bool CreateRole(string name)
        {
            try
            {
                var rm = new RoleManager<IdentityRole>(
                    new RoleStore<IdentityRole>(_context));
                var idResult = rm.Create(new IdentityRole(name));

                return idResult.Succeeded;
            }
            catch (Exception x)
            {

                throw x;
            }
        }

        /// <summary>
        /// Creates the user.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="password">The password.</param>
        /// <returns></returns>
        public bool CreateUser(SystemUser user, string password)
        {
            var idResult = UserManager.Create(user, password);

            return idResult.Succeeded;
        }

        /// <summary>
        /// Adds the user to role.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="roleName">Name of the role.</param>
        /// <returns></returns>
        public bool AddUserToRole(string userId, string roleName)
        {
            _ = (RoleExists(roleName) == true) ? true : CreateRole(roleName);
            var idResult = UserManager.AddToRole(userId, roleName);

            return idResult.Succeeded;
        }

        /// <summary>
        /// Clears the user roles.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        public void ClearUserRoles(string userId)
        {
            var user = UserManager.FindById(userId);
            var currentRoles = new List<IdentityUserRole>();
            currentRoles.AddRange(user.Roles);

            foreach (var role in currentRoles)
            {
                //um.RemoveFromRole( userId, role.Role.Name );
            }
        }


        public string CurrentUserId(string userName)
        {
            if (userName != null)
            {
                var aUser = UserManager.FindByName(userName);
                return aUser.Id;
            }

            return null;
        }
        public IEnumerable<SystemUser> GetUsersInRole(String role)
        {
            String roleId = _context.Roles.Where(x => x.Name == role).FirstOrDefault().Id;

            if (roleId == null)
                return Enumerable.Empty<SystemUser>();

            return UserManager.Users.Where(o => o.Roles.Any(s => s.RoleId == roleId)).ToList();
        }
    }
}
