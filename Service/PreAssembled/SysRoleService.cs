using DataFactory.Factory;
using Model.tech.QL.DTO.SysRole;

namespace Service.PreAssembled
{
    public class SysRoleService(QlPreAssembled_KingBase_UnitOfWorkFactory qlUowFactory) : ServiceBase(qlUowFactory)
    {
        public async Task<SysRoleItemDTO> List(SysRoleQueryDTO req)
        {
            throw new NotImplementedException();
        }
    }
}
