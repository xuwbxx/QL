namespace Model.Tech.IOT
{

    public class SHXARequest
    {

        public SHXARequest() { }

        public double? aisLatitude { set; get; }

        public double? aisLongitude { set; get; }

        public double? angrateP { set; get; }

        public double? angrateR { set; get; }

        public double? angrateY { set; get; }

        public double? bowSternSpeedGround { set; get; }

        public double? bowSternSpeedWater { set; get; }


        public string bth1DpModel { set; get; }
        public string bth2DpModel { set; get; }


        public string callSign { set; get; }

        public double? cog { set; get; }

        public long? createdAt { set; get; }

        public string destination { set; get; }

        public double? dte { set; get; }

        public string eta { set; get; }

        public double? gpsLatitude { set; get; }

        public double? gpsLongitude { set; get; }

        public double? heading { set; get; }

        public string headingType { set; get; }

        public int? id { set; get; }

        public string imo { set; get; }

        public string latitudeDirection { set; get; }

        public string longitudeDirection { set; get; }

        public double? mainLift1ActualLoad { set; get; }
        public double? mainLift1Height { set; get; }
        public double? mainLift1RatedLoad { set; get; }
        public double? mainLift2ActualLoad { set; get; }
        public double? mainLift2Height { set; get; }
        public double? mainLift2RatedLoad { set; get; }
        public double? maxDraught { set; get; }

        public string mp1AutoRudderModel { set; get; }

        public string mp1DpModel { set; get; }

        public string mp2AutoRudderModel { set; get; }

        public string mp2DpModel { set; get; }

        public string mp3AutoRudderModel { set; get; }

        public string mp3DpModel { set; get; }

        public string navigateStatus { set; get; }

        public double? pitch { set; get; }

        public double? posmruD { set; get; }

        public double? posmruF { set; get; }

        public double? posmruS { set; get; }

        public string reference { set; get; }

        public double? roll { set; get; }

        public double? rot { set; get; }

        public string runStatus { set; get; }

        public string shipCode { set; get; }

        public string shipName { set; get; }

        public string shipTypeAndCargoType { set; get; }

        public double? sog { set; get; }

        public string speedGroundStatus { set; get; }

        public string speedWaterStatus { set; get; }

        public double? sternSpeedGround { set; get; }

        public double? sternSpeedWater { set; get; }

        public string sternSpeedWaterStatus { set; get; }

        public double? totalDistanceGround { set; get; }

        public double? totalWaterDistance { set; get; }

        public double? transverseSpeedGround { set; get; }

        public double? transverseSpeedWater { set; get; }

        public double? tripDistanceGround { set; get; }

        public double? tripDistanceWater { set; get; }

        public double? trueHeading { set; get; }

        public long? updatedAt { set; get; }

        public int? uploadStatus { set; get; }

        public double? viceLiftActualLoad { set; get; }
        public double? viceLiftHeight { set; get; }
        public double? viceLiftRatedLoad { set; get; }
        public double? windAngle { set; get; }
        public double? windSpeed { set; get; }

        public string windSpeedUnit { set; get; }

        public double? yaw { set; get; }
    }

    public class SHXATable
    {
        public SHXATable() { }

        /// <summary>
        /// 主键ID
        /// </summary>
        public long? id { set; get; }

        /// <summary>
        /// 船端id
        /// </summary>
        public long? ship_data_id { set; get; }

        /// <summary>
        /// 船舶Code，用于岸端区分船舶
        /// </summary>
        public string ship_code { set; get; }

        /// <summary>
        /// 上传成功标识：0-未上传，1-上传成功
        /// </summary>
        public bool upload_status { set; get; }

        /// <summary>
        /// 剩余里程
        /// </summary>
        public double? dte { set; get; }

        /// <summary>
        /// 目的港
        /// </summary>
        public string destination { set; get; }

        /// <summary>
        /// 船名
        /// </summary>
        public string ship_name { set; get; }

        /// <summary>
        /// 呼号
        /// </summary>
        public string call_sign { set; get; }

        /// <summary>
        /// 最大吃水
        /// </summary>
        public double? max_draught { set; get; }

        /// <summary>
        /// 预计到达时间
        /// </summary>
        public string eta { set; get; }

        /// <summary>
        /// 船舶和货物种类
        /// </summary>
        public string ship_type_and_cargo_type { set; get; }

        /// <summary>
        /// IMO
        /// </summary>
        public string imo { set; get; }

        /// <summary>
        /// 船首真航向
        /// </summary>
        public double? true_heading { set; get; }

        /// <summary>
        /// 对地航向
        /// </summary>
        public double? cog { set; get; }

        /// <summary>
        /// 纬度
        /// </summary>
        public double? ais_latitude { set; get; }

        /// <summary>
        /// 经度
        /// </summary>
        public double? ais_longitude { set; get; }

        /// <summary>
        /// 实际航速(对地)
        /// </summary>
        public double? sog { set; get; }

        /// <summary>
        /// 转向速率
        /// </summary>
        public double? rot { set; get; }

        /// <summary>
        /// 航行状态
        /// </summary>
        public string navigate_status { set; get; }

        /// <summary>
        /// 纬度偏移
        /// </summary>
        public double? latitude_offset { set; get; }

        /// <summary>
        /// 速度（kph）
        /// </summary>
        public double? speed_kph { set; get; }

        /// <summary>
        /// 速度（节）
        /// </summary>
        public double? speed_knots { set; get; }

        /// <summary>
        /// 磁北航向
        /// </summary>
        public double? course_magnetic { set; get; }

        /// <summary>
        /// 真北航向
        /// </summary>
        public double? course_true { set; get; }

        /// <summary>
        /// 磁偏角方向(E/W)
        /// </summary>
        public string magnetic_variation_direction { set; get; }

        /// <summary>
        /// 磁偏角
        /// </summary>
        public double? magnetic_variation { set; get; }

        /// <summary>
        /// 航向角
        /// </summary>
        public double? course { set; get; }

        /// <summary>
        /// 速度
        /// </summary>
        public double? speed { set; get; }

        /// <summary>
        /// 经度
        /// </summary>
        public double? gps_longitude { set; get; }

        /// <summary>
        /// 纬度
        /// </summary>
        public double? gps_latitude { set; get; }

        /// <summary>
        /// 经度方向(E/W)
        /// </summary>
        public string longitude_direction { set; get; }

        /// <summary>
        /// 经度偏移
        /// </summary>
        public double? longitude_offset { set; get; }

        /// <summary>
        /// 纬度方向(N/S)
        /// </summary>
        public string latitude_direction { set; get; }

        /// <summary>
        /// 高度偏移
        /// </summary>
        public double? altitude_offset { set; get; }

        /// <summary>
        /// DP模式
        /// </summary>
        public string mp1_dp_model { set; get; }

        /// <summary>
        /// 自动舵模式
        /// </summary>
        public string mp1_auto_rudder_model { set; get; }

        /// <summary>
        /// DP模式
        /// </summary>
        public string mp2_dp_model { set; get; }

        /// <summary>
        /// 自动舵模式
        /// </summary>
        public string mp2_auto_rudder_model { set; get; }

        /// <summary>
        /// DP模式
        /// </summary>
        public string mp3_dp_model { set; get; }

        /// <summary>
        /// 自动舵模式
        /// </summary>
        public string mp3_auto_rudder_model { set; get; }

        /// <summary>
        /// DP模式
        /// </summary>
        public string bth1_dp_model { set; get; }

        /// <summary>
        /// DP模式
        /// </summary>
        public string bth2_dp_model { set; get; }

        /// <summary>
        /// 航向类型 (T/M)
        /// </summary>
        public string heading_type { set; get; }

        /// <summary>
        /// 航向
        /// </summary>
        public double? heading { set; get; }

        /// <summary>
        /// 地面旅行距离
        /// </summary>
        public double? trip_distance_ground { set; get; }

        /// <summary>
        /// 总地面距离
        /// </summary>
        public double? total_distance_ground { set; get; }

        /// <summary>
        /// 水上旅行距离
        /// </summary>
        public double? trip_distance_water { set; get; }

        /// <summary>
        /// 总水距离
        /// </summary>
        public double? total_water_distance { set; get; }

        /// <summary>
        /// 船尾在地面上的速度
        /// </summary>
        public double? stern_speed_ground { set; get; }

        /// <summary>
        /// 船尾速度在水中的状态
        /// </summary>
        public string stern_speed_water_status { set; get; }

        /// <summary>
        /// 船尾在水中的速度
        /// </summary>
        public double? stern_speed_water { set; get; }

        /// <summary>
        /// 速度在地面上的状态(A/V)
        /// </summary>
        public string speed_ground_status { set; get; }

        /// <summary>
        /// 横向在地面上的速度
        /// </summary>
        public double? transverse_speed_ground { set; get; }

        /// <summary>
        /// 船首/船尾在地面上的速
        /// </summary>
        public double? bow_stern_speed_ground { set; get; }

        /// <summary>
        /// 速度在水中的状态(A/V)
        /// </summary>
        public string speed_water_status { set; get; }

        /// <summary>
        /// 横向在水中的速度
        /// </summary>
        public double? transverse_speed_water { set; get; }

        /// <summary>
        /// 船首/船尾在水中的速度
        /// </summary>
        public double? bow_stern_speed_water { set; get; }

        /// <summary>
        /// 运行状态
        /// </summary>
        public string run_status { set; get; }

        /// <summary>
        /// 副起升高度
        /// </summary>
        public double? vice_lift_height { set; get; }

        /// <summary>
        /// 副起升实际载荷
        /// </summary>
        public double? vice_lift_actual_load { set; get; }

        /// <summary>
        /// 副起升额定载荷
        /// </summary>
        public double? vice_lift_rated_load { set; get; }

        /// <summary>
        /// 主起升2#高度
        /// </summary>
        public double? main_lift2_height { set; get; }

        /// <summary>
        /// 主起升2#实际载荷
        /// </summary>
        public double? main_lift2_actual_load { set; get; }

        /// <summary>
        /// 主起升2#额定载荷
        /// </summary>
        public double? main_lift2_rated_load { set; get; }

        /// <summary>
        /// 主起升1#高度
        /// </summary>
        public double? main_lift1_height { set; get; }

        /// <summary>
        /// 主起升1#实际载荷
        /// </summary>
        public double? main_lift1_actual_load { set; get; }

        /// <summary>
        /// 主起升1#额定载荷
        /// </summary>
        public double? main_lift1_rated_load { set; get; }

        /// <summary>
        /// Linear position at MRU point in down direction (h-frame)
        /// </summary>
        public double? posmru_d { set; get; }

        /// <summary>
        /// Linear position at MRU point in forward direction (h-frame)
        /// </summary>
        public double? posmru_f { set; get; }

        /// <summary>
        /// Linear position at MRU point in starboard direction (h-frame)
        /// </summary>
        public double? posmru_s { set; get; }

        /// <summary>
        /// Yaw angle (euler angle)
        /// </summary>
        public double? yaw { set; get; }

        /// <summary>
        /// Pitch angle (euler angle)
        /// </summary>
        public double? pitch { set; get; }

        /// <summary>
        /// Roll angle (euler angle)
        /// </summary>
        public double? roll { set; get; }

        /// <summary>
        /// Angular rate in yaw axis (b-frame)
        /// </summary>
        public double? angrate_y { set; get; }

        /// <summary>
        /// Angular rate in pitch axis (b-frame)
        /// </summary>
        public double? angrate_p { set; get; }

        /// <summary>
        /// Angular rate in roll axis (b-frame)
        /// </summary>
        public double? angrate_r { set; get; }

        /// <summary>
        /// 风速单位(K/M/N)
        /// </summary>
        public string wind_speed_unit { set; get; }

        /// <summary>
        /// 风速
        /// </summary>
        public double? wind_speed { set; get; }

        /// <summary>
        /// 参考(R/T)
        /// </summary>
        public string reference { set; get; }

        /// <summary>
        /// 风角
        /// </summary>
        public double? wind_angle { set; get; }

        /// <summary>
        /// 数据创建时间
        /// </summary>
        public long? created_at { set; get; }

        /// <summary>
        /// 数据更新时间
        /// </summary>
        public long? updated_at { set; get; }

    }


    public class SHXAFileRequest
    {
        public SHXAFileRequest()
        {

        }

        public string FileName { set; get; }

        public string FilePath { set; get; }

        public string FileContent { set; get; }

    }
}
