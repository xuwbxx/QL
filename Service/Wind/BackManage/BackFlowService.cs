using DataFactory.Factory;
using DataFactory.KingBase.CloudWind;
using Model.Tech.Cloud;
using Model.Tech.Cloud.BackManage;
using Tool;

namespace Service.Wind.BackManage
{
    public class BackFlowService
    {
        private readonly CloudWind_KingBase_UnitOfWorkFactory _cloudWindUowFactory;

        public BackFlowService(CloudWind_KingBase_UnitOfWorkFactory cloudWindUowFactory)
        {
            _cloudWindUowFactory = cloudWindUowFactory;
        }

        public List<Manage_Software> GetSoftwareList()
        {
            using (var uow = _cloudWindUowFactory.Create())
            {
                var repo = uow.GetRepository<Manage_Software>();
                return repo.Find(a => !a.IsDelete).ToList();
            }
        }

        public (List<CloudWindManageNode> list, int totalCount, int pageIndex, string msg) NodeListQuery(CloudWindBackManageRequest request)
        {
            List<CloudWindManageNode> list = new List<CloudWindManageNode>();
            string msg = "";
            int totalCount = 0;
            int pageIndex = request.PageIndex;

            try
            {
                using (var uow = _cloudWindUowFactory.Create())
                {
                    var nodeRepo = uow.GetRepository<Flow_Node>();
                    var managerRepo = uow.GetRepository<Flow_NodeManageUser>();

                    var predicate = PredicateBuilder.True<Flow_Node>();
                    predicate = PredicateBuilder.And(predicate, a => !a.IsDelete);

                    if (!string.IsNullOrEmpty(request.NodeName))
                    {
                        var nodeName = request.NodeName;
                        predicate = PredicateBuilder.And(predicate, a => a.NodeName != null && a.NodeName.Contains(nodeName));
                    }

                    if (request.SoftwareID != 0)
                    {
                        var softwareID = request.SoftwareID;
                        predicate = PredicateBuilder.And(predicate, a => a.SoftwareID == softwareID);
                    }

                    var (pageList, count) = nodeRepo.FindPage(predicate, a => a.ID, request.PageIndex, request.PageSize);

                    totalCount = count;

                    if (request.PageIndex != 1 && pageList.Count() == 0)
                    {
                        pageIndex = 1;
                        (pageList, totalCount) = nodeRepo.FindPage(predicate, a => a.ID, 1, request.PageSize);
                    }

                    var nodeManagers = managerRepo.Find(a => !a.IsDelete).ToList();

                    foreach (var a in pageList)
                    {
                        list.Add(new CloudWindManageNode()
                        {
                            ID = a.ID,
                            NodeName = a.NodeName ?? "",
                            SoftwareID = a.SoftwareID,
                            DoEdit = a.DoEdit ?? false,
                            NodeApprovalType = a.NodeApprovalType ?? false,
                            SoftwareName = Enum.GetName(typeof(CloudWindSoftware), a.SoftwareID) ?? "",
                            ManagerIsSetting = nodeManagers.Exists(b => b.NodeID == a.ID)
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(BackFlowService));
                msg = "发生错误";
            }

            return (list, totalCount, pageIndex, msg);
        }

        public (CloudWindManageNode? node, string msg) NodeDataQuery(CloudWindBackManageRequest request)
        {
            CloudWindManageNode? node = null;
            string msg = "";

            try
            {
                using (var uow = _cloudWindUowFactory.Create())
                {
                    var nodeRepo = uow.GetRepository<Flow_Node>();
                    var viewRepo = uow.GetRepository<View_NodeManageUser>();

                    var thisNode = nodeRepo.FindFirst(a => !a.IsDelete && a.ID == request.ID);
                    if (thisNode == null)
                    {
                        msg = "节点数据错误";
                        return (node, msg);
                    }

                    node = new CloudWindManageNode()
                    {
                        ID = thisNode.ID,
                        NodeName = thisNode.NodeName ?? "",
                        DoEdit = thisNode.DoEdit ?? false,
                        NodeApprovalType = thisNode.NodeApprovalType ?? false
                    };

                    var managers = viewRepo.Find(a => a.NodeID == request.ID).OrderBy(a => a.ID).ToList();
                    foreach (var a in managers)
                    {
                        node.NodeManagers.Add(new CloudWindManageNodeManager()
                        {
                            ID = a.ID,
                            ManageName = a.ManageName ?? "",
                            ManagePhone = a.ManagePhone ?? "",
                            ManageDepart = a.ManageDepart ?? "",
                            ManageUserCode = a.ManageUserCode ?? "",
                            ManageJobName = a.ManageJobName ?? ""
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(BackFlowService));
                msg = "发生错误";
            }

            return (node, msg);
        }

        public string DataSave(CloudWindBackManageRequest request)
        {
            string msg = "";

            try
            {
                using (var uow = _cloudWindUowFactory.Create())
                {
                    var nodeRepo = uow.GetRepository<Flow_Node>();
                    var managerRepo = uow.GetRepository<Flow_NodeManageUser>();

                    var node = nodeRepo.FindFirst(a => !a.IsDelete && a.ID == request.NodeID);
                    if (node == null)
                    {
                        return "节点数据错误";
                    }

                    node.DoEdit = request.NodeDoEdit;
                    node.NodeApprovalType = request.NodeApprovalType;

                    // 先删除旧的管理人员
                    var oldManagers = managerRepo.Find(a => !a.IsDelete && a.NodeID == node.ID).ToList();
                    foreach (var a in oldManagers)
                    {
                        a.IsDelete = true;
                    }

                    // 添加新的管理人员
                    foreach (var a in request.NodeManagers)
                    {
                        if (!string.IsNullOrEmpty(a.UserCode) && !string.IsNullOrEmpty(a.UserName))
                        {
                            var manager = new Flow_NodeManageUser()
                            {
                                NodeID = node.ID,
                                ManageName = a.UserName,
                                ManageUserCode = a.UserCode,
                                ManageDepart = a.UserDepartName,
                                ManagePhone = a.UserPhone,
                                ManageJobName = a.UserJobName,
                                CreateTime = DateTime.UtcNow,
                                IsDelete = false
                            };
                            managerRepo.Add(manager);
                        }
                    }

                    uow.Save();
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(BackFlowService));
                msg = "发生错误，请联系管理员";
            }

            return msg;
        }
    }
}
