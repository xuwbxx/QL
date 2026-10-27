import numpy as np
import pandas as pd
import xlrd
import random
import copy

def matrix(path, n):
    table = xlrd.open_workbook(path).sheets()[n]    # 获取第一个sheet表
    row = table.nrows                               # 行数
    col = table.ncols                               # 列数
    datamatrix = np.zeros((row, col))               # 生成一个nrows行ncols列，且元素均为0的初始矩阵
    for x in range(col):
        cols = np.matrix(table.col_values(x))   # 把list转换为矩阵进行矩阵操作
        datamatrix[:, x] = cols                 # 按列把数据存进矩阵中
      
    return datamatrix

# P: 
# R: 做权重，1最关心，0最不关心（参考point表里的R1sheet）
def gescha(P,Q,D,R):
    nrow, ncol=P.shape
    nrow1, ncol1=D.shape
    wP=P*R
    wQ=Q*R
    # 找重心，去中心化
    G1=np.sum(wP,axis=1)/np.sum(R,axis=1)
    G2=np.sum(wQ,axis=1)/np.sum(R,axis=1)
    PP=np.zeros((nrow, ncol))
    QQ=np.zeros((nrow, ncol))
    for i in range(3):
        for j in range(ncol):
            PP[i,j]=P[i,j]-G1[i]
            QQ[i,j]=Q[i,j]-G2[i]
    # SVD公式分解
    PT=np.transpose(PP)
    QT=np.transpose(QQ)
    H=(R*PP)@QT
    U, S, VT = np.linalg.svd(H)
    V=np.transpose(VT)
    UT=np.transpose(U)
    # 此处R开始，是指旋转矩阵，不再是前面的权重
    R=np.dot(V,UT)
    det_R=np.linalg.det(R)
    cha=np.diag([1,1,-1])
    T=np.zeros((3,ncol1))
    # 判定是否反射，否的话，需要修正（非反射的情况下，会走360°的大角度旋转，此处应选择小角度旋转）
    if det_R>0:
        t=G2-np.dot(R,G1)
    else:
        RR=np.dot(V,cha)
        R=np.dot(RR,UT)
        t=G2-np.dot(R,G1)
    
    for i in range(3):
        for j in range(ncol1):
            # 得到T 平移矩阵
            T[i,j]=t[i]
    # D 旋转，再平移，得到计算的未来的预测坐标
    D1=np.dot(R,D)+T

    return D1

def ZERO(R, i):
    for j in range(3):
        R[j,i-1]=0
    
    return R

path='D:\projects\三航局\福州机场\试验3：随机失活\POINT.xlsx'
# P1: 理论值（当前已完工的节段）
P1=matrix(path, 0)
# P2: 实测值（当前已完工的节段）
P2=matrix(path, 1)
# P3: 理论值（剩余未完工节段）
P3=matrix(path, 2)
# P4: 周老师测试用，当前不用参考
P4=matrix(path, 3)
# R: 节段权重（0最不关心，1最关心）
R1=matrix(path, 4)
R2=matrix(path, 5)
# 计算结果
P5=gescha(P1,P2,P3,R1)
# 周老师测试用，当前不用参考
P6=gescha(P5,P4,P5,R2)

# numbers=[18,16,33,40,4,7,1,9,14]
# R22=copy.copy(R2)
# for k in range(len(numbers)):
#     R22=ZERO(R22, numbers[k])
# P7=gescha(P5,P4,P5,R22)
# E=(P7-P6)*R22
# print(E[2,:])
# max_index = np.abs(E[2,:]).argmax()
# print(max_index)
# max_element = E.flat[max_index]
# err=max_element
# print(max_element)
# data = pd.DataFrame(E)
# writer = pd.ExcelWriter('D:\projects\三航局\福州机场\试验3：随机失活\Test\Result1.xlsx')		# 写入Excel文件
# data.to_excel(writer, 'E', float_format='%.5f')		# ‘page_1’是写入excel的sheet名
# writer.save()

n=9
m=500
r=2
err=np.zeros((m, n+1))
for i in range(m):
    numbers = random.sample(range(1, 55), n)
    for j in range(len(numbers)):
        err[i,j]=numbers[j]
    R22=copy.copy(R2)
    for k in range(len(numbers)):
        R22=ZERO(R22, numbers[k])
    P7=gescha(P5,P4,P5,R22)
    E=(P7-P6)*R22
    row=E[r,:]
    max_index = np.abs(row).argmax()
    max_element = row.flat[max_index]
    err[i,n]=max_element

data = pd.DataFrame(err)
writer = pd.ExcelWriter('D:\projects\三航局\福州机场\试验3：随机失活\Test\Result{}-{}-2.xlsx'.format(m,n))		# 写入Excel文件
data.to_excel(writer, 'err', float_format='%.5f')		# ‘page_1’是写入excel的sheet名
writer.save()