using DataFactory.Factory;
using DataFactory.KingBase.Towing;
using Kdbndp;
using Model.Base;
using Model.Tech.IOT;
using Model.TechApi;
using System.Data;
using System.Data.Common;
using Tool;

namespace Service.Api
{
    public class SHXAService
    {
        private readonly Towing_KingBase_UnitOfWorkFactory _towingKingBaseUowFactory;

        public SHXAService(Towing_KingBase_UnitOfWorkFactory towingKingBaseUowFactory)
        {
            _towingKingBaseUowFactory = towingKingBaseUowFactory;
        }

        /// <summary>
        /// 三航翔安数据接收
        /// </summary>
        public string GetData_SHXA(BaseApiRequest<List<SHXARequest>> request)
        {
            if (string.IsNullOrEmpty(request.Requester) || !request.Requester.Equals("SHXA"))
            {
                return "error,bad requester";
            }
            if (request.Data == null || request.Data.Count == 0)
            {
                return "no data";
            }
            try
            {
                string FullSql = "";
                string TableName = DateTime.Now.Year.ToString();
                int month = DateTime.Now.Month;

                if (month == 1 || month == 2 || month == 3)
                    TableName += "01";
                else if (month == 4 || month == 5 || month == 6)
                    TableName += "02";
                else if (month == 7 || month == 8 || month == 9)
                    TableName += "03";
                else
                    TableName += "04";

                foreach (var item in request.Data)
                {
                    string Sql = string.Format(
                        @"INSERT INTO {79}(ship_data_id, ship_code, upload_status, dte, destination, ship_name, call_sign, max_draught, eta, ship_type_and_cargo_type, imo, true_heading, cog, ais_latitude, ais_longitude, sog, rot, navigate_status, latitude_offset, speed_kph, speed_knots, course_magnetic, course_true, magnetic_variation_direction, magnetic_variation, course, speed, gps_longitude, gps_latitude, longitude_direction, longitude_offset, latitude_direction, altitude_offset, mp1_dp_model, mp1_auto_rudder_model, mp2_dp_model, mp2_auto_rudder_model, mp3_dp_model, mp3_auto_rudder_model, bth1_dp_model, bth2_dp_model, heading_type, heading, trip_distance_ground, total_distance_ground, trip_distance_water, total_water_distance, stern_speed_ground, stern_speed_water_status, stern_speed_water, speed_ground_status, transverse_speed_ground, bow_stern_speed_ground, speed_water_status, transverse_speed_water, bow_stern_speed_water, run_status, vice_lift_height, vice_lift_actual_load, vice_lift_rated_load, main_lift2_height, main_lift2_actual_load, main_lift2_rated_load, main_lift1_height, main_lift1_actual_load, main_lift1_rated_load, posmru_d, posmru_f, posmru_s, yaw, pitch, roll, angrate_y, angrate_p, angrate_r, wind_speed_unit, wind_speed, reference, wind_angle, created_at, updated_at)
                        VALUES({0}, '{1}', {2}, {3}::numeric, '{4}'::varchar, '{5}'::varchar, '{6}'::varchar, {7}::numeric, '{8}'::varchar, '{9}'::varchar, '{10}'::varchar, {11}::numeric, {12}::numeric, {13}::numeric, {14}::numeric, {15}::numeric, {16}::numeric, '{17}'::varchar, {18}::numeric, {19}::numeric, {20}::numeric, {21}::numeric, {22}::numeric, '{23}'::varchar, {24}::numeric, {25}::numeric, {26}::numeric, {27}::numeric, {28}::numeric, '{29}'::varchar, {30}::numeric, '{31}'::varchar, {32}::numeric, '{33}'::varchar, '{34}'::varchar, '{35}'::varchar, '{36}'::varchar, '{37}'::varchar, '{38}'::varchar, '{39}'::varchar, '{40}'::varchar, '{41}'::varchar, {42}::numeric, {43}::numeric, {44}::numeric, {45}::numeric, {46}::numeric, {47}::numeric, '{48}'::varchar, {49}::numeric, '{50}'::varchar, {51}::numeric, {52}::numeric, '{53}'::varchar, {54}::numeric, {55}::numeric, '{56}'::varchar, {57}::numeric, {58}::numeric, {59}::numeric, {60}::numeric, {61}::numeric, {62}::numeric, {63}::numeric, {64}::numeric, {65}::numeric, {66}::numeric, {67}::numeric, {68}::numeric, {69}::numeric, {70}::numeric, {71}::numeric, {72}::numeric, {73}::numeric, {74}::numeric, '{75}'::varchar, {76}::numeric, '{77}'::varchar, {78}::numeric, '{80}', '{81}');",
                        item.id,
                        item.shipCode ?? "",
                        item.uploadStatus,
                        item.dte,
                        item.destination ?? "",
                        item.shipName ?? "",
                        item.callSign ?? "",
                        item.maxDraught,
                        item.eta ?? "",
                        item.shipTypeAndCargoType ?? "",
                        item.imo ?? "",
                        item.trueHeading,
                        item.cog,
                        item.aisLatitude,
                        item.aisLongitude,
                        item.sog,
                        item.rot,
                        item.navigateStatus ?? "",
                        "NULL",  //latitude_offset
                        "NULL",  //speed_kph
                        "NULL",  //speed_knots
                        "NULL",  //course_magnetic
                        "NULL",  //course_true
                        "NULL",  //magnetic_variation_direction
                        "NULL",  //magnetic_variation
                        "NULL",  //course
                        "NULL",  //speed
                        item.gpsLongitude,
                        item.gpsLatitude,
                        item.longitudeDirection ?? "",
                        "NULL",  //longitude_offset
                        item.latitudeDirection ?? "",
                        "NULL",  //altitude_offset
                        item.mp1DpModel ?? "",
                        item.mp1AutoRudderModel ?? "",
                        item.mp2DpModel ?? "",
                        item.mp2AutoRudderModel ?? "",
                        item.mp3DpModel ?? "",
                        item.mp3AutoRudderModel ?? "",
                        item.bth1DpModel ?? "",
                        item.bth2DpModel ?? "",
                        item.headingType ?? "",
                        item.heading,
                        item.tripDistanceGround,
                        item.totalDistanceGround,
                        item.tripDistanceWater,
                        item.totalWaterDistance,
                        item.sternSpeedGround,
                        item.sternSpeedWaterStatus,
                        item.sternSpeedWater,
                        item.speedGroundStatus ?? "",
                        item.transverseSpeedGround,
                        item.bowSternSpeedGround,
                        item.speedWaterStatus ?? "",
                        item.transverseSpeedWater,
                        item.bowSternSpeedWater,
                        item.runStatus ?? "",
                        item.viceLiftHeight,
                        item.viceLiftActualLoad,
                        item.viceLiftRatedLoad,
                        item.mainLift2Height,
                        item.mainLift2ActualLoad,
                        item.mainLift2RatedLoad,
                        item.mainLift1Height,
                        item.mainLift1ActualLoad,
                        item.mainLift1RatedLoad,
                        item.posmruD,
                        item.posmruF,
                        item.posmruS,
                        item.yaw,
                        item.pitch,
                        item.roll,
                        item.angrateY,
                        item.angrateP,
                        item.angrateR,
                        item.windSpeedUnit ?? "",
                        item.windSpeed,
                        item.reference ?? "",
                        item.windAngle,
                        "\"public\".\"Data_SHXA_" + TableName + "\"",
                        DateTimeOffset.FromUnixTimeMilliseconds(item.createdAt.Value).ToOffset(TimeSpan.FromHours(8)).DateTime,
                        DateTimeOffset.FromUnixTimeMilliseconds(item.updatedAt.Value).ToOffset(TimeSpan.FromHours(8)).DateTime
                    );

                    FullSql += Sql;
                }

                if (!string.IsNullOrEmpty(FullSql))
                {
                    using (var uow = _towingKingBaseUowFactory.Create())
                    {
                        var repo = uow.GetRepository<Data_Backup>();
                        repo.ExecuteSql(FullSql);
                    }
                }

                return "success";
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(SHXAService));
                return "error";
            }
        }

        /// <summary>
        /// Sql参数查询
        /// </summary>
        public Model.TechApi.BaseApiReturn<List<SHXATable>> QueryData_SHXA(SqlParamRequest request)
        {
            var ret = new Model.TechApi.BaseApiReturn<List<SHXATable>>();

            if (string.IsNullOrEmpty(request.Sql))
            {
                ret.Message = "sql语句是空的";
                return ret;
            }
            if (string.IsNullOrEmpty(request.Requester) || !request.Requester.Equals("SHXA"))
            {
                ret.Message = "商户号错误";
                return ret;
            }

            try
            {
                var parameters = BuildParameters(request.Params);

                using (var uow = _towingKingBaseUowFactory.Create())
                {
                    var repo = uow.GetRepository<Data_Backup>();
                    DataTable dt = repo.QueryDataTable(request.Sql, parameters);
                    var data = ConvertToSHXATableList(dt);
                    ret.Success = true;
                    ret.Data = data;
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(SHXAService));
                ret.Message = "error";
            }
            return ret;
        }

        /// <summary>
        /// 增删改
        /// </summary>
        public Model.TechApi.BaseApiReturn<int> OperateData_SHXA(SqlParamRequest request)
        {
            var ret = new Model.TechApi.BaseApiReturn<int>();

            if (string.IsNullOrEmpty(request.Sql))
            {
                ret.Message = "sql语句是空的";
                return ret;
            }
            if (string.IsNullOrEmpty(request.Requester) || !request.Requester.Equals("SHXA"))
            {
                ret.Message = "商户号错误";
                return ret;
            }

            try
            {
                var parameters = BuildParameters(request.Params);

                using (var uow = _towingKingBaseUowFactory.Create())
                {
                    var repo = uow.GetRepository<Data_Backup>();
                    int count = repo.ExecuteSql(request.Sql, parameters);
                    ret.Success = true;
                    ret.Data = count;
                }
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(SHXAService));
                ret.Message = "error";
            }
            return ret;
        }

        /// <summary>
        /// 接收图片
        /// </summary>
        public string GetPic_SHXA(BaseApiRequest<List<SHXAFileRequest>> request)
        {
            if (string.IsNullOrEmpty(request.Requester) || !request.Requester.Equals("SHXA"))
            {
                return "error,bad requester";
            }
            if (request.Data == null || request.Data.Count == 0)
            {
                return "no data";
            }

            try
            {
                string BaseDir = @"D:/Data/SHXA/";
                string FullSql = "";

                foreach (var item in request.Data)
                {
                    if (!string.IsNullOrEmpty(item.FilePath) && !string.IsNullOrEmpty(item.FileContent))
                    {
                        string FileName = item.FilePath.Substring(item.FilePath.LastIndexOf('/') + 1);
                        string FullPath = BaseDir + item.FilePath;
                        string DirPath = FullPath.Substring(0, FullPath.LastIndexOf('/') + 1);

                        FileUtils.CreateFolderFile(item.FileContent, DirPath, BaseDir + item.FilePath);

                        string Sql = string.Format(@"INSERT INTO {0}({4}, {5}, {6})
                            VALUES('{1}', '{2}', '{3}');",
                            "\"public\".\"Data_SHXA_File\"",
                            FileName,
                            item.FilePath,
                            DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                            "\"FileName\"",
                            "\"FilePath\"",
                            "\"CreateTime\""
                            );

                        FullSql += Sql;
                    }
                }

                if (!string.IsNullOrEmpty(FullSql))
                {
                    using (var uow = _towingKingBaseUowFactory.Create())
                    {
                        var repo = uow.GetRepository<Data_Backup>();
                        repo.ExecuteSql(FullSql);
                    }
                }

                return "success";
            }
            catch (Exception ex)
            {
                LoggerUtils.Error(ex.ToString(), typeof(SHXAService));
                return "error";
            }
        }

        #region 辅助方法

        private DbParameter[] BuildParameters(List<SqlParam> sqlParams)
        {
            var parameters = new List<DbParameter>();
            if (sqlParams != null && sqlParams.Count > 0)
            {
                foreach (var item in sqlParams)
                {
                    parameters.Add(new KdbndpParameter(item.Name, item.Value));
                }
            }
            return parameters.ToArray();
        }

        private List<SHXATable> ConvertToSHXATableList(DataTable dt)
        {
            var list = new List<SHXATable>();
            foreach (DataRow row in dt.Rows)
            {
                var item = new SHXATable();
                if (dt.Columns.Contains("ship_data_id") && row["ship_data_id"] != DBNull.Value) item.ship_data_id = Convert.ToInt64(row["ship_data_id"]);
                if (dt.Columns.Contains("ship_code") && row["ship_code"] != DBNull.Value) item.ship_code = row["ship_code"].ToString();
                if (dt.Columns.Contains("upload_status") && row["upload_status"] != DBNull.Value) item.upload_status = Convert.ToBoolean(row["upload_status"]);
                if (dt.Columns.Contains("dte") && row["dte"] != DBNull.Value) item.dte = Convert.ToDouble(row["dte"]);
                if (dt.Columns.Contains("destination") && row["destination"] != DBNull.Value) item.destination = row["destination"].ToString();
                if (dt.Columns.Contains("ship_name") && row["ship_name"] != DBNull.Value) item.ship_name = row["ship_name"].ToString();
                if (dt.Columns.Contains("call_sign") && row["call_sign"] != DBNull.Value) item.call_sign = row["call_sign"].ToString();
                if (dt.Columns.Contains("max_draught") && row["max_draught"] != DBNull.Value) item.max_draught = Convert.ToDouble(row["max_draught"]);
                if (dt.Columns.Contains("eta") && row["eta"] != DBNull.Value) item.eta = row["eta"].ToString();
                if (dt.Columns.Contains("ship_type_and_cargo_type") && row["ship_type_and_cargo_type"] != DBNull.Value) item.ship_type_and_cargo_type = row["ship_type_and_cargo_type"].ToString();
                if (dt.Columns.Contains("imo") && row["imo"] != DBNull.Value) item.imo = row["imo"].ToString();
                if (dt.Columns.Contains("true_heading") && row["true_heading"] != DBNull.Value) item.true_heading = Convert.ToDouble(row["true_heading"]);
                if (dt.Columns.Contains("cog") && row["cog"] != DBNull.Value) item.cog = Convert.ToDouble(row["cog"]);
                if (dt.Columns.Contains("ais_latitude") && row["ais_latitude"] != DBNull.Value) item.ais_latitude = Convert.ToDouble(row["ais_latitude"]);
                if (dt.Columns.Contains("ais_longitude") && row["ais_longitude"] != DBNull.Value) item.ais_longitude = Convert.ToDouble(row["ais_longitude"]);
                if (dt.Columns.Contains("sog") && row["sog"] != DBNull.Value) item.sog = Convert.ToDouble(row["sog"]);
                if (dt.Columns.Contains("rot") && row["rot"] != DBNull.Value) item.rot = Convert.ToDouble(row["rot"]);
                if (dt.Columns.Contains("navigate_status") && row["navigate_status"] != DBNull.Value) item.navigate_status = row["navigate_status"].ToString();
                if (dt.Columns.Contains("gps_longitude") && row["gps_longitude"] != DBNull.Value) item.gps_longitude = Convert.ToDouble(row["gps_longitude"]);
                if (dt.Columns.Contains("gps_latitude") && row["gps_latitude"] != DBNull.Value) item.gps_latitude = Convert.ToDouble(row["gps_latitude"]);
                if (dt.Columns.Contains("longitude_direction") && row["longitude_direction"] != DBNull.Value) item.longitude_direction = row["longitude_direction"].ToString();
                if (dt.Columns.Contains("latitude_direction") && row["latitude_direction"] != DBNull.Value) item.latitude_direction = row["latitude_direction"].ToString();
                if (dt.Columns.Contains("heading") && row["heading"] != DBNull.Value) item.heading = Convert.ToDouble(row["heading"]);
                if (dt.Columns.Contains("posmru_d") && row["posmru_d"] != DBNull.Value) item.posmru_d = Convert.ToDouble(row["posmru_d"]);
                if (dt.Columns.Contains("posmru_f") && row["posmru_f"] != DBNull.Value) item.posmru_f = Convert.ToDouble(row["posmru_f"]);
                if (dt.Columns.Contains("posmru_s") && row["posmru_s"] != DBNull.Value) item.posmru_s = Convert.ToDouble(row["posmru_s"]);
                if (dt.Columns.Contains("yaw") && row["yaw"] != DBNull.Value) item.yaw = Convert.ToDouble(row["yaw"]);
                if (dt.Columns.Contains("pitch") && row["pitch"] != DBNull.Value) item.pitch = Convert.ToDouble(row["pitch"]);
                if (dt.Columns.Contains("roll") && row["roll"] != DBNull.Value) item.roll = Convert.ToDouble(row["roll"]);
                if (dt.Columns.Contains("angrate_y") && row["angrate_y"] != DBNull.Value) item.angrate_y = Convert.ToDouble(row["angrate_y"]);
                if (dt.Columns.Contains("angrate_p") && row["angrate_p"] != DBNull.Value) item.angrate_p = Convert.ToDouble(row["angrate_p"]);
                if (dt.Columns.Contains("angrate_r") && row["angrate_r"] != DBNull.Value) item.angrate_r = Convert.ToDouble(row["angrate_r"]);
                if (dt.Columns.Contains("wind_speed") && row["wind_speed"] != DBNull.Value) item.wind_speed = Convert.ToDouble(row["wind_speed"]);
                if (dt.Columns.Contains("wind_angle") && row["wind_angle"] != DBNull.Value) item.wind_angle = Convert.ToDouble(row["wind_angle"]);
                if (dt.Columns.Contains("created_at") && row["created_at"] != DBNull.Value) item.created_at = Convert.ToInt64(row["created_at"]);
                if (dt.Columns.Contains("updated_at") && row["updated_at"] != DBNull.Value) item.updated_at = Convert.ToInt64(row["updated_at"]);
                list.Add(item);
            }
            return list;
        }

        #endregion
    }
}
