BEGIN;

-- 原设计坐标表改为源表，保留全部历史数据。
ALTER TABLE "biz_concrete_beam_design_coordinate"
    RENAME TO "biz_concrete_beam_design_coordinate_source";

CREATE INDEX IF NOT EXISTS "IX_design_coordinate_source_group"
    ON "biz_concrete_beam_design_coordinate_source" ("CastingGroupID", "Status");

COMMENT ON TABLE "biz_concrete_beam_design_coordinate_source" IS '混凝土梁设计坐标导入源表，原样保留导入数据';
COMMENT ON COLUMN "biz_concrete_beam_design_coordinate_source"."ID" IS '主键ID';
COMMENT ON COLUMN "biz_concrete_beam_design_coordinate_source"."ProjID" IS '所属项目ID';
COMMENT ON COLUMN "biz_concrete_beam_design_coordinate_source"."GirderID" IS '所属梁ID，对应项目梁表主键';
COMMENT ON COLUMN "biz_concrete_beam_design_coordinate_source"."CastingGroupID" IS '所属浇筑组ID';
COMMENT ON COLUMN "biz_concrete_beam_design_coordinate_source"."NodeName" IS '节点名称，同一浇筑组内唯一';
COMMENT ON COLUMN "biz_concrete_beam_design_coordinate_source"."XE" IS '东侧控制点X原始坐标，单位：米';
COMMENT ON COLUMN "biz_concrete_beam_design_coordinate_source"."YE" IS '东侧控制点Y原始坐标，单位：米';
COMMENT ON COLUMN "biz_concrete_beam_design_coordinate_source"."ZE" IS '东侧控制点Z原始坐标，单位：米';
COMMENT ON COLUMN "biz_concrete_beam_design_coordinate_source"."XC" IS '中间控制点X原始坐标，单位：米';
COMMENT ON COLUMN "biz_concrete_beam_design_coordinate_source"."YC" IS '中间控制点Y原始坐标，单位：米';
COMMENT ON COLUMN "biz_concrete_beam_design_coordinate_source"."ZC" IS '中间控制点Z原始坐标，单位：米';
COMMENT ON COLUMN "biz_concrete_beam_design_coordinate_source"."XW" IS '西侧控制点X原始坐标，单位：米';
COMMENT ON COLUMN "biz_concrete_beam_design_coordinate_source"."YW" IS '西侧控制点Y原始坐标，单位：米';
COMMENT ON COLUMN "biz_concrete_beam_design_coordinate_source"."ZW" IS '西侧控制点Z原始坐标，单位：米';
COMMENT ON COLUMN "biz_concrete_beam_design_coordinate_source"."Status" IS '数据状态：0-正常，-1-逻辑删除';
COMMENT ON COLUMN "biz_concrete_beam_design_coordinate_source"."CreatedBy" IS '创建人账号';
COMMENT ON COLUMN "biz_concrete_beam_design_coordinate_source"."CreatedTime" IS '创建时间';
COMMENT ON COLUMN "biz_concrete_beam_design_coordinate_source"."UpdatedBy" IS '最后更新人账号';
COMMENT ON COLUMN "biz_concrete_beam_design_coordinate_source"."UpdatedTime" IS '最后更新时间';

-- 计算表当前与源表字段一致，后续算法在此表的坐标字段上处理。
CREATE TABLE "biz_concrete_beam_design_coordinate_calculation" (
    "ID" SERIAL PRIMARY KEY,
    "ProjID" integer NOT NULL,
    "GirderID" integer NOT NULL,
    "CastingGroupID" integer NOT NULL,
    "NodeName" varchar(100) NOT NULL,
    "XE" numeric(20,4) NOT NULL,
    "YE" numeric(20,4) NOT NULL,
    "ZE" numeric(20,4) NOT NULL,
    "XC" numeric(20,4) NOT NULL,
    "YC" numeric(20,4) NOT NULL,
    "ZC" numeric(20,4) NOT NULL,
    "XW" numeric(20,4) NOT NULL,
    "YW" numeric(20,4) NOT NULL,
    "ZW" numeric(20,4) NOT NULL,
    "Status" integer NOT NULL DEFAULT 0,
    "CreatedBy" varchar(100),
    "CreatedTime" timestamp,
    "UpdatedBy" varchar(100),
    "UpdatedTime" timestamp
);

CREATE INDEX "IX_design_coordinate_calculation_group"
    ON "biz_concrete_beam_design_coordinate_calculation" ("CastingGroupID", "Status");

COMMENT ON TABLE "biz_concrete_beam_design_coordinate_calculation" IS '混凝土梁设计坐标计算表，当前同步源表数据，供后续算法使用';
COMMENT ON COLUMN "biz_concrete_beam_design_coordinate_calculation"."ID" IS '主键ID';
COMMENT ON COLUMN "biz_concrete_beam_design_coordinate_calculation"."ProjID" IS '所属项目ID';
COMMENT ON COLUMN "biz_concrete_beam_design_coordinate_calculation"."GirderID" IS '所属梁ID，对应项目梁表主键';
COMMENT ON COLUMN "biz_concrete_beam_design_coordinate_calculation"."CastingGroupID" IS '所属浇筑组ID';
COMMENT ON COLUMN "biz_concrete_beam_design_coordinate_calculation"."NodeName" IS '节点名称，与源表节点名称一致';
COMMENT ON COLUMN "biz_concrete_beam_design_coordinate_calculation"."XE" IS '东侧控制点X计算坐标，单位：米';
COMMENT ON COLUMN "biz_concrete_beam_design_coordinate_calculation"."YE" IS '东侧控制点Y计算坐标，单位：米';
COMMENT ON COLUMN "biz_concrete_beam_design_coordinate_calculation"."ZE" IS '东侧控制点Z计算坐标，单位：米';
COMMENT ON COLUMN "biz_concrete_beam_design_coordinate_calculation"."XC" IS '中间控制点X计算坐标，单位：米';
COMMENT ON COLUMN "biz_concrete_beam_design_coordinate_calculation"."YC" IS '中间控制点Y计算坐标，单位：米';
COMMENT ON COLUMN "biz_concrete_beam_design_coordinate_calculation"."ZC" IS '中间控制点Z计算坐标，单位：米';
COMMENT ON COLUMN "biz_concrete_beam_design_coordinate_calculation"."XW" IS '西侧控制点X计算坐标，单位：米';
COMMENT ON COLUMN "biz_concrete_beam_design_coordinate_calculation"."YW" IS '西侧控制点Y计算坐标，单位：米';
COMMENT ON COLUMN "biz_concrete_beam_design_coordinate_calculation"."ZW" IS '西侧控制点Z计算坐标，单位：米';
COMMENT ON COLUMN "biz_concrete_beam_design_coordinate_calculation"."Status" IS '数据状态：0-正常，-1-逻辑删除';
COMMENT ON COLUMN "biz_concrete_beam_design_coordinate_calculation"."CreatedBy" IS '创建人账号';
COMMENT ON COLUMN "biz_concrete_beam_design_coordinate_calculation"."CreatedTime" IS '创建时间';
COMMENT ON COLUMN "biz_concrete_beam_design_coordinate_calculation"."UpdatedBy" IS '最后更新人账号';
COMMENT ON COLUMN "biz_concrete_beam_design_coordinate_calculation"."UpdatedTime" IS '最后更新时间';

-- 将源表中的历史记录同步到计算表，按源表ID保持原始数据顺序。
INSERT INTO "biz_concrete_beam_design_coordinate_calculation" (
    "ProjID", "GirderID", "CastingGroupID", "NodeName",
    "XE", "YE", "ZE", "XC", "YC", "ZC", "XW", "YW", "ZW",
    "Status", "CreatedBy", "CreatedTime", "UpdatedBy", "UpdatedTime"
)
SELECT
    "ProjID", "GirderID", "CastingGroupID", "NodeName",
    "XE", "YE", "ZE", "XC", "YC", "ZC", "XW", "YW", "ZW",
    "Status", "CreatedBy", "CreatedTime", "UpdatedBy", "UpdatedTime"
FROM "biz_concrete_beam_design_coordinate_source"
ORDER BY "ID";

COMMIT;
