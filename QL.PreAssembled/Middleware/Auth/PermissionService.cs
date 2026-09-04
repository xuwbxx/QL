namespace QL.PreAssembled.Middleware.Auth
{
    public class PermissionService : IPermissionService
    {
        public async Task<bool> CheckUserPermission(string permissionFlag, long userId)
        {
            throw new NotImplementedException();
        }

        public async Task<HashSet<string>> GetUserPermissionCodes(long userId)
        {
            
            // 查询用户角色 → 查询角色对应的所有PermissionCode
            // 返回去重的权限编码集合
            HashSet<string> result = new HashSet<string>();
            // todo 你的数据库查询逻辑
            return result;
        }
    }
}
