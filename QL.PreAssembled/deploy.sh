#!/bin/bash
set -euo pipefail

# ==================== 变量定义 ====================
SITE_DIR="/website/qlprea"
BASE_DEV="/devops/qlprea"
BACKUP_ROOT="${BASE_DEV}/backup"
TODEPLOY_DIR="${BASE_DEV}/todeploy"

# 显式内层目录名（逗号分隔，按优先级从左到右），可为空
INNER_SUB_DIR_NAMES="QL.PreAssembled,QL.Preassembled,publish"

# ★ 主程序集文件名（发布根特征之一）
MAIN_DLL_NAME="QL.PreAssembled.dll"

# 服务名（优先使用 qlpre；不存在时回退到 jexus）
SERVICE_NAME="qlpre"

# 传入参数：发布包文件名
PACKAGE_NAME="${1:-}"
PACKAGE_PATH="${BASE_DEV}/${PACKAGE_NAME}"

# ==================== 参数校验 ====================
if [[ -z "${PACKAGE_NAME}" ]]; then
    echo "用法: $0 <tar包文件名>"
    echo "示例: $0 qrsite.20260903_001.tar"
    echo "支持格式: .tar / .tar.gz / .tgz / .tar.bz2 / .tbz2 / .tar.xz / .txz / .zip"
    exit 1
fi

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

# ==================== 1. 解压（按后缀自动选择方式） ====================
rm -rf "${TODEPLOY_DIR}"
mkdir -p "${TODEPLOY_DIR}"
echo "1/5 解压发布包到 ${TODEPLOY_DIR}"

case "${PACKAGE_NAME}" in
    *.tar.gz|*.tgz)
        echo "  检测到 gzip 压缩包，使用 tar -xzf"
        tar -xzf "${PACKAGE_PATH}" -C "${TODEPLOY_DIR}"
        ;;
    *.tar.bz2|*.tbz2)
        echo "  检测到 bzip2 压缩包，使用 tar -xjf"
        tar -xjf "${PACKAGE_PATH}" -C "${TODEPLOY_DIR}"
        ;;
    *.tar.xz|*.txz)
        echo "  检测到 xz 压缩包，使用 tar -xJf"
        tar -xJf "${PACKAGE_PATH}" -C "${TODEPLOY_DIR}"
        ;;
    *.tar)
        echo "  检测到 tar 包，使用 tar -xf"
        tar -xf "${PACKAGE_PATH}" -C "${TODEPLOY_DIR}"
        ;;
    *.zip)
        echo "  检测到 zip 包，使用 unzip"
        command -v unzip >/dev/null 2>&1 || { echo "错误：未安装 unzip 命令"; exit 1; }
        unzip -q "${PACKAGE_PATH}" -d "${TODEPLOY_DIR}"
        ;;
    *)
        echo "错误：不支持的压缩包格式 ${PACKAGE_NAME}"
        exit 1
        ;;
esac

# 解压结果校验（防止空目录覆盖站点）
if [[ -z "$(ls -A "${TODEPLOY_DIR}")" ]]; then
    echo "错误：解压后目录为空或解压失败"
    exit 1
fi
echo "  解压完成，内容如下："
ls -la "${TODEPLOY_DIR}"

# ==================== 2. 定位发布源目录 ====================
# 判定标志（满足任一即为发布根）：
#   ① 含 wwwroot 目录
#   ② 含主程序集 QL.PreAssembled.dll（精确名，非任意 *.dll）
# 查找顺序：显式目录名 → 根目录特征 → 一级子目录特征 → 兜底
DEPLOY_SOURCE_DIR="${TODEPLOY_DIR}"
FOUND_INNER=""

# 判断一个目录是否满足"发布根特征"
is_deploy_root() {
    local dir="$1"
    [[ -d "${dir}/wwwroot" ]] && return 0
    [[ -f "${dir}/${MAIN_DLL_NAME}" ]] && return 0
    # 兼容大小写：查找精确文件名（不区分大小写）
    find "${dir}" -maxdepth 1 -type f -iname "${MAIN_DLL_NAME}" 2>/dev/null | grep -q . && return 0
    return 1
}

# ① 显式目录名匹配
IFS=',' read -ra INNER_DIRS <<< "${INNER_SUB_DIR_NAMES}"
for dir_name in "${INNER_DIRS[@]}"; do
    dir_name_trimmed="$(echo "${dir_name}" | xargs)"
    if [[ -n "${dir_name_trimmed}" && -d "${TODEPLOY_DIR}/${dir_name_trimmed}" ]]; then
        DEPLOY_SOURCE_DIR="${TODEPLOY_DIR}/${dir_name_trimmed}"
        FOUND_INNER="${dir_name_trimmed}"
        echo "2/5 按显式目录名命中发布源：${FOUND_INNER}"
        break
    fi
done

# ② 特征探测（仅当显式名未命中）
if [[ -z "${FOUND_INNER}" ]]; then
    echo "2/5 显式目录名未命中，开始特征探测..."

    # 优先：根目录即发布根
    if is_deploy_root "${TODEPLOY_DIR}"; then
        DEPLOY_SOURCE_DIR="${TODEPLOY_DIR}"
        FOUND_INNER="$(basename "${TODEPLOY_DIR}")"
        echo "  解压根目录满足发布根特征，即为发布源"

    # 再找：一级子目录
    else
        for subdir in "${TODEPLOY_DIR}"/*/; do
            [[ -d "${subdir}" ]] || continue
            if is_deploy_root "${subdir%/}"; then
                DEPLOY_SOURCE_DIR="${subdir%/}"
                FOUND_INNER="$(basename "${subdir}")"
                echo "  子目录满足发布根特征：${FOUND_INNER}"
                break
            fi
        done
    fi
fi

# ③ 兜底提示
if [[ -z "${FOUND_INNER}" ]]; then
    echo "2/5 未定位到内层发布目录，使用解压根目录作为发布源"
else
    echo "  最终发布源：${DEPLOY_SOURCE_DIR}"
fi

echo "  发布源文件数：$(find "${DEPLOY_SOURCE_DIR}" -type f | wc -l)"

# 发布源非空校验
if [[ -z "$(ls -A "${DEPLOY_SOURCE_DIR}")" ]]; then
    echo "错误：发布源目录为空 ${DEPLOY_SOURCE_DIR}"
    exit 1
fi

# ==================== 3. 备份站点 ====================
mkdir -p "${BACKUP_DIR}"
echo "3/5 备份站点文件到 ${BACKUP_DIR}"
cp -a "${SITE_DIR}/." "${BACKUP_DIR}/"

# ==================== 4. 复制新文件到站点 ====================
echo "4/5 复制新文件到站点目录 ${SITE_DIR}"
echo "  源目录: ${DEPLOY_SOURCE_DIR}"
cp -a "${DEPLOY_SOURCE_DIR}/." "${SITE_DIR}/"

# ==================== 5. 重启服务 ====================
echo "5/5 重启服务"

# 优先使用 qlpre service；不存在则回退到 jexus
if systemctl list-unit-files --type=service 2>/dev/null | grep -q "^${SERVICE_NAME}\.service"; then
    echo "  检测到 systemd 服务 ${SERVICE_NAME}，执行 systemctl restart ${SERVICE_NAME}"
    systemctl restart "${SERVICE_NAME}"

    if systemctl is-active --quiet "${SERVICE_NAME}"; then
        echo "  ${SERVICE_NAME} 服务启动成功"
    else
        echo "【警告】${SERVICE_NAME} 服务重启后状态异常，请执行 journalctl -u ${SERVICE_NAME} 查看日志"
        exit 1
    fi
else
    echo "  未检测到 ${SERVICE_NAME} service，回退到 jexus"

    echo "  执行 /usr/jexus/jws stop"
    if /usr/jexus/jws stop; then
        echo "  jexus 已停止，等待 2 秒..."
        sleep 2
    else
        echo "  jexus stop 失败（可能未运行），继续执行 start"
        sleep 2
    fi

    echo "  执行 /usr/jexus/jws start"
    /usr/jexus/jws start

    # jexus 没有 systemd 状态，用进程检查
    if pgrep -f "jws" >/dev/null 2>&1; then
        echo "  jws 服务启动成功"
    else
        echo "【警告】jws 服务重启后状态异常，请检查 jexus 日志"
        exit 1
    fi
fi

echo "==================== 部署完成 ${PACKAGE_NAME} ===================="
echo "备份位置：${BACKUP_DIR}"
echo "临时解压目录保留：${TODEPLOY_DIR}"