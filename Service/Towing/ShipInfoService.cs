using Business.Towing;
using DataFactory.Factory;
using DataFactory.KingBase.Towing;
using Model.Towing;
using Tool;

namespace Service.Towing
{
    public class ShipInfoService
    {
        private readonly Towing_KingBase_UnitOfWorkFactory _towingKingBaseUowFactory;

        public ShipInfoService(Towing_KingBase_UnitOfWorkFactory towingKingBaseUowFactory)
        {
            _towingKingBaseUowFactory = towingKingBaseUowFactory;
        }

        public ResponseBase<PXYShipInfo> GetShipSurShips(string mmsi = "")
        {
            var res = new ResponseBase<PXYShipInfo>();
            try
            {
                if (string.IsNullOrEmpty(mmsi))
                {
                    using (var uow = _towingKingBaseUowFactory.Create())
                    {
                        var repo = uow.GetRepository<Manage_Ship>();
                        var ship = repo.Find(a => true).FirstOrDefault();
                        if (ship != null)
                        {
                            mmsi = ship.mmsi;
                        }
                    }
                }

                if (string.IsNullOrEmpty(mmsi))
                {
                    res.SetError($"没有有效mmsi:{mmsi}");
                }
                else
                {
                    res = PXYShipService.GetSurRoudingShips(mmsi);
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(ShipInfoService));
                res.SetError(ex);
            }
            return res;
        }
    }
}
