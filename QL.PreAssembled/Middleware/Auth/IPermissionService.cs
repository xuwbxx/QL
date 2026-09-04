namespace QL.PreAssembled.Middleware.Auth
{
    public interface IPermissionService
    {
        /// <summary>
        /// 接口过滤器使用：校验单个权限，每一次API请求执行
        /// 底层可以走 EXISTS 查询，不需要加载全部权限到内存
        /// </summary>
        Task<bool> CheckUserPermission(string permissionFlag, long userId);

        /// <summary>
        /// 仅登录后 /current‑user 调用一次，返回全部权限编码，供前端菜单渲染
        /// 业务上调用频次很低
        /// </summary>
        Task<HashSet<string>> GetUserPermissionCodes(long userId);
    }
}
