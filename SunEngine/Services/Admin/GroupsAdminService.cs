using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using LinqToDB;
using LinqToDB.Data;
using LinqToDB.Identity;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Schema;
using SunEngine.Commons.DataBase;
using SunEngine.Commons.Models;
using SunEngine.Commons.Models.UserGroups;
using SunEngine.EntityServices;

namespace SunEngine.Services.Admin
{
    public class GroupsAdminService : DbService
    {
        const string UserGroupsFileName = "UserGroups.json";
        const string UserGroupSchemaFileName = "UserGroup.schema.json";


        readonly string UserGroupsConfigPath;
        readonly string UserGroupSchemaPath;


        public GroupsAdminService(
            DataBaseConnection db,
            IHostingEnvironment env) : base(db)
        {
            UserGroupsConfigPath = Path.Combine(env.ContentRootPath, UserGroupsFileName);
            UserGroupSchemaPath = Path.Combine(env.ContentRootPath, UserGroupSchemaFileName);
        }

        public string GetGroupsJson()
        {
            return File.ReadAllText(UserGroupsConfigPath);
        }

        public async Task LoadUserGroupsFromJsonAsync(string json)
        {
            IDictionary<string, Category> categories =
                await db.Categories.ToDictionaryAsync(x => x.Name);
            IDictionary<string, OperationKeyDB> operationKeys =
                await db.OperationKeys.ToDictionaryAsync(x => x.Name);

            var schema = GetJsonSchema();
            
            UserGroupsLoaderFromJson loader = new UserGroupsLoaderFromJson(categories, operationKeys, schema);

            
            loader.Seed(json);

            using (var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                List<UserToGroupTmp> userToGroups = await SaveUserToGroupsAsync();
                await ClearGroupsAsync();
                await CopyToDb(loader, userToGroups);
                SaveToFile(json);
                transaction.Complete();
            }
        }

        JSchema GetJsonSchema()
        {
            using (var jReader = new JsonTextReader(new StreamReader(File.OpenRead(UserGroupSchemaPath))))
            {
                return JSchema.Load(jReader);
            }
        }

        void SaveToFile(string json)
        {
            File.WriteAllText(UserGroupsConfigPath, json);
        }

        async Task ClearGroupsAsync()
        {
            await db.UserToGroups.DeleteAsync();
            await db.CategoryOperationAccess.DeleteAsync();
            await db.CategoryAccess.DeleteAsync();
            await db.UserGroups.DeleteAsync();
        }

        Task<List<UserToGroupTmp>> SaveUserToGroupsAsync()
        {
            return db.UserToGroups.Select(x => new UserToGroupTmp {userId = x.UserId, roleName = x.UserGroup.Name})
                .ToListAsync();
        }

        async Task CopyToDb(UserGroupsLoaderFromJson loader, List<UserToGroupTmp> userToGroups)
        {
            BulkCopyOptions options = new BulkCopyOptions
            {
                CheckConstraints = false,
                //BulkCopyType = BulkCopyType.Default,
                KeepIdentity = true
            };

            db.BulkCopy(options, loader.userGroups);
            db.BulkCopy(options, loader.categoryAccesses);
            db.BulkCopy(options, loader.categoryOperationAccesses);

            List<UserToGroup> userToGroupsNew = userToGroups.Select(x => new UserToGroup
            {
                UserId = x.userId,
                RoleId = loader.userGroups.FirstOrDefault(y => y.Name == x.roleName).Id
            }).ToList();

            db.BulkCopy(options, userToGroupsNew);
        }

        private class UserToGroupTmp
        {
            public int userId;
            public string roleName;
        }
    }
}