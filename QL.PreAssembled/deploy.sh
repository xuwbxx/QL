#!/bin/bash
set -euo pipefail

# 变量定义
SITE_DIR="/var/www/qlprea"
BASE_DEV="/devops/qlpreassembled"
BACKUP_ROOT="${BASE_DEV}/backup"
TODEPLOY_DIR="${BASE_DEV}/todeploy"
INNER_SUB_DIR_NAME="QL.PreAssembled"

# 传入参数：发布包文件名
PACKAGE_NAME="$1"
PACKAGE_PATH="${BASE_DEV}/${PACKAGE_NAME}"

# 校验输入参数
if [[ -z "${PACKAGE_NAME}" ]]; then
    echo "用法: $0 <tar包文件名>"
    echo "示例: $0 qrsite.20260831_001.tar"
    exit 1
fi

# 校验发布包文件是否存在
if [[ ! -f "${PACKAGE_PATH}" ]]; then
    echo "错误：发布包不存在 ${PACKAGE_PATH}"
    exit 1
fi

# 备份目录，以发布包名称作为子目录名
BACKUP_DIR="${BACKUP_ROOT}/${PACKAGE_NAME}"

echo "==================== 开始部署 ${PACKAGE_NAME} ===================="
echo "发布包: ${PACKAGE_PATH}"
echo "站点目录: ${SITE_DIR}"
echo "备份目录: ${BACKUP_DIR}"
echo "解压临时目录: ${TODEPLOY_DIR}"

# 1. 清理并创建todeploy临时目录
rm -rf "${TODEPLOY_DIR}"
mkdir -p "${TODEPLOY_DIR}"

# 解压缩发布包到 todeploy
echo "1/4 解压发布包到 ${TODEPLOY_DIR}"
tar -xf "${PACKAGE_PATH}" -C "${TODEPLOY_DIR}"

# 判断解压后是否存在内层 QL.PreAssembled 目录
DEPLOY_SOURCE_DIR="${TODEPLOY_DIR}"
INNER_FULL_PATH="${TODEPLOY_DIR}/${INNER_SUB_DIR_NAME}"
if [[ -d "${INNER_FULL_PATH}" ]]; then
    echo "检测到内层目录 ${INNER_SUB_DIR_NAME}，将使用该目录作为发布源"
    DEPLOY_SOURCE_DIR="${INNER_FULL_PATH}"
fi

# 2. 备份站点现有文件到backup下的子目录
mkdir -p "${BACKUP_DIR}"
echo "2/4 备份站点文件到 ${BACKUP_DIR}"
# 复制整个站点目录内容到备份目录
cp -a "${SITE_DIR}/." "${BACKUP_DIR}/"

# 3. 将源目录文件覆盖复制到站点目录
echo "3/4 复制新文件到站点目录 ${SITE_DIR}，源目录:${DEPLOY_SOURCE_DIR}"
cp -a "${DEPLOY_SOURCE_DIR}/." "${SITE_DIR}/"

# 4. 重启systemd服务 qlprea
echo "4/4 执行 systemctl restart qlprea"
systemctl restart qlprea

# 简单校验服务状态
echo "校验 qlprea 服务状态..."
systemctl is-active --quiet qlprea
if [[ $? -eq 0 ]]; then
    echo "qlprea 服务启动成功"
else
    echo "【警告】qlprea 服务重启后状态异常，请执行 journalctl -u qlprea 查看日志"
fi

echo "==================== 部署完成 ${PACKAGE_NAME} ===================="
echo "备份位置：${BACKUP_DIR}"
echo "临时解压目录保留：${TODEPLOY_DIR}"