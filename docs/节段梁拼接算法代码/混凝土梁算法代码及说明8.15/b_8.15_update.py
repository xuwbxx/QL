import numpy as np
import math
import copy
import xlrd
import xlwt
# ============================================================
# 基础数据结构
# ============================================================
class Vec3:
    """三维坐标点。"""
    def __init__(self, x=0.0, y=0.0, z=0.0):
        self.X = x
        self.Y = y
        self.Z = z

class Joint:
    """一个节段断面上的三个关键点：E（东）、C（中）、W（西）。"""
    def __init__(self, e=None, c=None, w=None):
        self.E = e if e is not None else Vec3()
        self.C = c if c is not None else Vec3()
        self.W = w if w is not None else Vec3()

class RotationParams:
    """三次旋转的 sin/cos 参数（对应原 Python cs 数组）。"""
    def __init__(self):
        self.sin1 = 0.0
        self.cos1 = 0.0
        self.sin2 = 0.0
        self.cos2 = 0.0
        self.sin3 = 0.0
        self.cos3 = 0.0

class CalcResult:
    """calc 返回的结果对象。"""
    def __init__(self):
        # ---- 业务标量 ----
        self.CenterLength = 0.0
        self.SouthOffset = 0.0
        self.NorthOffset = 0.0
        self.SouthEastElev = 0.0
        self.SouthWestElev = 0.0
        self.NorthEastElev = 0.0
        self.NorthWestElev = 0.0
        self.WetCastSouthEastElev = 0.0
        self.WetCastSouthWestElev = 0.0
        self.WetCastNorthEastElev = 0.0
        self.WetCastNorthWestElev = 0.0
        # ---- Bulkhead 三量（仅匹配段使用）----
        self.BulkheadEastElev = 0.0
        self.BulkheadWestElev = 0.0
        self.BulkheadCenterOffset = 0.0
        # ---- 中间 Joint ----
        self.TheoreticalJointS = None
        self.TheoreticalJointN = None
        self.MatchJointN = None
        self.ActualPointS = None
        self.ActualPointN = None
        # ---- 旋转参数 ----
        self.cs = None

def array_to_joint(arr):
    """9 元素数组 → Joint。"""
    return Joint(
        Vec3(arr[0], arr[1], arr[2]),
        Vec3(arr[3], arr[4], arr[5]),
        Vec3(arr[6], arr[7], arr[8])
    )

# ============================================================
# 节段计算分派
# ============================================================
def calc(K, d, n, A, B, C, H):
    """按节段类型分派计算。返回 CalcResult。"""
    if n == 0:
        return zero(K, d, A, B, C, H)
    if n == 1:
        return one(K, d, A, B, C, H)
    if n == 2:
        return standard(K, d, A, B, C, H)
    raise ValueError("未知节段类型 n = {}".format(n))

# ============================================================
# 0 号节段
# ============================================================
def zero(K, d, A, B, C, H):
    """0 号节段计算。返回 CalcResult。"""
    k1 = K[0][:]
    k2 = K[1][:]
    arr1, cs = axischa(K, k1, d, A, B, C)
    arr2, _ = axischa(K, k2, d, A, B, C)
    theoreticalJointS = array_to_joint(arr1)
    theoreticalJointN = array_to_joint(arr2)
    calcRes = CalcResult()
    calcRes.CenterLength = theoreticalJointS.C.X
    calcRes.SouthOffset = theoreticalJointS.C.Y
    calcRes.NorthOffset = theoreticalJointN.C.Y
    calcRes.SouthEastElev = H + theoreticalJointS.E.Z
    calcRes.SouthWestElev = H + theoreticalJointS.W.Z
    calcRes.NorthEastElev = H + theoreticalJointN.E.Z
    calcRes.NorthWestElev = H + theoreticalJointN.W.Z
    calcRes.TheoreticalJointS = copy.deepcopy(theoreticalJointS)
    calcRes.TheoreticalJointN = copy.deepcopy(theoreticalJointN)
    calcRes.cs = cs
    return calcRes

# ============================================================
# 1 号节段
# ============================================================
def one(K, d, A, B, C, H):
    """1 号节段（首次匹配浇筑段）计算。返回 CalcResult。"""
    k1 = K[0][:]
    k2 = K[1][:]
    k3 = K[3][:]
    k4 = K[4][:]
    k5 = K[2][:]
    arr1, cs = axischa(K, k1, d, A, B, C)
    arr2, _ = axischa(K, k2, d, A, B, C)
    arr3, _ = axischa(K, k3, d, A, B, C)
    arr4, _ = axischa(K, k4, d, A, B, C)
    arr5, _ = axischa(K, k5, d, A, B, C)
    theoreticalJointS = array_to_joint(arr1)   # KK[0]
    theoreticalJointN = array_to_joint(arr2)   # KK[1]
    matchJointN = array_to_joint(arr5)         # KK[2]
    actualPointS = array_to_joint(arr3)        # KK[3]
    actualPointN = array_to_joint(arr4)        # KK[4]
    calcRes = CalcResult()
    calcRes.CenterLength = theoreticalJointS.C.X
    calcRes.SouthOffset = actualPointS.C.Y
    calcRes.NorthOffset = actualPointN.C.Y
    calcRes.SouthEastElev = H + actualPointS.E.Z
    calcRes.SouthWestElev = H + actualPointS.W.Z
    calcRes.NorthEastElev = H + actualPointN.E.Z
    calcRes.NorthWestElev = H + actualPointN.W.Z
    calcRes.WetCastSouthEastElev = H + theoreticalJointS.E.Z
    calcRes.WetCastSouthWestElev = H + theoreticalJointS.W.Z
    calcRes.WetCastNorthEastElev = H + theoreticalJointN.E.Z
    calcRes.WetCastNorthWestElev = H + theoreticalJointN.W.Z
    calcRes.BulkheadEastElev = H + theoreticalJointN.E.Z
    calcRes.BulkheadWestElev = H + theoreticalJointN.W.Z
    calcRes.BulkheadCenterOffset = theoreticalJointN.C.Y
    calcRes.TheoreticalJointS = copy.deepcopy(theoreticalJointS)
    calcRes.TheoreticalJointN = copy.deepcopy(theoreticalJointN)
    calcRes.MatchJointN = copy.deepcopy(matchJointN)
    calcRes.ActualPointS = copy.deepcopy(actualPointS)
    calcRes.ActualPointN = copy.deepcopy(actualPointN)
    calcRes.cs = cs
    return calcRes

# ============================================================
# 标准节段
# ============================================================
def standard(K, d, A, B, C, H):
    """标准节段计算。返回 CalcResult。"""
    k1 = K[2][:]
    k2 = K[1][:]
    k3 = K[3][:]
    k4 = K[4][:]
    k5 = K[0][:]
    arr1, cs = axischa(K, k1, d, A, B, C)
    arr2, _ = axischa(K, k2, d, A, B, C)
    arr3, _ = axischa(K, k3, d, A, B, C)
    arr4, _ = axischa(K, k4, d, A, B, C)
    arr5, _ = axischa(K, k5, d, A, B, C)
    matchJointN = array_to_joint(arr1)         # KK[2]
    theoreticalJointN = array_to_joint(arr2)   # KK[1]
    actualPointS = array_to_joint(arr3)        # KK[3]
    actualPointN = array_to_joint(arr4)        # KK[4]
    theoreticalJointS = array_to_joint(arr5)   # KK[0]
    calcRes = CalcResult()
    calcRes.CenterLength = matchJointN.C.X
    calcRes.SouthOffset = actualPointS.C.Y
    calcRes.NorthOffset = actualPointN.C.Y
    calcRes.SouthEastElev = H + actualPointS.E.Z
    calcRes.SouthWestElev = H + actualPointS.W.Z
    calcRes.NorthEastElev = H + actualPointN.E.Z
    calcRes.NorthWestElev = H + actualPointN.W.Z
    calcRes.WetCastSouthEastElev = H + matchJointN.E.Z
    calcRes.WetCastSouthWestElev = H + matchJointN.W.Z
    calcRes.WetCastNorthEastElev = H + theoreticalJointN.E.Z
    calcRes.WetCastNorthWestElev = H + theoreticalJointN.W.Z
    calcRes.BulkheadEastElev = H + theoreticalJointN.E.Z
    calcRes.BulkheadWestElev = H + theoreticalJointN.W.Z
    calcRes.BulkheadCenterOffset = theoreticalJointN.C.Y
    calcRes.TheoreticalJointS = copy.deepcopy(theoreticalJointS)
    calcRes.TheoreticalJointN = copy.deepcopy(theoreticalJointN)
    calcRes.MatchJointN = copy.deepcopy(matchJointN)
    calcRes.ActualPointS = copy.deepcopy(actualPointS)
    calcRes.ActualPointN = copy.deepcopy(actualPointN)
    calcRes.cs = cs
    return calcRes

# ============================================================
# 坐标变换（正向）
# ============================================================
def axischa(K, k, d, A, B, C):
    """全局坐标系 → 局部坐标系（三次旋转：绕 Z、绕 Y、绕 X）。
    输入输出均为 9 元素数组。返回 (K23, RotationParams)。
    """
    n = 10 ** (-200)
    K1 = []
    K2 = []
    K3 = []
    K4 = []
    for i in range(3):
        for j in range(3):
            if (3 * i + j + 1) % 3 == 0:
                K1.append((K[0][3 * i + j] - K[1][3 + j]))
                K2.append((k[3 * i + j] - K[1][3 + j]))
                K3.append((K[2][3 * i + j] - K[1][3 + j]))
                K4.append((K[1][3 * i + j] - K[1][3 + j]))
            else:
                K1.append(d * (K[0][3 * i + j] - K[1][3 + j]))
                K2.append(d * (k[3 * i + j] - K[1][3 + j]))
                K3.append(d * (K[2][3 * i + j] - K[1][3 + j]))
                K4.append(d * (K[1][3 * i + j] - K[1][3 + j]))
    # 第一次旋转（绕 Z）
    LXY = math.sqrt(K1[3] ** 2 + K1[4] ** 2)
    Sin1 = abs(K1[4] / LXY) * (K1[3] + n) / abs(K1[3] + n) * (-K1[4] + n) / abs(-K1[4] + n)
    Cos1 = abs(K1[3] / LXY)
    K11 = copy.copy(K1)
    K21 = copy.copy(K2)
    K31 = copy.copy(K3)
    K41 = copy.copy(K4)
    for i in range(3):
        K11[3 * i] = Cos1 * K1[3 * i] - Sin1 * K1[3 * i + 1]
        K11[3 * i + 1] = Sin1 * K1[3 * i] + Cos1 * K1[3 * i + 1]
        K21[3 * i] = Cos1 * K2[3 * i] - Sin1 * K2[3 * i + 1]
        K21[3 * i + 1] = Sin1 * K2[3 * i] + Cos1 * K2[3 * i + 1]
        K31[3 * i] = Cos1 * K3[3 * i] - Sin1 * K3[3 * i + 1]
        K31[3 * i + 1] = Sin1 * K3[3 * i] + Cos1 * K3[3 * i + 1]
        K41[3 * i] = Cos1 * K4[3 * i] - Sin1 * K4[3 * i + 1]
        K41[3 * i + 1] = Sin1 * K4[3 * i] + Cos1 * K4[3 * i + 1]
    # 第二次旋转（绕 Y）
    LXZ = math.sqrt(K31[3] ** 2 + K31[5] ** 2)
    Sin2 = abs(K31[5] / LXZ) * (K31[3] + n) / abs(K31[3] + n) * (-K31[5] + n) / abs(-K31[5] + n)
    Cos2 = abs(K31[3] / LXZ)
    K12 = copy.copy(K11)
    K22 = copy.copy(K21)
    K32 = copy.copy(K31)
    K42 = copy.copy(K41)
    for i in range(3):
        K12[3 * i] = Cos2 * K11[3 * i] - Sin2 * K11[3 * i + 2]
        K12[3 * i + 2] = Sin2 * K11[3 * i] + Cos2 * K11[3 * i + 2]
        K22[3 * i] = Cos2 * K21[3 * i] - Sin2 * K21[3 * i + 2]
        K22[3 * i + 2] = Sin2 * K21[3 * i] + Cos2 * K21[3 * i + 2]
        K32[3 * i] = Cos2 * K31[3 * i] - Sin2 * K31[3 * i + 2]
        K32[3 * i + 2] = Sin2 * K31[3 * i] + Cos2 * K31[3 * i + 2]
        K42[3 * i] = Cos2 * K41[3 * i] - Sin2 * K41[3 * i + 2]
        K42[3 * i + 2] = Sin2 * K41[3 * i] + Cos2 * K41[3 * i + 2]
    # 第三次旋转（绕 X）
    LYZ = math.sqrt(K42[7] ** 2 + K42[8] ** 2)
    Sin3 = abs(K42[8] / LYZ) * (K42[8] + n) / abs(K42[8] + n) * (-K42[7] + n) / abs(-K42[7] + n)
    Cos3 = abs(K42[7] / LYZ)
    K13 = copy.copy(K12)
    K23 = copy.copy(K22)
    K33 = copy.copy(K32)
    K43 = copy.copy(K42)
    for i in range(3):
        K13[3 * i + 1] = Cos3 * K12[3 * i + 1] - Sin3 * K12[3 * i + 2]
        K13[3 * i + 2] = Sin3 * K12[3 * i + 1] + Cos3 * K12[3 * i + 2]
        K23[3 * i + 1] = Cos3 * K22[3 * i + 1] - Sin3 * K22[3 * i + 2]
        K23[3 * i + 2] = Sin3 * K22[3 * i + 1] + Cos3 * K22[3 * i + 2]
        K33[3 * i + 1] = Cos3 * K32[3 * i + 1] - Sin3 * K32[3 * i + 2]
        K33[3 * i + 2] = Sin3 * K32[3 * i + 1] + Cos3 * K32[3 * i + 2]
        K43[3 * i + 1] = Cos3 * K42[3 * i + 1] - Sin3 * K42[3 * i + 2]
        K43[3 * i + 2] = Sin3 * K42[3 * i + 1] + Cos3 * K42[3 * i + 2]
    cs = RotationParams()
    cs.sin1 = Sin1
    cs.cos1 = Cos1
    cs.sin2 = Sin2
    cs.cos2 = Cos2
    cs.sin3 = Sin3
    cs.cos3 = Cos3
    return K23, cs

# ============================================================
# 坐标变换（反向）
# ============================================================
def revaxischa(K, d, k, cs):
    """局部坐标系 → 全局坐标系（反向三次旋转 + 平移回基准点）。
    输入输出均为 9 元素数组；cs 为 RotationParams 对象。
    """
    Sin1 = cs.sin1
    Cos1 = cs.cos1
    Sin2 = cs.sin2
    Cos2 = cs.cos2
    Sin3 = cs.sin3
    Cos3 = cs.cos3
    # 反向绕 X
    K0 = copy.copy(k)
    for i in range(3):
        K0[3 * i + 1] = Cos3 * k[3 * i + 1] + Sin3 * k[3 * i + 2]
        K0[3 * i + 2] = -Sin3 * k[3 * i + 1] + Cos3 * k[3 * i + 2]
    # 反向绕 Y
    K1 = copy.copy(K0)
    for i in range(3):
        K1[3 * i] = Cos2 * K0[3 * i] + Sin2 * K0[3 * i + 2]
        K1[3 * i + 2] = -Sin2 * K0[3 * i] + Cos2 * K0[3 * i + 2]
    # 反向绕 Z
    K2 = copy.copy(K1)
    for i in range(3):
        K2[3 * i] = Cos1 * K1[3 * i] + Sin1 * K1[3 * i + 1]
        K2[3 * i + 1] = -Sin1 * K1[3 * i] + Cos1 * K1[3 * i + 1]
    # 平移回基准点
    K3 = []
    for i in range(3):
        for j in range(3):
            if (3 * i + j + 1) % 3 == 0:
                K3.append(K2[3 * i + j] + K[1][3 + j])
            else:
                K3.append(d * K2[3 * i + j] + K[1][3 + j])
    return K3

# ============================================================
# 误差修正时的额外坐标变换【已修复：Joint对象不再做下标访问】
# ============================================================
def spaxis(actualPointS: Joint, rerr, k):
    """误差修正时额外的坐标变换。
    输入输出均为 9 元素数组；rerr 保持 24 项数组。
    """
    ka = [actualPointS.C.X, actualPointS.C.Y, actualPointS.C.Z]
    rk = [0] * 9
    for i in range(3):
        rk[3 * i] = k[3 * i] - ka[0]
        rk[3 * i + 1] = k[3 * i + 1] - ka[1] - rerr[20]
        rk[3 * i + 2] = k[3 * i + 2] - ka[2] - rerr[8]
    sin1 = rerr[14]
    cos1 = rerr[15]
    sin2 = rerr[18]
    cos2 = rerr[19]
    sin3 = rerr[22]
    cos3 = rerr[23]
    rk1 = copy.copy(rk)
    for i in range(3):
        rk1[3 * i] = rk[3 * i] * cos1 - rk[3 * i + 1] * sin1
        rk1[3 * i + 1] = rk[3 * i] * sin1 + rk[3 * i + 1] * cos1
    rk2 = copy.copy(rk1)
    for i in range(3):
        rk2[3 * i] = rk1[3 * i] * cos2 - rk1[3 * i + 2] * sin2
        rk2[3 * i + 2] = rk1[3 * i] * sin2 + rk1[3 * i + 2] * cos2
    rk3 = copy.copy(rk2)
    for i in range(3):
        rk3[3 * i + 1] = rk2[3 * i + 1] * cos3 - rk2[3 * i + 2] * sin3
        rk3[3 * i + 2] = rk2[3 * i + 1] * sin3 + rk2[3 * i + 2] * cos3
    for i in range(3):
        rk[3 * i] = rk3[3 * i] + ka[0]
        rk[3 * i + 1] = rk3[3 * i + 1] + ka[1]
        rk[3 * i + 2] = rk3[3 * i + 2] + ka[2]
    rk[2] = rk[2] + rerr[9] / 2
    rk[8] = rk[8] - rerr[9] / 2
    return rk

# ============================================================
# 0 号节段误差修正【修复：KCB[1] 改为 theoreticalJointS.E.Y】
# ============================================================
def zeroerr(E, bo, A, C, calcRes, K, d, cs):
    """0 号节段实测误差修正。返回四个 9 元素数组。"""
    AN = bo[0]
    AS = bo[1]
    CN = bo[2]
    CS = bo[3]
    theoreticalJointS = calcRes.TheoreticalJointS
    theoreticalJointN = calcRes.TheoreticalJointN
    southEastElev = calcRes.SouthEastElev
    southWestElev = calcRes.SouthWestElev
    northEastElev = calcRes.NorthEastElev
    northWestElev = calcRes.NorthWestElev
    # ---- KCB（南侧浇筑）----
    KCB = [0] * 9
    KCB[0] = E[3] - AS
    KCB[1] = E[2] + theoreticalJointS.E.Y   # <<==== FIXED 原错用 .C.Y
    KCB[2] = theoreticalJointS.E.Z + E[4] - southEastElev
    KCB[6] = E[1] - AS
    KCB[7] = E[2] + theoreticalJointS.W.Y
    KCB[8] = theoreticalJointS.W.Z + E[6] - southWestElev
    KCB[3] = (E[1] + E[3]) / 2 - CS
    KCB[4] = theoreticalJointS.C.Y + E[12]
    KCB[5] = theoreticalJointS.C.Z
    # ---- KCF（北侧浇筑）----
    KCF = [0] * 9
    KCF[0] = theoreticalJointN.E.X + AN
    KCF[1] = theoreticalJointN.E.Y
    KCF[2] = theoreticalJointN.E.Z + E[5] - northEastElev
    KCF[6] = theoreticalJointN.W.X + AN
    KCF[7] = theoreticalJointN.W.Y
    KCF[8] = theoreticalJointN.W.Z + E[7] - northWestElev
    KCF[3] = theoreticalJointN.C.X + CN
    KCF[4] = theoreticalJointN.C.Y + E[13]
    KCF[5] = theoreticalJointN.C.Z
    # ---- KB（南侧节点）----
    KB = [0] * 9
    KB[0] = E[3]
    KB[1] = KCB[1]
    KB[2] = E[8] - southEastElev
    KB[6] = E[1]
    KB[7] = KCB[7]
    KB[8] = E[9] - southEastElev
    KB[3] = (KB[0] + KB[6]) / 2
    KB[4] = E[2]
    KB[5] = KCB[5]
    # ---- KF（北侧节点）----
    KF = [0] * 9
    KF[0] = theoreticalJointN.E.X
    KF[1] = theoreticalJointN.E.Y
    KF[2] = theoreticalJointN.E.Z + E[10] - northEastElev
    KF[6] = theoreticalJointN.W.X
    KF[7] = theoreticalJointN.W.Y
    KF[8] = theoreticalJointN.W.Z + E[11] - northWestElev
    KF[3] = theoreticalJointN.C.X
    KF[4] = theoreticalJointN.C.Y
    KF[5] = theoreticalJointN.C.Z
    KCBR = revaxischa(K, d, KCB, cs)
    KCFR = revaxischa(K, d, KCF, cs)
    KBR = revaxischa(K, d, KB, cs)
    KFR = revaxischa(K, d, KF, cs)
    return KCBR, KCFR, KBR, KFR

# ============================================================
# 1 号/标准节段误差修正【9处全部修复完成】
# ============================================================
def oneerr(E, bo, A, C, B, H, calcRes, K, d, cs):
    """1 号/标准节段实测误差修正。返回三个 9 元素数组。"""
    n = 10 ** (-200)
    AN1 = bo[0]
    CN1 = bo[1]
    AN2 = bo[2]
    AS2 = bo[3]
    CN2 = bo[4]
    CS2 = bo[5]
    # ---- 中间 Joint ----
    theoreticalJointS = calcRes.TheoreticalJointS
    theoreticalJointN = calcRes.TheoreticalJointN
    matchJointN = calcRes.MatchJointN
    actualPointS = calcRes.ActualPointS
    actualPointN = calcRes.ActualPointN

    # 关键：区分 wetCastJoint，适配 n=1 / n=2(standard)
    # n=1: wetCastJoint = theoreticalJointS
    # n=2: wetCastJoint = matchJointN
    # 判断依据：WetCastSouthEastElev 来自哪个Joint的E.Z
    if abs(calcRes.WetCastSouthEastElev - (H + theoreticalJointS.E.Z)) < 1e-12:
        wetCastJoint = theoreticalJointS
    else:
        wetCastJoint = matchJointN
    matchCastJoint = theoreticalJointS

    # ---- 业务标量 ----
    centerLength = calcRes.CenterLength
    southOffset = calcRes.SouthOffset
    northOffset = calcRes.NorthOffset
    wetCastSouthEastElev = calcRes.WetCastSouthEastElev
    wetCastSouthWestElev = calcRes.WetCastSouthWestElev
    wetCastNorthEastElev = calcRes.WetCastNorthEastElev
    wetCastNorthWestElev = calcRes.WetCastNorthWestElev

    # ---- rerr 计算【FIXED：rerr0‑3 使用calcRes摘要高程，不再用actualPointS几何坐标】----
    rerr = [0] * 24
    rerr[0] = E[6] - calcRes.SouthEastElev
    rerr[1] = E[7] - calcRes.SouthWestElev
    rerr[2] = E[8] - calcRes.NorthEastElev
    rerr[3] = E[9] - calcRes.NorthWestElev

    rerr[8] = (rerr[0] + rerr[1]) / 2.0
    rerr[10] = (rerr[2] + rerr[3]) / 2.0
    rerr[9] = (rerr[0] + rerr[3]) - (rerr[1] + rerr[2])
    print('Twist:{}'.format(rerr[9]))

    # FIXED：rerr[11] 修正引用（原死代码，一并修正）
    rerr[11] = (actualPointN.C.Z + rerr[10]) - (rerr[8] - rerr[10]) / (actualPointS.C.X - actualPointN.C.X) * (actualPointN.C.X - matchCastJoint.C.X)

    rerr[4] = rerr[0] - rerr[8]
    rerr[5] = rerr[1] - rerr[8]
    rerr[6] = rerr[2] + rerr[9] / 2.0 - rerr[10]
    rerr[7] = rerr[3] - rerr[9] / 2.0 - rerr[10]

    rerr[12] = E[1] - centerLength
    rerr[13] = E[3] - northOffset
    rerr[16] = math.sqrt((actualPointN.C.X - actualPointS.C.X) ** 2
                        + (actualPointN.C.Y - actualPointS.C.Y) ** 2)
    rerr[17] = math.sqrt((actualPointN.C.X - actualPointS.C.X) ** 2
                        + (actualPointN.C.Z - actualPointS.C.Z) ** 2)
    rerr[20] = E[2] - southOffset
    rerr[21] = math.sqrt((actualPointS.W.Y - actualPointS.E.Y) ** 2
                        + (actualPointS.W.Z - actualPointS.E.Z) ** 2)

    dxNNSS = K[4][3] - K[3][3]
    rerr[14] = math.sin(2 * math.asin(abs(rerr[13] - rerr[20]) / 2.0 / rerr[16])) * \
        (rerr[20] - rerr[13] + n) / abs(rerr[20] - rerr[13] + n) * \
        (d * dxNNSS + n) / abs(d * dxNNSS + n)

    rerr[18] = math.sin(2 * math.asin(abs(rerr[8] - rerr[10]) / 2.0 / rerr[17])) * \
        (rerr[8] - rerr[10] + n) / abs(rerr[8] - rerr[10] + n) * \
        (d * dxNNSS + n) / abs(d * dxNNSS + n)

    rerr[22] = math.sin(2 * math.asin(abs(rerr[6]) / rerr[21])) * \
        (rerr[6] + n) / abs(rerr[6] + n) * \
        (-actualPointN.E.Y + n) / abs(-actualPointN.E.Y + n)

    rerr[15] = math.sqrt(1.0 - rerr[14] ** 2)
    rerr[19] = math.sqrt(1.0 - rerr[18] ** 2)
    rerr[23] = math.sqrt(1.0 - rerr[22] ** 2)

    # ---- KCB（南侧浇筑）【FIXED：使用 wetCastJoint / matchCastJoint，适配 n=1/n=2】----
    KCB = [0] * 9
    KCB[0] = actualPointN.E.X - AN1 - AS2 + actualPointN.E.Y * rerr[14]
    KCB[1] = actualPointN.E.Y + rerr[13]
    KCB[2] = wetCastJoint.E.Z + E[10] - wetCastSouthEastElev

    KCB[3] = actualPointN.C.X - CN1 - CS2
    KCB[4] = matchCastJoint.C.Y + E[14]

    KCB[5] = wetCastJoint.C.Z
    KCB[6] = actualPointN.W.X - AN1 - AS2 + actualPointN.W.Y * rerr[14]
    KCB[7] = actualPointN.W.Y + rerr[13]
    KCB[8] = wetCastJoint.W.Z + E[11] - wetCastSouthWestElev

    # ---- KCF（北侧浇筑）----
    KCF = [0] * 9
    KCF[3] = theoreticalJointN.C.X + CN2 - rerr[12]
    KCF[4] = theoreticalJointN.C.Y + E[15]
    KCF[5] = theoreticalJointN.C.Z
    KCF[0] = KCF[3] - CN2 + AN2
    KCF[1] = KCF[4] + B
    KCF[2] = theoreticalJointN.E.Z + E[12] - wetCastNorthEastElev
    KCF[6] = KCF[3] - CN2 + AN2
    KCF[7] = KCF[4] - B
    KCF[8] = theoreticalJointN.W.Z + E[13] - wetCastNorthWestElev

    # ---- KF（固定模板）----
    KF = [0] * 9
    KF[0] = theoreticalJointN.E.X - rerr[12]
    KF[1] = theoreticalJointN.E.Y
    KF[2] = theoreticalJointN.E.Z + E[4] - H
    KF[3] = theoreticalJointN.C.X - rerr[12]
    KF[4] = theoreticalJointN.C.Y
    KF[5] = theoreticalJointN.C.Z
    KF[6] = theoreticalJointN.W.X - rerr[12]
    KF[7] = theoreticalJointN.W.Y
    KF[8] = theoreticalJointN.W.Z + E[5] - H

    KCBr = spaxis(actualPointS, rerr, KCB)
    KCFr = spaxis(actualPointS, rerr, KCF)
    KFr = spaxis(actualPointS, rerr, KF)

    KCBR = revaxischa(K, d, KCBr, cs)
    KCFR = revaxischa(K, d, KCFr, cs)
    KFR = revaxischa(K, d, KFr, cs)
    return KCBR, KCFR, KFR

# ============================================================
# 输入校验
# ============================================================
def judge(k, m, s):
    """输入校验。"""
    if s == 0:
        for i in range(len(k)):
            if abs(k[i]) < 1e-12:
                if m - 1 <= i:
                    print('判定通过！')
                    return True
                else:
                    print('判定不通过！当前记录允许计算第{}节段'.format(i + 1))
                    return False
        return True
    else:
        if m == 0:
            if len(k) == 14:
                print('实测数据输入正确')
                return True
            else:
                print('请检查实测数据输入格式！')
                return False
        else:
            if len(k) == 16:
                print('实测数据输入正确')
                return True
            else:
                print('请检查实测数据输入格式！')
                return False

# ============================================================
# 主程序
# ============================================================
def main(path1, m, A, B, C, H, first, st, E=[], bo=[], pathA=0, pathB=0):
    """主程序。"""
    errL = []
    errHP = []
    errH = []
    KK = [[0 for _ in range(9)] for _ in range(5)]
    if first:
        table = xlrd.open_workbook(pathA).sheets()[0]
        row = table.nrows
        col = table.ncols
        K = np.zeros((row, col))
        for x in range(col):
            cols = np.matrix(table.col_values(x))
            K[:, x] = cols
        d = (K[0][3] - K[1][3]) / abs(K[0][3] - K[1][3])
        row = len(K)
        col = len(K[0])
        n = 0
        tableZ = xlrd.open_workbook(pathB).sheets()[0]
        colZ = tableZ.ncols
        rowZ = tableZ.nrows
        YGDZ = np.zeros((rowZ, colZ))
        for i in range(colZ):
            cols = np.matrix(tableZ.col_values(i))
            YGDZ[:, i] = cols
        for i in range(colZ):
            K[i][2] = K[i][2] + YGDZ[i][0]
            K[i][5] = K[i][5] + YGDZ[i][1]
            K[i][8] = K[i][8] + YGDZ[i][2]
        KT = [[0 for _ in range(col)] for _ in range(row)]
        KCT = [[0 for _ in range(col)] for _ in range(2 * (row - 1))]
        workbook = xlwt.Workbook(encoding='utf-8')
        sheet1 = workbook.add_sheet('K')
        sheet2 = workbook.add_sheet('KT')
        sheet3 = workbook.add_sheet('KCT')
        for i in range(len(K)):
            for j in range(len(K[i])):
                sheet1.write(i, j, K[i][j])
        for i in range(len(KT)):
            for j in range(len(KT[i])):
                sheet2.write(i, j, KT[i][j])
        for i in range(len(KCT)):
            for j in range(len(KCT[i])):
                sheet3.write(i, j, KCT[i][j])
        workbook.save(path1)
        J = True
    else:
        table0 = xlrd.open_workbook(path1).sheets()[0]
        table1 = xlrd.open_workbook(path1).sheets()[1]
        table2 = xlrd.open_workbook(path1).sheets()[2]
        row0 = table0.nrows
        col0 = table0.ncols
        row1 = table1.nrows
        col1 = table1.ncols
        row2 = table2.nrows
        col2 = table2.ncols
        K = np.zeros((row0, col0))
        KT = np.zeros((row1, col1))
        KCT = np.zeros((row2, col2))
        for x in range(col0):
            cols0 = np.matrix(table0.col_values(x))
            K[:, x] = cols0
        d = (K[0][3] - K[1][3]) / abs(K[0][3] - K[1][3])
        row = len(K)
        col = len(K[0])
        n = 0
        for x in range(col1):
            cols1 = np.matrix(table1.col_values(x))
            KT[:, x] = cols1
        for x in range(col2):
            cols2 = np.matrix(table2.col_values(x))
            KCT[:, x] = cols2
        J = judge(KT, m, 0)
    if J:
        for i in range(9):
            KK[0][i] = K[m][i]
            KK[1][i] = K[m + 1][i]
        if m != 0:
            for i in range(9):
                KK[3][i] = KCT[2 * (m - 1)][i]
                KK[4][i] = KCT[2 * m - 1][i]
        if m < 2:
            n = m
            for i in range(9):
                KK[2][i] = K[m][i]
            calcRes = calc(KK, d, n, A, B, C, H)
        else:
            n = 2
            for i in range(9):
                KK[2][i] = KT[m][i]
            calcRes = calc(KK, d, n, A, B, C, H)
        # 输出预制匹配参数
        if m == 0:
            print('No.{} Floating Bulkhead Setup:\n'.format(m)
                  + 'East Elev:{}\n'.format(calcRes.SouthEastElev)
                  + 'West Elev:{}\n'.format(calcRes.SouthWestElev)
                  + 'Center Offset:{}\n'.format(calcRes.TheoreticalJointS.C.Y)
                  + 'East Length:{}\n'.format(calcRes.TheoreticalJointS.E.X)
                  + 'West Length:{}\n'.format(calcRes.TheoreticalJointS.W.X))
        else:
            print('No.{} Match‑Cast Set‑up:\n'.format(m)
                  + 'Length:{}\n'.format(calcRes.CenterLength)
                  + 'Center Offset:\n'
                  + 'South:{}\n'.format(calcRes.SouthOffset)
                  + 'North:{}\n'.format(calcRes.NorthOffset)
                  + 'South East Elev:{}\n'.format(calcRes.SouthEastElev)
                  + 'South West Elev:{}\n'.format(calcRes.SouthWestElev)
                  + 'North East Elev:{}\n'.format(calcRes.NorthEastElev)
                  + 'North West Elev:{}\n'.format(calcRes.NorthWestElev))
        if st == 1:
            print('进行坐标修正')
            J = judge(E, n, 1)
            if J:
                if n == 0:
                    print('perfect cast:\n'
                          + 'Floating Bulkhead:\n'
                          + 'East Elev:{}\n'.format(calcRes.SouthEastElev)
                          + 'West Elev:{}\n'.format(calcRes.SouthWestElev)
                          + 'Center:{}\n'.format(calcRes.TheoreticalJointS.C.Y)
                          + 'Fixed Bulkhead:\n'
                          + 'East Elev:{}\n'.format(calcRes.NorthEastElev)
                          + 'West Elev:{}\n'.format(calcRes.NorthWestElev)
                          + 'Center Offset:{}\n'.format(calcRes.TheoreticalJointN.C.Y)
                          + 'Wet‑Cast:\n'
                          + 'North East Elev:{}\n'.format(calcRes.NorthEastElev)
                          + 'North West Elev:{}\n'.format(calcRes.NorthWestElev)
                          + 'South East Elev:{}\n'.format(calcRes.SouthEastElev)
                          + 'South West Elev:{}\n'.format(calcRes.SouthWestElev)
                          + 'North Offset:{}\n'.format(calcRes.TheoreticalJointN.C.Y)
                          + 'South Offset:{}\n'.format(calcRes.TheoreticalJointS.C.Y)
                          + 'East Length:{}\n'.format(calcRes.TheoreticalJointS.E.X)
                          + 'West Length:{}\n'.format(calcRes.TheoreticalJointS.W.X))
                    KCBR, KCFR, KBR, KFR = zeroerr(E, bo, A, C, calcRes, KK, d, calcRes.cs)
                    print('No.0 AS‑CAST COORDINATES:\n'
                          + 'Bolts and Hairpins(East Bolt, Hairpin, West Bolt):\n'
                          + 'South:{}\n'.format(KCBR)
                          + 'North:{}\n'.format(KCFR)
                          + 'Joints(East, Center, West)\n'
                          + 'South Joint:{}\n'.format(KBR)
                          + 'North Joint:{}\n'.format(KFR)
                          + 'No.0 Theoretical COORDINATES:\n'
                          + 'Bulkhead Joint:(East, Center, West)\n'
                          + 'South Joint:{}\n'.format(K[0, :])
                          + 'North Joint:{}\n'.format(K[1, :]))
                else:
                    print('perfect cast:\n'
                          + 'Match‑Cast:\n'
                          + 'North East Elev:{}\n'.format(calcRes.NorthEastElev)
                          + 'North West Elev:{}\n'.format(calcRes.NorthWestElev)
                          + 'South East Elev:{}\n'.format(calcRes.SouthEastElev)
                          + 'South West Elev:{}\n'.format(calcRes.SouthWestElev)
                          + 'North Offset:{}\n'.format(calcRes.NorthOffset)
                          + 'South Offset:{}\n'.format(calcRes.SouthOffset)
                          + 'Wet‑Cast:\n'
                          + 'North East Elev:{}\n'.format(calcRes.WetCastNorthEastElev)
                          + 'North West Elev:{}\n'.format(calcRes.WetCastNorthWestElev)
                          + 'South East Elev:{}\n'.format(calcRes.WetCastSouthEastElev)
                          + 'South West Elev:{}\n'.format(calcRes.WetCastSouthWestElev)
                          + 'North Offset:{}\n'.format(0)
                          + 'South Offset:{}\n'.format(0)
                          + 'Length:{}\n'.format(calcRes.CenterLength)
                          + 'Bulkhead:\n'
                          + 'East Elev:{}\n'.format(calcRes.BulkheadEastElev)
                          + 'West Elev:{}\n'.format(calcRes.BulkheadWestElev)
                          + 'Center Offset:{}\n'.format(calcRes.BulkheadCenterOffset))
                    KCBR, KCFR, KFR = oneerr(E, bo, A, C, B, H, calcRes, KK, d, calcRes.cs)
                    print('No.{} AS‑CAST COORDINATES:\n'.format(m)
                          + 'Bolts and Hairpins(East Bolt, Hairpin, West Bolt):\n'
                          + 'South:{}\n'.format(KCBR)
                          + 'North:{}\n'.format(KCFR)
                          + 'Joints(East, Center, West)\n'
                          + 'North Joint:{}\n'.format(KFR)
                          + 'No.{} Theoretical COORDINATES:\n'.format(m)
                          + 'Bulkhead Joint:(East, Center, West)\n'
                          + 'South Joint:{}\n'.format(K[m, :])
                          + 'North Joint:{}\n'.format(K[m + 1, :]))
                for i in range(9):
                    if m != 0:
                        KT[m + 1][i] = KFR[i]
                    else:
                        KT[m][i] = KBR[i]
                        KT[m + 1][i] = KFR[i]
                    KCT[2 * m][i] = KCBR[i]
                    KCT[2 * m + 1][i] = KCFR[i]
                LT = math.sqrt((K[m][3] - K[m + 1][3]) ** 2 + (K[m][4] - K[m + 1][4]) ** 2 + (K[m][5] - K[m + 1][5]) ** 2)
                LA = math.sqrt((KT[m][3] - KT[m + 1][3]) ** 2 + (KT[m][4] - KT[m + 1][4]) ** 2 + (KT[m][5] - KT[m + 1][5]) ** 2)
                LTT = math.sqrt((K[0][3] - K[m + 1][3]) ** 2 + (K[0][4] - K[m + 1][4]) ** 2 + (K[0][5] - K[m + 1][5]) ** 2)
                LAT = math.sqrt((KT[0][3] - KT[m + 1][3]) ** 2 + (KT[0][4] - KT[m + 1][4]) ** 2 + (KT[0][5] - KT[m + 1][5]) ** 2)
                print('Length on Center:\n'
                      + 'Theoretical:{}'.format(LT) + 'Total:{}\n'.format(LTT)
                      + 'As‑Cast:{}'.format(LA) + 'Total:{}\n'.format(LAT))
                workbook = xlwt.Workbook(encoding='utf‑8')
                sheet1 = workbook.add_sheet('K')
                sheet2 = workbook.add_sheet('KT')
                sheet3 = workbook.add_sheet('KCT')
                for i in range(len(K)):
                    for j in range(len(K[i])):
                        sheet1.write(i, j, K[i][j])
                for i in range(len(KT)):
                    for j in range(len(KT[i])):
                        sheet2.write(i, j, KT[i][j])
                for i in range(len(KCT)):
                    for j in range(len(KCT[i])):
                        sheet3.write(i, j, KCT[i][j])
                workbook.save(path1)
                print('现场拼装坐标已更新')
            else:
                print('退出运行')
        else:
            print('不进行坐标修正')
    else:
        print('退出运行')
    table1 = xlrd.open_workbook(path1).sheets()[1]
    row1 = table1.nrows
    col1 = table1.ncols
    KT = np.zeros((row1, col1))
    for x in range(col1):
        cols1 = np.matrix(table1.col_values(x))
        KT[:, x] = cols1
    mm = 0
    for i in range(row1):
        if abs(KT[i, 0]) > 1e-12:
            mm = mm + 1
        else:
            break
    for i in range(mm):
        errL.append(KT[i, 3] - K[i, 3])
        errHP.append(KT[i, 4] - K[i, 4])
        errH.append(KT[i, 5] - K[i, 5])
    print('节段长度偏差曲线：', errL)
    print('节段偏距偏差曲线：', errHP)
    print('节段高程偏差曲线：', errH)
    return

# ============================================================
# 输入参数
# ============================================================
if __name__ == '__main__':
    path1 = r'D:\projects\三航局\宝山\短线预制线型控制算法\Kr.xlsx'
    pathA = r'D:\projects\三航局\宝山\短线预制线型控制算法\K.xlsx'
    pathB = r'D:\projects\三航局\宝山\短线预制线型控制算法\ZZYGD.xlsx'
    A = 0.1
    B = 2.9
    C = 0.1
    H = 1
    m = 1
    first = False
    st = 1
    bo = [0.1, 0.1, 0.1, 0.1, 0.1, 0.1]
    E = [0, 2.9950, 0, -0.0043, 0.9996, 1.0004, 0.9992, 1.0025,
         0.9986, 1.005, 0.9944, 1.0059, 0.9985, 1.005, 0, 0]
    main(path1, m, A, B, C, H, first, st, E, bo, pathA, pathB)
