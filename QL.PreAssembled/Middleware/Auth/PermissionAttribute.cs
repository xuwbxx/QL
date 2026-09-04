namespace QL.PreAssembled.Middleware.Auth
{
    /// <summary>
    /// 接口权限标记特性，标记本接口需要哪个权限码
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class PermissionAttribute : Attribute
    {
        public string PermissionCode { get; }

        public PermissionAttribute(string permissionCode)
        {
            PermissionCode = permissionCode;
        }
    }
}
