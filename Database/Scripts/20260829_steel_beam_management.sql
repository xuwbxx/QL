BEGIN;

CREATE TABLE IF NOT EXISTS "dbo"."biz_steel_beam_theoretical" (
    "ID" serial PRIMARY KEY,
    "ProjID" integer NOT NULL,
    "BridgeID" integer NOT NULL,
    "PointCode" varchar(50) NOT NULL,
    "DesignX" numeric(20,6) NOT NULL,
    "DesignY" numeric(20,6) NOT NULL,
    "DesignZ" numeric(20,6) NOT NULL,
    "PreCamber" numeric(20,6) NOT NULL,
    "Weight" numeric(5,4) NOT NULL,
    "SegmentNo" varchar(50) NOT NULL,
    "PositionName" varchar(100) NOT NULL,
    "IsFirstCoordinate" boolean NULL,
    "PositionOrder" integer NULL,
    "DistanceFromStart" numeric(20,6) NULL,
    "Version" integer NOT NULL DEFAULT 1,
    "Status" integer NOT NULL DEFAULT 0,
    "CreatedBy" varchar(100) NULL,
    "CreatedTime" timestamp without time zone NULL,
    "UpdatedBy" varchar(100) NULL,
    "UpdatedTime" timestamp without time zone NULL
);

CREATE TABLE IF NOT EXISTS "dbo"."biz_steel_beam_measure_batch" (
    "ID" serial PRIMARY KEY,
    "ProjID" integer NOT NULL,
    "BridgeID" integer NOT NULL,
    "PushCount" integer NOT NULL,
    "MeasureTime" timestamp without time zone NOT NULL,
    "ImportCount" integer NOT NULL,
    "ReplacedByBatchID" integer NULL,
    "Status" integer NOT NULL DEFAULT 0,
    "CreatedBy" varchar(100) NULL,
    "CreatedTime" timestamp without time zone NULL,
    "UpdatedBy" varchar(100) NULL,
    "UpdatedTime" timestamp without time zone NULL
);

CREATE TABLE IF NOT EXISTS "dbo"."biz_steel_beam_measured" (
    "ID" serial PRIMARY KEY,
    "ProjID" integer NOT NULL,
    "BridgeID" integer NOT NULL,
    "BatchID" integer NOT NULL,
    "TheoreticalID" integer NOT NULL,
    "PointCode" varchar(50) NOT NULL,
    "MeasuredX" numeric(20,6) NOT NULL,
    "MeasuredY" numeric(20,6) NOT NULL,
    "MeasuredZ" numeric(20,6) NOT NULL,
    "ImportTime" timestamp without time zone NOT NULL,
    "Version" integer NOT NULL DEFAULT 1,
    "Status" integer NOT NULL DEFAULT 0,
    "CreatedBy" varchar(100) NULL,
    "CreatedTime" timestamp without time zone NULL,
    "UpdatedBy" varchar(100) NULL,
    "UpdatedTime" timestamp without time zone NULL
);

COMMENT ON TABLE "dbo"."biz_steel_beam_theoretical" IS '钢梁理论数据表';
COMMENT ON COLUMN "dbo"."biz_steel_beam_theoretical"."ID" IS '主键ID';
COMMENT ON COLUMN "dbo"."biz_steel_beam_theoretical"."ProjID" IS '项目ID';
COMMENT ON COLUMN "dbo"."biz_steel_beam_theoretical"."BridgeID" IS '桥梁ID';
COMMENT ON COLUMN "dbo"."biz_steel_beam_theoretical"."PointCode" IS '测点编号（同一项目内唯一，不区分大小写）';
COMMENT ON COLUMN "dbo"."biz_steel_beam_theoretical"."DesignX" IS '设计坐标X，单位：米';
COMMENT ON COLUMN "dbo"."biz_steel_beam_theoretical"."DesignY" IS '设计坐标Y，单位：米';
COMMENT ON COLUMN "dbo"."biz_steel_beam_theoretical"."DesignZ" IS '设计坐标Z，单位：米';
COMMENT ON COLUMN "dbo"."biz_steel_beam_theoretical"."PreCamber" IS '预拱度，单位：米';
COMMENT ON COLUMN "dbo"."biz_steel_beam_theoretical"."Weight" IS '权值';
COMMENT ON COLUMN "dbo"."biz_steel_beam_theoretical"."SegmentNo" IS '所属梁段号';
COMMENT ON COLUMN "dbo"."biz_steel_beam_theoretical"."PositionName" IS '测位名称';
COMMENT ON COLUMN "dbo"."biz_steel_beam_theoretical"."IsFirstCoordinate" IS '是否为首坐标';
COMMENT ON COLUMN "dbo"."biz_steel_beam_theoretical"."PositionOrder" IS '测点顺序';
COMMENT ON COLUMN "dbo"."biz_steel_beam_theoretical"."DistanceFromStart" IS '距起点距离，单位：米';
COMMENT ON COLUMN "dbo"."biz_steel_beam_theoretical"."Version" IS '乐观锁版本号';
COMMENT ON COLUMN "dbo"."biz_steel_beam_theoretical"."Status" IS '数据状态：0正常，-1逻辑删除';
COMMENT ON COLUMN "dbo"."biz_steel_beam_theoretical"."CreatedBy" IS '创建人';
COMMENT ON COLUMN "dbo"."biz_steel_beam_theoretical"."CreatedTime" IS '创建时间';
COMMENT ON COLUMN "dbo"."biz_steel_beam_theoretical"."UpdatedBy" IS '最后更新人';
COMMENT ON COLUMN "dbo"."biz_steel_beam_theoretical"."UpdatedTime" IS '最后更新时间';

COMMENT ON TABLE "dbo"."biz_steel_beam_measure_batch" IS '钢梁实测数据导入批次表';
COMMENT ON COLUMN "dbo"."biz_steel_beam_measure_batch"."ID" IS '主键ID';
COMMENT ON COLUMN "dbo"."biz_steel_beam_measure_batch"."ProjID" IS '项目ID';
COMMENT ON COLUMN "dbo"."biz_steel_beam_measure_batch"."BridgeID" IS '桥梁ID';
COMMENT ON COLUMN "dbo"."biz_steel_beam_measure_batch"."PushCount" IS '顶推次数，从0开始';
COMMENT ON COLUMN "dbo"."biz_steel_beam_measure_batch"."MeasureTime" IS '测量时间，精确到小时';
COMMENT ON COLUMN "dbo"."biz_steel_beam_measure_batch"."ImportCount" IS '本批次导入数据条数';
COMMENT ON COLUMN "dbo"."biz_steel_beam_measure_batch"."ReplacedByBatchID" IS '覆盖本批次的新批次ID';
COMMENT ON COLUMN "dbo"."biz_steel_beam_measure_batch"."Status" IS '数据状态：0正常，-1逻辑删除';
COMMENT ON COLUMN "dbo"."biz_steel_beam_measure_batch"."CreatedBy" IS '创建人';
COMMENT ON COLUMN "dbo"."biz_steel_beam_measure_batch"."CreatedTime" IS '创建时间（导入时间）';
COMMENT ON COLUMN "dbo"."biz_steel_beam_measure_batch"."UpdatedBy" IS '最后更新人';
COMMENT ON COLUMN "dbo"."biz_steel_beam_measure_batch"."UpdatedTime" IS '最后更新时间';

COMMENT ON TABLE "dbo"."biz_steel_beam_measured" IS '钢梁实测数据明细表';
COMMENT ON COLUMN "dbo"."biz_steel_beam_measured"."ID" IS '主键ID';
COMMENT ON COLUMN "dbo"."biz_steel_beam_measured"."ProjID" IS '项目ID';
COMMENT ON COLUMN "dbo"."biz_steel_beam_measured"."BridgeID" IS '桥梁ID';
COMMENT ON COLUMN "dbo"."biz_steel_beam_measured"."BatchID" IS '实测数据导入批次ID';
COMMENT ON COLUMN "dbo"."biz_steel_beam_measured"."TheoreticalID" IS '关联的理论数据ID';
COMMENT ON COLUMN "dbo"."biz_steel_beam_measured"."PointCode" IS '测点编号';
COMMENT ON COLUMN "dbo"."biz_steel_beam_measured"."MeasuredX" IS '实测坐标X，单位：米';
COMMENT ON COLUMN "dbo"."biz_steel_beam_measured"."MeasuredY" IS '实测坐标Y，单位：米';
COMMENT ON COLUMN "dbo"."biz_steel_beam_measured"."MeasuredZ" IS '实测坐标Z，单位：米';
COMMENT ON COLUMN "dbo"."biz_steel_beam_measured"."ImportTime" IS '导入时间';
COMMENT ON COLUMN "dbo"."biz_steel_beam_measured"."Version" IS '乐观锁版本号';
COMMENT ON COLUMN "dbo"."biz_steel_beam_measured"."Status" IS '数据状态：0正常，-1逻辑删除';
COMMENT ON COLUMN "dbo"."biz_steel_beam_measured"."CreatedBy" IS '创建人';
COMMENT ON COLUMN "dbo"."biz_steel_beam_measured"."CreatedTime" IS '创建时间';
COMMENT ON COLUMN "dbo"."biz_steel_beam_measured"."UpdatedBy" IS '最后更新人';
COMMENT ON COLUMN "dbo"."biz_steel_beam_measured"."UpdatedTime" IS '最后更新时间';

CREATE UNIQUE INDEX IF NOT EXISTS "UX_SteelTheory_Project_PointCode"
    ON "dbo"."biz_steel_beam_theoretical" ("ProjID", lower("PointCode")) WHERE "Status" <> -1;
CREATE INDEX IF NOT EXISTS "IX_SteelTheory_Bridge_Status"
    ON "dbo"."biz_steel_beam_theoretical" ("BridgeID", "Status");
CREATE UNIQUE INDEX IF NOT EXISTS "UX_SteelBatch_Bridge_Push_Time"
    ON "dbo"."biz_steel_beam_measure_batch" ("BridgeID", "PushCount", "MeasureTime") WHERE "Status" <> -1;
CREATE INDEX IF NOT EXISTS "IX_SteelBatch_Bridge_Status"
    ON "dbo"."biz_steel_beam_measure_batch" ("BridgeID", "Status", "PushCount", "MeasureTime");
CREATE UNIQUE INDEX IF NOT EXISTS "UX_SteelMeasured_Batch_Point"
    ON "dbo"."biz_steel_beam_measured" ("BatchID", lower("PointCode")) WHERE "Status" <> -1;
CREATE INDEX IF NOT EXISTS "IX_SteelMeasured_Bridge_Point"
    ON "dbo"."biz_steel_beam_measured" ("BridgeID", "Status", "PointCode");

COMMIT;
