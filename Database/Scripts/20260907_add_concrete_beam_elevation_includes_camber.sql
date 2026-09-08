-- 混凝土梁设计坐标导入：记录页面“标高含预拱度”勾选状态。
ALTER TABLE "biz_concrete_beam_config"
    ADD COLUMN IF NOT EXISTS "ElevationIncludesCamber" boolean NOT NULL DEFAULT false;

COMMENT ON COLUMN "biz_concrete_beam_config"."ElevationIncludesCamber"
    IS '导入设计坐标时是否勾选标高含预拱度：false-否，true-是';
