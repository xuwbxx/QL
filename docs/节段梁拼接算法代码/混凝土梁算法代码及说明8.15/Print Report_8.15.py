import math
import numpy as np

def PR(K):#输入节点表格，输出计算参数
    row=len(K)
    col=len(K[0])
    kk=[['Joint','B_East','B_West','Bulkhead Angle(East)','Bulkhead Angle(West)','Long Offset(East)','Long Offset(West)','Cross-Slope(East)','Cross-Slope(West)','Z-Axis']]

    for i in range(row):
        if i==0:
            BE=math.sqrt((K[i][0]-K[i][3])**2+(K[i][1]-K[i][4])**2+(K[i][2]-K[i][5])**2)
            BW=math.sqrt((K[i][6]-K[i][3])**2+(K[i][7]-K[i][4])**2+(K[i][8]-K[i][5])**2)
            BEL=math.sqrt((K[i][0]-K[i][3])**2+(K[i][1]-K[i][4])**2)##新增
            BWL=math.sqrt((K[i][6]-K[i][3])**2+(K[i][7]-K[i][4])**2)##新增
            AE='NAN'
            AW='NAN'
            LOE='NAN'
            LOW='NAN'
            CSE=(K[i][2]-K[i][5])/BEL##修改
            CSW=(K[i][5]-K[i][8])/BWL##修改
            ZA='NAN'
            a=[i,BE,BW,AE,AW,LOE,LOW,CSE,CSW,ZA]
            kk.append(a)
        else:
            BE=math.sqrt((K[i][0]-K[i][3])**2+(K[i][1]-K[i][4])**2+(K[i][2]-K[i][5])**2)
            BW=math.sqrt((K[i][6]-K[i][3])**2+(K[i][7]-K[i][4])**2+(K[i][8]-K[i][5])**2)

            L1=[K[i-1][3]-K[i-1][0],K[i-1][4]-K[i-1][1],K[i-1][5]-K[i-1][2]]
            L2=[K[i][0]-K[i-1][0],K[i][1]-K[i-1][1],K[i][2]-K[i-1][2]]
            L3=[K[i-1][3]-K[i-1][6],K[i-1][4]-K[i-1][7],K[i-1][5]-K[i-1][8]]
            L4=[K[i][6]-K[i-1][6],K[i][7]-K[i-1][7],K[i][8]-K[i-1][8]]
            AE=CA(L1,L2)
            AW=CA(L3,L4)

            LC=[K[i-1][3]-K[i][3],K[i-1][4]-K[i][4],K[i-1][5]-K[i][5]]
            LE=[K[i][0]-K[i][3],K[i][1]-K[i][4],K[i][2]-K[i][5]]
            LW=[K[i][6]-K[i][3],K[i][7]-K[i][4],K[i][8]-K[i][5]]
            LOE=np.dot(LC,LE)/np.linalg.norm(LC)
            LOW=np.dot(LC,LW)/np.linalg.norm(LC)
            
            CSE=(K[i][2]-K[i][5])/BE
            CSW=(K[i][5]-K[i][8])/BW
            
            if K[i-1][5]<K[i][5]:
                ZA='Upward'
            else:
                ZA='Check'
            a=[i,BE,BW,AE,AW,LOE,LOW,CSE,CSW,ZA]
            kk.append(a)

    return kk

def CA(L1,L2):
    dot_product = np.dot(L1, L2)
    norm_vector1 = np.linalg.norm(L1)
    norm_vector2 = np.linalg.norm(L2)
    cos_theta = dot_product / (norm_vector1 * norm_vector2)
    angle_rad = np.arccos(cos_theta)
    A = np.degrees(angle_rad)
    return A

K=[[531242.1046,314826.3704,55.3135,531241.2840,314834.3207,55.6583,531240.4633,314842.2710,56.0030],
[531251.0571,314827.2945,55.4072,531250.2364,314835.2449,55.7518,531249.4157,314843.1952,56.0964],
[531260.3410,314828.2529,55.5045,531259.5203,314836.2032,55.8489,531258.6997,314844.1535,56.1936]]
kk=PR(K)
print(kk)