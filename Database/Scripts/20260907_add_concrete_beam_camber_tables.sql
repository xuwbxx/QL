CREATE TABLE IF NOT EXISTS "biz_concrete_beam_camber_source" (
    "ID" SERIAL PRIMARY KEY,
    "ProjID" integer NOT NULL, "GirderID" integer NOT NULL, "CastingGroupID" integer NOT NULL,
    "NodeName" varchar(100) NOT NULL, "ZE" numeric(20,4), "ZC" numeric(20,4) NOT NULL, "ZW" numeric(20,4),
    "Status" integer NOT NULL DEFAULT 0, "CreatedBy" varchar(100), "CreatedTime" timestamp, "UpdatedBy" varchar(100), "UpdatedTime" timestamp
);
CREATE INDEX IF NOT EXISTS "IX_camber_source_group" ON "biz_concrete_beam_camber_source" ("CastingGroupID", "Status");
COMMENT ON TABLE "biz_concrete_beam_camber_source" IS '混凝土梁预拱度导入源表，保留原始ZE/ZC/ZW';
COMMENT ON COLUMN "biz_concrete_beam_camber_source"."ID" IS '主键ID';
COMMENT ON COLUMN "biz_concrete_beam_camber_source"."ProjID" IS '所属项目ID';
COMMENT ON COLUMN "biz_concrete_beam_camber_source"."GirderID" IS '所属梁ID，对应项目梁表主键';
COMMENT ON COLUMN "biz_concrete_beam_camber_source"."CastingGroupID" IS '所属浇筑组ID';
COMMENT ON COLUMN "biz_concrete_beam_camber_source"."NodeName" IS '节点名称，必须与设计坐标节点名称一致';
COMMENT ON COLUMN "biz_concrete_beam_camber_source"."ZE" IS '东侧预拱度原始值，单位：米，允许为空';
COMMENT ON COLUMN "biz_concrete_beam_camber_source"."ZC" IS '中间预拱度原始值，单位：米，不允许为空';
COMMENT ON COLUMN "biz_concrete_beam_camber_source"."ZW" IS '西侧预拱度原始值，单位：米，允许为空';
COMMENT ON COLUMN "biz_concrete_beam_camber_source"."Status" IS '数据状态：0-正常，-1-逻辑删除';
COMMENT ON COLUMN "biz_concrete_beam_camber_source"."CreatedBy" IS '创建人账号';
COMMENT ON COLUMN "biz_concrete_beam_camber_source"."CreatedTime" IS '创建时间';
COMMENT ON COLUMN "biz_concrete_beam_camber_source"."UpdatedBy" IS '最后更新人账号';源表保持
COMMENT ON COLUMN "biz_concrete_beam_camber_source"."UpdatedTime" IS '最后更新时间';

CREATE TABLE IF NOT EXISTS "biz_concrete_beam_camber_calculation" (
    "ID" SERIAL PRIMARY KEY,
    "ProjID" integer NOT NULL, "GirderID" integer NOT NULL, "CastingGroupID" integer NOT NULL,
    "NodeName" varchar(100) NOT NULL, "ZE" numeric(20,4) NOT NULL, "ZC" numeric(20,4) NOT NULL, "ZW" numeric(20,4) NOT NULL,
    "Status" integer NOT NULL DEFAULT 0, "CreatedBy" varchar(100), "CreatedTime" timestamp, "UpdatedBy" varchar(100), "UpdatedTime" timestamp
);
CREATE INDEX IF NOT EXISTS "IX_camber_calculation_group" ON "biz_concrete_beam_camber_calculation" ("CastingGroupID", "Status");
COMMENT ON TABLE "biz_concrete_beam_camber_calculation" IS '混凝土梁预拱度计算表，空ZE或ZW由ZC补齐';
COMMENT ON COLUMN "biz_concrete_beam_camber_calculation"."ID" IS '主键ID';
COMMENT ON COLUMN "biz_concrete_beam_camber_calculation"."ProjID" IS '所属项目ID';
COMMENT ON COLUMN "biz_concrete_beam_camber_calculation"."GirderID" IS '所属梁ID，对应项目梁表主键';
COMMENT ON COLUMN "biz_concrete_beam_camber_calculation"."CastingGroupID" IS '所属浇筑组ID';
COMMENT ON COLUMN "biz_concrete_beam_camber_calculation"."NodeName" IS '节点名称，与源表及设计坐标节点名称一致';
COMMENT ON COLUMN "biz_concrete_beam_camber_calculation"."ZE" IS '东侧预拱度计算值，源值为空时取ZC，单位：米';
COMMENT ON COLUMN "biz_concrete_beam_camber_calculation"."ZC" IS '中间预拱度计算值，单位：米';
COMMENT ON COLUMN "biz_concrete_beam_camber_calculation"."ZW" IS '西侧预拱度计算值，源值为空时取ZC，单位：米';
COMMENT ON COLUMN "biz_concrete_beam_camber_calculation"."Status" IS '数据状态：0-正常，-1-逻辑删除';
COMMENT ON COLUMN "biz_concrete_beam_camber_calculation"."CreatedBy" IS '创建人账号';
COMMENT ON COLUMN "biz_concrete_beam_camber_calculation"."CreatedTime" IS '创建时间';
COMMENT ON COLUMN "biz_concrete_beam_camber_calculation"."UpdatedBy" IS '最后更新人账号';
COMMENT ON COLUMN "biz_concrete_beam_camber_calculation"."UpdatedTime" IS '最后更新时间';
