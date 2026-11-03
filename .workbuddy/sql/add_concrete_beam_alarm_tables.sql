-- 混凝土梁【误差分析与报警】：新建预警配置表 + 预警记录表
-- 目标库：Kingbase V9（PostgreSQL 兼容）
-- 说明：
--   1. 预警配置每浇筑组一行；六个阈值列均可空（空 = 该等级未配置）。
--   2. 预警记录幂等键：浇筑组 + 节点 + 测线(E/C/W) + 等级。
--   3. 表已存在时跳过（IF NOT EXISTS）。

CREATE TABLE IF NOT EXISTS biz_concrete_beam_alarm_config (
    ID             SERIAL PRIMARY KEY,
    ProjID         INTEGER       NOT NULL,
    GirderID       INTEGER       NOT NULL,
    CastingGroupID INTEGER       NOT NULL,
    Level1Min      NUMERIC(20,4),
    Level1Max      NUMERIC(20,4),
    Level2Min      NUMERIC(20,4),
    Level2Max      NUMERIC(20,4),
    Level3Min      NUMERIC(20,4),
    Level3Max      NUMERIC(20,4),
    Status         INTEGER       NOT NULL DEFAULT 0,
    CreatedBy      VARCHAR(100),
    CreatedTime    TIMESTAMP(6),
    UpdatedBy      VARCHAR(100),
    UpdatedTime    TIMESTAMP(6)
);

CREATE UNIQUE INDEX IF NOT EXISTS uidx_cbeam_alarm_config_cg
    ON biz_concrete_beam_alarm_config (CastingGroupID)
    WHERE Status <> -1;

CREATE TABLE IF NOT EXISTS biz_concrete_beam_alarm_record (
    ID                 SERIAL PRIMARY KEY,
    ProjID             INTEGER       NOT NULL,
    GirderID           INTEGER       NOT NULL,
    CastingGroupID     INTEGER       NOT NULL,
    SegmentID          INTEGER       NOT NULL,
    DesignCoordinateID INTEGER       NOT NULL,
    NodeName           VARCHAR(100)  NOT NULL,
    LineType           VARCHAR(10)   NOT NULL,
    Deviation          NUMERIC(20,4) NOT NULL,
    Level              INTEGER       NOT NULL,
    AlarmTime          TIMESTAMP(6)  NOT NULL,
    Status             INTEGER       NOT NULL DEFAULT 0,
    CreatedBy          VARCHAR(100),
    CreatedTime        TIMESTAMP(6),
    UpdatedBy          VARCHAR(100),
    UpdatedTime        TIMESTAMP(6)
);

CREATE UNIQUE INDEX IF NOT EXISTS uidx_cbeam_alarm_record_node
    ON biz_concrete_beam_alarm_record (CastingGroupID, DesignCoordinateID, LineType, Level)
    WHERE Status <> -1;
