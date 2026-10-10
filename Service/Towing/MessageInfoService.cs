using Business.Towing;
using DataFactory.Factory;
using Microsoft.EntityFrameworkCore;
using Tool;

namespace Service.Towing
{
    public class MessageInfo
    {
        public int ID { set; get; }
        /// <summary>
        /// text：文字，image：图片
        /// </summary>
        public string MsgType { set; get; }
        /// <summary>
        /// 0：普通，1：紧急
        /// </summary>
        public int MsgLevel { set; get; }
        /// <summary>
        /// 消息内容
        /// </summary>
        public string MsgContent { set; get; }
        /// <summary>
        /// 发送人
        /// </summary>
        public string Sender { set; get; }
        public DateTime CreateTime { set; get; }
        public bool IsDeleted { set; get; }
        public DateTime LastTime { set; get; }
    }

    public class MessageInfoService
    {
        private readonly Towing_KingBase_UnitOfWorkFactory _towingKingBaseUowFactory;

        public MessageInfoService(Towing_KingBase_UnitOfWorkFactory towingKingBaseUowFactory)
        {
            _towingKingBaseUowFactory = towingKingBaseUowFactory;
        }

        public ResponseBase<MessageInfo> Send(MessageInfo msg)
        {
            var res = new ResponseBase<MessageInfo>();
            try
            {
                if (string.IsNullOrEmpty(msg.Sender))
                {
                    msg.Sender = "未知发送人";
                }

                using (var uow = _towingKingBaseUowFactory.Create())
                {
                    var repo = uow.GetRepository<DataFactory.KingBase.Towing.Message_Body>();
                    var data = new DataFactory.KingBase.Towing.Message_Body();
                    data.MsgType = msg.MsgType;
                    data.MsgLevel = msg.MsgLevel;
                    data.MsgContent = msg.MsgContent;
                    data.Sender = msg.Sender;
                    data.CreateTime = DateTime.Now;
                    data.IsDelete = false;

                    repo.Add(data);
                    uow.Save();

                    var msgList = repo.Find(a => !a.IsDelete && a.CreateTime > msg.LastTime)
                        .OrderBy(a => a.CreateTime)
                        .ToList();

                    foreach (var d in msgList)
                    {
                        var item = new MessageInfo();
                        item.ID = d.id;
                        item.MsgType = d.MsgType;
                        item.MsgLevel = d.MsgLevel ?? 0;
                        item.MsgContent = d.MsgContent;
                        item.Sender = d.Sender;
                        item.CreateTime = d.CreateTime ?? DateTime.MinValue;
                        item.IsDeleted = d.IsDelete;
                        res.List.Add(item);
                    }

                    if (res.List.Count > 0)
                    {
                        res.Data = new MessageInfo();
                        res.Data.LastTime = res.List.Max(a => a.CreateTime);
                    }

                    res.SetOK();
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(MessageInfoService));
                res.SetError(ex);
            }
            return res;
        }

        public ResponseBase<MessageInfo> MessageListHistory(DateTime? dateTime = null,
            string type = "", int lvl = -1,
            string query = "", int idx = 1, int size = 20)
        {
            var res = new ResponseBase<MessageInfo>();
            try
            {
                using (var uow = _towingKingBaseUowFactory.Create())
                {
                    var repo = uow.GetRepository<DataFactory.KingBase.Towing.Message_Body>();
                    if (size == 0) size = LimitValues.MsgSize;

                    var q = repo.Find(a => !a.IsDelete).AsEnumerable();
                    if (!string.IsNullOrEmpty(query))
                    {
                        foreach (var str in query.Split(' '))
                        {
                            if (!string.IsNullOrEmpty(str))
                            {
                                q = q.Where(a => EF.Functions.Like(a.MsgContent, $"%{str}%"));
                            }
                        }
                    }
                    if (dateTime != null)
                    {
                        q = q.Where(a => a.CreateTime != null && a.CreateTime.Value.Date == dateTime.Value.Date);
                    }
                    if (!string.IsNullOrEmpty(type))
                    {
                        q = q.Where(a => a.MsgType == type);
                    }
                    if (lvl > -1)
                    {
                        q = q.Where(a => a.MsgLevel == lvl);
                    }

                    if (idx < 1) idx = 1;
                    if (size < 1) size = 20;
                    res.TotalRowNum = q.Count();
                    res.PageCount = (int)(res.TotalRowNum / size);
                    res.PageIndex = idx;
                    res.PageSize = size;

                    var list = q.OrderByDescending(a => a.CreateTime)
                        .Skip((idx - 1) * size)
                        .Take(size).ToList();

                    foreach (var data in list.OrderBy(a => a.CreateTime))
                    {
                        var itm = new MessageInfo();
                        itm.ID = data.id;
                        itm.MsgType = data.MsgType;
                        itm.MsgLevel = data.MsgLevel ?? 0;
                        itm.MsgContent = data.MsgContent;
                        itm.Sender = data.Sender;
                        itm.CreateTime = data.CreateTime ?? DateTime.MinValue;
                        itm.IsDeleted = data.IsDelete;
                        res.List.Add(itm);
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(MessageInfoService));
                res.SetError(ex);
            }
            return res;
        }

        public ResponseBase<MessageInfo> ListMsg(DateTime? lastTime = null, string query = "", int size = 0)
        {
            var res = new ResponseBase<MessageInfo>();
            try
            {
                using (var uow = _towingKingBaseUowFactory.Create())
                {
                    var repo = uow.GetRepository<DataFactory.KingBase.Towing.Message_Body>();
                    if (size == 0) size = LimitValues.MsgSize;

                    lastTime = lastTime ?? DateTime.Now.AddDays(-1);
                    var q = repo.Find(a => !a.IsDelete).AsEnumerable();
                    if (!string.IsNullOrEmpty(query))
                    {
                        foreach (var str in query.Split(' '))
                        {
                            if (!string.IsNullOrEmpty(str))
                            {
                                q = q.Where(a => EF.Functions.Like(a.MsgContent, $"%{str}%"));
                            }
                        }
                    }
                    else
                    {
                        q = q.Where(a => a.CreateTime > lastTime);
                    }

                    var list = q.OrderByDescending(a => a.CreateTime)
                        .Take(size).ToList();

                    foreach (var data in list.OrderBy(a => a.CreateTime))
                    {
                        var itm = new MessageInfo();
                        itm.ID = data.id;
                        itm.MsgType = data.MsgType;
                        itm.MsgLevel = data.MsgLevel ?? 0;
                        itm.MsgContent = data.MsgContent;
                        itm.Sender = data.Sender;
                        itm.CreateTime = data.CreateTime ?? DateTime.MinValue;
                        itm.IsDeleted = data.IsDelete;
                        res.List.Add(itm);
                    }

                    res.Data = new MessageInfo();
                    if (list.Count == 0)
                    {
                        res.Data.LastTime = lastTime ?? DateTime.MinValue;
                    }
                    else
                    {
                        res.Data.LastTime = res.List.Max(a => a.CreateTime);
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(MessageInfoService));
                res.SetError(ex);
            }
            return res;
        }
    }
}
