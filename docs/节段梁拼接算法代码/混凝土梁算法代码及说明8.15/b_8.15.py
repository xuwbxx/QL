import numpy as np
import math
import copy
import xlrd
import xlwt


def calc(K,d,n,A,B,C,H):#节段计算
    
    if n==0:
        res,cs=zero(K,d,A,B,C,H)
    if n==1:
        res,cs=one(K,d,A,B,C,H)
    if n==2:
        res,cs=standard(K,d,A,B,C,H)
    
    return res,cs

def zero(K,d,A,B,C,H):#0号节段计算
    k1=[]#截面B
    k2=[]#截面F
    res=[0,0,0,H,H,H,H]
    for i in range(2):
        for j in range(9):
            if i==0:
                k1.append(K[i][j])
            else:
                k2.append(K[i][j])
    
    res1,cs=axischa(K,k1,d,A,B,C)
    res2, *_=axischa(K,k2,d,A,B,C)
    
    res[0]=res1[3]
    res[1]=res1[4]
    res[2]=res2[4]
    res[3]=res[3]+res1[2]
    res[4]=res[4]+res1[8]
    res[5]=res[5]+res2[2]
    res[6]=res[6]+res2[8]

    R=[res, res1, res2]
    return R,cs

def one(K,d,A,B,C,H):#1号节段计算
    k1=[]
    k2=[]
    k3=[]
    k4=[]
    k5=[]
    res=[0]*11
    for j in range(9):
        k1.append(K[0][j])
        k2.append(K[1][j])
        k3.append(K[3][j])
        k4.append(K[4][j])
        k5.append(K[2][j])
    
    res1,cs=axischa(K,k1,d,A,B,C)
    res2, *_=axischa(K,k2,d,A,B,C)
    res3, *_=axischa(K,k3,d,A,B,C)
    res4, *_=axischa(K,k4,d,A,B,C)
    res5, *_=axischa(K,k5,d,A,B,C)
    
    res[0]=res1[3]
    res[1]=res3[4]
    res[2]=res4[4]
    res[3]=H+res3[2]
    res[4]=H+res3[8]
    res[5]=H+res4[2]
    res[6]=H+res4[8]
    res[7]=H+res1[2]
    res[8]=H+res1[8]
    res[9]=H+res2[2]
    res[10]=H+res2[8]

    R=[res, res1,res2,res3,res4,res5]

    return R,cs

def standard(K,d,A,B,C,H):#标准节段计算
    k1=[]
    k2=[]
    k3=[]
    k4=[]
    k5=[]
    res=[0]*11
    for j in range(9):
        k1.append(K[2][j])
        k2.append(K[1][j])
        k3.append(K[3][j])
        k4.append(K[4][j])
        k5.append(K[0][j])
    
    res1,cs=axischa(K,k1,d,A,B,C)
    res2, *_=axischa(K,k2,d,A,B,C)
    res3, *_=axischa(K,k3,d,A,B,C)
    res4, *_=axischa(K,k4,d,A,B,C)
    res5, *_=axischa(K,k5,d,A,B,C)
    
    res[0]=res1[3]
    res[1]=res3[4]
    res[2]=res4[4]
    res[3]=H+res3[2]
    res[4]=H+res3[8]
    res[5]=H+res4[2]
    res[6]=H+res4[8]
    res[7]=H+res1[2]
    res[8]=H+res1[8]
    res[9]=H+res2[2]
    res[10]=H+res2[8]

    R=[res, res1,res2,res3,res4,res5]

    return R,cs

def axischa(K,k,d,A,B,C):#坐标变换
    n=10**(-200)
    K1=[]
    K2=[]
    K3=[]
    K4=[]##新增

    for i in range(3):
        for j in range(3):
            if (3*i+j+1)%3 == 0:
                K1.append((K[0][3*i+j]-K[1][3+j]))
                K2.append((k[3*i+j]-K[1][3+j]))
                K3.append((K[2][3*i+j]-K[1][3+j]))
                K4.append((K[1][3*i+j]-K[1][3+j]))##新增
            else:
                K1.append(d*(K[0][3*i+j]-K[1][3+j]))
                K2.append(d*(k[3*i+j]-K[1][3+j]))
                K3.append(d*(K[2][3*i+j]-K[1][3+j]))
                K4.append(d*(K[1][3*i+j]-K[1][3+j]))##新增
    
    LXY=math.sqrt(K1[3]**2+K1[4]**2)
    Sin1=abs(K1[4]/LXY)*(K1[3]+n)/abs(K1[3]+n)*(-K1[4]+n)/abs(K1[4]+n)
    Cos1=abs(K1[3]/LXY)
    K11=copy.copy(K1)
    K21=copy.copy(K2)
    K31=copy.copy(K3)
    K41=copy.copy(K4)##新增
    for i in range(3):
        K11[3*i]=Cos1*K1[3*i]-Sin1*K1[3*i+1]
        K11[3*i+1]=Sin1*K1[3*i]+Cos1*K1[3*i+1]
        K21[3*i]=Cos1*K2[3*i]-Sin1*K2[3*i+1]
        K21[3*i+1]=Sin1*K2[3*i]+Cos1*K2[3*i+1]
        K31[3*i]=Cos1*K3[3*i]-Sin1*K3[3*i+1]
        K31[3*i+1]=Sin1*K3[3*i]+Cos1*K3[3*i+1]
        K41[3*i]=Cos1*K4[3*i]-Sin1*K4[3*i+1]##新增
        K41[3*i+1]=Sin1*K4[3*i]+Cos1*K4[3*i+1]##新增
    
    LXZ=math.sqrt(K31[3]**2+K31[5]**2)
    Sin2=abs(K31[5]/LXZ)*(K31[3]+n)/abs(K31[3]+n)*(-K31[5]+n)/abs(K31[5]+n)
    Cos2=abs(K31[3]/LXZ)
    K12=copy.copy(K11)
    K22=copy.copy(K21)
    K32=copy.copy(K31)
    K42=copy.copy(K41)##新增
    for i in range(3):
        K12[3*i]=Cos2*K11[3*i]-Sin2*K11[3*i+2]
        K12[3*i+2]=Sin2*K11[3*i]+Cos2*K11[3*i+2]
        K22[3*i]=Cos2*K21[3*i]-Sin2*K21[3*i+2]
        K22[3*i+2]=Sin2*K21[3*i]+Cos2*K21[3*i+2]
        K32[3*i]=Cos2*K31[3*i]-Sin2*K31[3*i+2]
        K32[3*i+2]=Sin2*K31[3*i]+Cos2*K31[3*i+2]
        K42[3*i]=Cos2*K41[3*i]-Sin2*K41[3*i+2]##新增
        K42[3*i+2]=Sin2*K41[3*i]+Cos2*K41[3*i+2]##新增

    ##新增
    LYZ=math.sqrt(K42[7]**2+K42[8]**2)
    Sin3=abs(K42[8]/LYZ)*(K42[8]+n)/abs(K42[8]+n)*(-K42[7]+n)/abs(K42[7]+n)
    Cos3=abs(K42[7]/LYZ)
    K13=copy.copy(K12)
    K23=copy.copy(K22)
    K33=copy.copy(K32)
    K43=copy.copy(K42)
    for i in range(3):
        K13[3*i+1]=Cos3*K12[3*i+1]-Sin3*K12[3*i+2]
        K13[3*i+2]=Sin3*K12[3*i+1]+Cos3*K12[3*i+2]
        K23[3*i+1]=Cos3*K22[3*i+1]-Sin3*K22[3*i+2]
        K23[3*i+2]=Sin3*K22[3*i+1]+Cos3*K22[3*i+2]
        K33[3*i+1]=Cos3*K32[3*i+1]-Sin3*K32[3*i+2]
        K33[3*i+2]=Sin3*K32[3*i+1]+Cos3*K32[3*i+2]
        K43[3*i+1]=Cos3*K42[3*i+1]-Sin3*K42[3*i+2]
        K43[3*i+2]=Sin3*K42[3*i+1]+Cos3*K42[3*i+2]
    
    cs=[Sin1, Cos1, Sin2, Cos2, Sin3, Cos3]##修改

    return K23, cs##修改
    
def main(path1,m,A,B,C,H,first,st,E=[],bo=[],pathA=0,pathB=0):#主程序 bo：测点轴向偏位[aN,aS,cN,cS]
    errL=[]
    errHP=[]
    errH=[]

    KK=[[0 for _ in range(9)] for _ in range(5)]
    if first:#新建工程，新建一个内部数据表
        #读取桥梁成桥线形
        table = xlrd.open_workbook(pathA).sheets()[0]
        row = table.nrows
        col = table.ncols
        K = np.zeros((row, col))
        for x in range(col):
            cols = np.matrix(table.col_values(x))
            K[:, x] = cols
        d=(K[0][3]-K[1][3])/abs(K[0][3]-K[1][3]) #判断坐标轴方向
        row=len(K)
        col=len(K[0])
        n=0

        #读取制造和施工预拱度
        tableZ = xlrd.open_workbook(pathB).sheets()[0]
        colZ = tableZ.ncols
        rowZ = tableZ.nrows
        YGDZ = np.zeros((rowZ,colZ))
        for i in range(colZ):
            cols = np.matrix(tableZ.col_values(i))
            YGDZ[:, i] = cols
        
        # tableS = xlrd.open_workbook(pathC).sheets()[0]
        # colS = tableS.ncols
        # YGDS = np.zeros(colS)
        # rows = np.matrix(tableS.row_values(0))
        # for i in range(colS):
        #     YGDS[i]=rows[0,i]
        
        #K加制造预拱度
        for i in range(colZ):
            K[i][2]=K[i][2]+YGDZ[i][0]#预拱度E
            K[i][5]=K[i][5]+YGDZ[i][1]#预拱度C
            K[i][8]=K[i][8]+YGDZ[i][2]#预拱度W

        #计算施工预拱度与制造预拱度差值YGDC
        # YGDC=YGDS-YGDZ

        KT=[[0 for _ in range(col)] for _ in range(row)]#存储实际joint坐标
        KCT=[[0 for _ in range(col)] for _ in range(2*(row-1))]#存储实际测点坐标
        # KTSG=copy.copy(KT)
        # KCTSG=copy.copy(KCT)
        workbook = xlwt.Workbook(encoding='utf-8')
        sheet1=workbook.add_sheet('K')
        sheet2=workbook.add_sheet('KT')#实际截面坐标
        sheet3=workbook.add_sheet('KCT')#实际测点坐标
        # sheet4=workbook.add_sheet('YGD')
        # sheet5=workbook.add_sheet('KTSG')#加施工预拱度后实际截面坐标
        # sheet6=workbook.add_sheet('KCTSG')#加施工预拱度后实际测点坐标
        for i in range(len(K)):
            for j in range(len(K[i])):
                sheet1.write(i,j,K[i][j])
        for i in range(len(KT)):
            for j in range(len(KT[i])):
                sheet2.write(i,j,KT[i][j])
                # sheet5.write(i,j,KT[i][j])
        for i in range(len(KCT)):
            for j in range(len(KCT[i])):
                sheet3.write(i,j,KCT[i][j])
                # sheet6.write(i,j,KCT[i][j])
        # for i in range(len(YGDC)):
        #     sheet4.write(0,i,YGDC[i])

        savePath = path1
        workbook.save(savePath)
        J=True
    else:#已有工程，读取对应数据表
        table0 = xlrd.open_workbook(path1).sheets()[0]
        table1 = xlrd.open_workbook(path1).sheets()[1]
        table2 = xlrd.open_workbook(path1).sheets()[2]
        #table3 = xlrd.open_workbook(path1).sheets()[3]
        # table4 = xlrd.open_workbook(path1).sheets()[4]
        # table5 = xlrd.open_workbook(path1).sheets()[5]
        row0 = table0.nrows
        col0 = table0.ncols
        row1 = table1.nrows
        col1 = table1.ncols
        row2 = table2.nrows
        col2 = table2.ncols
        #col3 = table3.ncols
        K = np.zeros((row0, col0))
        KT = np.zeros((row1, col1))
        KCT = np.zeros((row2, col2))
        # KTSG = np.zeros((row1, col1))
        # KCTSG = np.zeros((row2, col2))
        #YGDC = np.zeros(col3)

        for x in range(col0):
            cols0 = np.matrix(table0.col_values(x))
            K[:, x] = cols0#读取理论坐标并写入K
        d=(K[0][3]-K[1][3])/abs(K[0][3]-K[1][3]) #判断坐标轴方向
        row=len(K)
        col=len(K[0])
        n=0
        
        #读取施工预拱度与制造预拱度差值YGDC
        # rows3=np.matrix(table1.row_values(0))
        # for x in range(col3):
        #     YGDC[x] = rows3[0,x]

        for x in range(col1):
            cols1 = np.matrix(table1.col_values(x))
            # cols4 = np.matrix(table4.col_values(x))
            KT[:, x] = cols1#读取并写入实际joint坐标
            # KTSG[:, x] = cols4
        for x in range(col2):
            cols2 = np.matrix(table2.col_values(x))
            # cols5 = np.matrix(table5.col_values(x))
            KCT[:, x] = cols2#读取并写入实际测点坐标
            # KCTSG[:, x] = cols5
        J=judge(KT,m,0)
    if J:#判定能否计算第m节段
        for i in range(9):
            KK[0][i]=K[m][i]#预制节段的理论Joint(S)
            KK[1][i]=K[m+1][i]#预制节段的理论Joint(N)
            if m!=0:
                KK[3][i]=KCT[2*(m-1)][i]#匹配节段实际测点(S)
                KK[4][i]=KCT[2*m-1][i]#匹配节段实际测点(N)
        
        if m<2:
            n=m
            for i in range(9):
                KK[2][i]=K[m][i]#实测匹配段joint(N)/0号段为0号段joint（S)
            res,cs=calc(KK,d,n,A,B,C,H)

        else:
            n=2
            for i in range(9):
                KK[2][i]=KT[m][i]#实测匹配段joint(N)
            res,cs=calc(KK,d,n,A,B,C,H)
        
        #输出预制匹配参数
        if m==0:
             print('No.{} Floating Bulkhead Setup:\n'.format(m)
                  +'East Elev:{}\n'.format(res[0][3])
                  +'West Elev:{}\n'.format(res[0][4])
                  +'Center Offset:{}\n'.format(res[1][4])
                  +'East Length:{}\n'.format(res[1][0])
                  +'West Length:{}\n'.format(res[1][6]))
        else:
             print('No.{} Match-Cast Set-up:\n'.format(m)
                  +'Length:{}\n'.format(res[0][0])
                  +'Center Offset:\n'
                  +'South:{}\n'.format(res[0][1])
                  +'North:{}\n'.format(res[0][2])
                  +'South East Elev:{}\n'.format(res[0][3])
                  +'South West Elev:{}\n'.format(res[0][4])
                  +'North East Elev:{}\n'.format(res[0][5])
                  +'North West Elev:{}\n'.format(res[0][6]))

        if st==1:#是否输入实测数据进行修正
            print('进行坐标修正')
            J=judge(E,n,1)#判定实测数据输入是否合规
            if J:#输出修正后的坐标
                if n==0:
                    print('perfect cast:\n'
                    +'Floating Bulkhead:\n'
                    +'East Elev:{}\n'.format(res[0][3])
                    +'West Elev:{}\n'.format(res[0][4])
                    +'Center:{}\n'.format(res[1][4])
                    +'Fixed Bulkhead:\n'
                    +'East Elev:{}\n'.format(res[0][5])
                    +'West Elev:{}\n'.format(res[0][6])
                    +'Center Offset:{}\n'.format(res[2][4])
                    +'Wet-Cast:\n'
                    +'North East Elev:{}\n'.format(res[0][5])
                    +'North West Elev:{}\n'.format(res[0][6])
                    +'South East Elev:{}\n'.format(res[0][3])
                    +'South West Elev:{}\n'.format(res[0][4])
                    +'North Offset:{}\n'.format(res[2][4])
                    +'South Offset:{}\n'.format(res[1][4])
                    +'East Length:{}\n'.format(res[1][0])
                    +'West Length:{}\n'.format(res[1][6]))

                    KCBR, KCFR, KBR, KFR=zeroerr(E,bo,A,C,res,KK,d,cs)
                    
                    #加预拱度差值，新建参数的名称
                    # l1=KK[1][3]
                    # l2=KCBR[3]
                    # l3=KCFR[3]
                    # L1=abs(l1-l2)
                    # L2=abs(l1-l3)
                    # L=(E[1]+E[3])/2
                    # ygdc=YGDC[0]
                    # yB=ygdc/L*(L-L1)
                    # yF=ygdc/L*(L-L2)
                    # KCBRSG=copy.copy(KCBR)
                    # KCFRSG=copy.copy(KCFR)
                    # for i in range(3):
                    #     KCBRSG[3*i+2]=KCBRSG[3*i+2]+yB
                    #     KCFRSG[3*i+2]=KCFRSG[3*i+2]+yF

                    print('No.0 AS-CAST COORDINATES:\n'
                        +'Bolts and Hairpins(East Bolt, Hairpin, West Bolt):\n'
                        +'South:{}\n'.format(KCBR)
                        +'North:{}\n'.format(KCFR)
                        +'Joints(East, Center, West)\n'
                        +'South Joint:{}\n'.format(KBR)
                        +'North Joint:{}\n'.format(KFR)
                        +'No.0 Theoretical COORDINATES:\n'
                        +'Bulkhead Joint:(East, Center, West)\n'
                        +'South Joint:{}\n'.format(K[0,:])
                        +'North Joint:{}\n'.format(K[1,:]))
                else:
                    print('perfect cast:\n'
                    +'Match-Cast:\n'
                    +'North East Elev:{}\n'.format(res[0][5])
                    +'North West Elev:{}\n'.format(res[0][6])
                    +'South East Elev:{}\n'.format(res[0][3])
                    +'South West Elev:{}\n'.format(res[0][4])
                    +'North Offset:{}\n'.format(res[0][2])
                    +'South Offset:{}\n'.format(res[0][1])
                    +'Wet-Cast:\n'
                    +'North East Elev:{}\n'.format(res[0][9])
                    +'North West Elev:{}\n'.format(res[0][10])
                    +'South East Elev:{}\n'.format(res[0][7])
                    +'South West Elev:{}\n'.format(res[0][8])
                    +'North Offset:{}\n'.format(0)
                    +'South Offset:{}\n'.format(0)
                    +'Length:{}\n'.format(res[0][0])
                    +'Bulkhead:\n'
                    +'East Elev:{}\n'.format(res[2][2]+H)
                    +'West Elev:{}\n'.format(res[2][8]+H)
                    +'Center Offset:{}\n'.format(res[2][4]))

                    KCBR, KCFR, KFR=oneerr(E,bo,A,C,B,H,res,KK,d,cs)
                    
                    #加预拱度差值，新建参数的名称
                    # l1=KK[2][3]
                    # l2=KFR[3]
                    # l3=KCBR[3]
                    # l4=KCFR[3]
                    # L=abs(l1-l2)
                    # L1=abs(l2-l3)
                    # L2=abs(l2-l4)
                    # ygdc=YGDC[m-1]
                    # yB=ygdc/L*(L-L1)
                    # yF=ygdc/L*(L-L2)
                    # KFRSG=copy.copy(KFR)
                    # KCBRSG=copy.copy(KCBR)
                    # KCFRSG=copy.copy(KCFR)
                    # for i in range(3):
                    #     KFRSG[3*i+2]=KFRSG[3*i+2]+ygdc
                    #     KCBRSG[3*i+2]=KCBRSG[3*i+2]+yB
                    #     KCFRSG[3*i+2]=KCFRSG[3*i+2]+yF

                    print('No.{} AS-CAST COORDINATES:\n'.format(m)
                        +'Bolts and Hairpins(East Bolt, Hairpin, West Bolt):\n'
                        +'South:{}\n'.format(KCBR)
                        +'North:{}\n'.format(KCFR)
                        +'Joints(East, Center, West)\n'
                        +'North Joint:{}\n'.format(KFR)
                        +'No.{} Theoretical COORDINATES:\n'.format(m)
                        +'Bulkhead Joint:(East, Center, West)\n'
                        +'South Joint:{}\n'.format(K[m,:])
                        +'North Joint:{}\n'.format(K[m+1,:]))
            
                for i in range(9):
                    if m!=0:
                        KT[m+1][i]=KFR[i]
                    else:
                        KT[m][i]=KBR[i]
                        KT[m+1][i]=KFR[i]
                        # KTSG[m-1][i]=KFRSG[i]
                    KCT[2*m][i]=KCBR[i]
                    KCT[2*m+1][i]=KCFR[i]
                    # KCTSG[2*m][i]=KCBRSG[i]
                    # KCTSG[2*m+1][i]=KCFRSG[i]

                LT=math.sqrt((K[m][3]-K[m+1][3])**2+(K[m][4]-K[m+1][4])**2+(K[m][5]-K[m+1][5])**2)
                LA=math.sqrt((KT[m][3]-KT[m+1][3])**2+(KT[m][4]-KT[m+1][4])**2+(KT[m][5]-KT[m+1][5])**2)
                LTT=math.sqrt((K[0][3]-K[m+1][3])**2+(K[0][4]-K[m+1][4])**2+(K[0][5]-K[m+1][5])**2)
                LAT=math.sqrt((KT[0][3]-KT[m+1][3])**2+(KT[0][4]-KT[m+1][4])**2+(KT[0][5]-KT[m+1][5])**2)
                print('Length on Center:\n'
                      +'Theoretical:{}'.format(LT)+'Total:{}\n'.format(LTT)
                      +'As-Cast:{}'.format(LA)+'Total:{}\n'.format(LAT))

                #修正后的坐标更新入表格
                workbook = xlwt.Workbook(encoding='utf-8')
                sheet1=workbook.add_sheet('K')
                sheet2=workbook.add_sheet('KT')
                sheet3=workbook.add_sheet('KCT')
                # sheet4=workbook.add_sheet('YGD')
                # sheet5=workbook.add_sheet('KTSG')
                # sheet6=workbook.add_sheet('KCTSG')
                for i in range(len(K)):
                    for j in range(len(K[i])):
                        sheet1.write(i,j,K[i][j])
                for i in range(len(KT)):
                    for j in range(len(KT[i])):
                        sheet2.write(i,j,KT[i][j])
                        # sheet5.write(i,j,KTSG[i][j])
                for i in range(len(KCT)):
                    for j in range(len(KCT[i])):
                        sheet3.write(i,j,KCT[i][j])
                        # sheet6.write(i,j,KCTSG[i][j])
                # for i in range(len(YGDC)):
                #     sheet4.write(0,i,YGDC[i])

                savePath = path1
                workbook.save(savePath)
                print('现场拼装坐标已更新')
            else:
                print('退出运行')
        else:
            print('不进行坐标修正')
    else:
        print('退出运行')
    
    #输出预制偏差曲线数据
    table1 = xlrd.open_workbook(path1).sheets()[1]
    row1 = table1.nrows
    col1 = table1.ncols
    KT = np.zeros((row1, col1))
    for x in range(col1):
        cols1 = np.matrix(table1.col_values(x))
        KT[:, x] = cols1
    mm=0
    for i in range(row1):
        if KT[i,0]!=0:
            mm=mm+1
        else:
            break

    for i in range(mm):
        errL.append(KT[i,3]-K[i,3])
        errHP.append(KT[i,4]-K[i,4])
        errH.append(KT[i,5]-K[i,5])
    print('节段长度偏差曲线：',errL)#输出了曲线的纵坐标，横坐标为各节段号，下同
    print('节段偏距偏差曲线：',errHP)
    print('节段高程偏差曲线：',errH)

    #实际拼装误差，不影响制造线形
    #设定调节限值，偏差除以该限值，比较后续有几个节段数，确定用个节段进行修正
    #修正值叠加在预拱度差值上
    #更新表格预拱度差值数据
    #输出叠加拼装偏差后的曲线数据

    return       

def zeroerr(E,bo,A,C,R,K,d,cs):#0号节段实测误差修正
    #E=[固定模板偏距，右边线长，自由端模偏距，左边线长，CBLH,CFLH,CBRH,CFRH, 自由端模左侧高，自由端模右侧高,固定端模左侧高，固定端模右侧高,测点CBC偏距,测点CFC偏距]
    KCB=[0]*9
    KCF=[0]*9
    KF=[0]*9
    KB=[0]*9
    AN=bo[0]
    AS=bo[1]
    CN=bo[2]
    CS=bo[3]

    KCB[0]=E[3]-AS
    KCB[1]=E[2]+R[1][1]
    KCB[2]=R[1][2]+E[4]-R[0][3]
    KCB[6]=E[1]-AS
    KCB[7]=E[2]+R[1][7]
    KCB[8]=R[1][8]+E[6]-R[0][4]
    KCB[3]=(E[1]+E[3])/2-CS
    KCB[4]=R[1][4]+E[12]
    KCB[5]=R[1][5]

    KCF[0]=R[2][0]+AN
    KCF[1]=R[2][1]
    KCF[2]=R[2][2]+E[5]-R[0][5]
    KCF[6]=R[2][6]+AN
    KCF[7]=R[2][7]
    KCF[8]=R[2][8]+E[7]-R[0][6]
    KCF[3]=R[2][3]+CN
    KCF[4]=R[2][4]+E[13]
    KCF[5]=R[2][5]

    KB[0]=E[3]
    KB[1]=KCB[1]
    KB[2]=E[8]-R[0][3]
    KB[6]=E[1]
    KB[7]=KCB[7]
    KB[8]=E[9]-R[0][3]
    KB[3]=(KB[0]+KB[6])/2
    KB[4]=E[2]
    KB[5]=KCB[5]

    KF[0]=R[2][0]
    KF[1]=R[2][1]
    KF[2]=R[2][2]+E[10]-R[0][5]
    KF[6]=R[2][6]
    KF[7]=R[2][7]
    KF[8]=R[2][8]+E[11]-R[0][6]
    KF[3]=R[2][3]
    KF[4]=R[2][4]
    KF[5]=R[2][5]

    KCBR=revaxischa(K,d,KCB,cs)
    KCFR=revaxischa(K,d,KCF,cs)
    KBR=revaxischa(K,d,KB,cs)
    KFR=revaxischa(K,d,KF,cs)

    return KCBR, KCFR, KBR, KFR

#参数中引入A
def oneerr(E,bo,A,C,B,H,R,K,d,cs):#1号节及标准节段实测误差修正 
    KCB=[0]*9
    KCF=[0]*9
    KF=[0]*9
    rerr=[0]*24
    n=10**(-200)
    AN1=bo[0]
    CN1=bo[1]
    AN2=bo[2]
    AS2=bo[3]
    CN2=bo[4]
    CS2=bo[5]

    #rerr[0-11]
    for i in range(4):
        rerr[i]=E[6+i]-R[0][3+i]
    
    rerr[8]=(rerr[0]+rerr[1])/2
    rerr[10]=(rerr[2]+rerr[3])/2
    rerr[9]=(rerr[0]+rerr[3])-(rerr[1]+rerr[2])
    print('Twist:{}'.format(rerr[9]))
    rerr[11]=(R[4][5]+rerr[10])-(rerr[8]-rerr[10])/(R[3][3]-R[4][3])*(R[4][3]-R[5][3])
    rerr[4]=rerr[0]-rerr[8]
    rerr[5]=rerr[1]-rerr[8]
    rerr[6]=rerr[2]+rerr[9]/2-rerr[10]
    rerr[7]=rerr[3]-rerr[9]/2-rerr[10]


    #rerr[12-23]
    rerr[12]=E[1]-R[0][0]
    rerr[13]=E[3]-R[0][2]
    rerr[16]=math.sqrt((R[4][3]-R[3][3])**2+(R[4][4]-R[3][4])**2)
    rerr[17]=math.sqrt((R[4][3]-R[3][3])**2+(R[4][5]-R[3][5])**2)
    rerr[20]=E[2]-R[0][1]
    rerr[21]=math.sqrt((R[3][7]-R[3][1])**2+(R[3][8]-R[3][2])**2)
    rerr[14]=math.sin(2*math.asin(abs(rerr[13]-rerr[20])/2/rerr[16]))*\
        (rerr[20]-rerr[13]+n)/abs(rerr[20]-rerr[13]+n)*\
        (d*(K[4][3]-K[3][3])+n)/abs(d*(K[4][3]-K[3][3])+n)
    rerr[18]=math.sin(2*math.asin(abs(rerr[8]-rerr[10])/2/rerr[17]))*\
        (rerr[8]-rerr[10]+n)/abs(rerr[8]-rerr[10]+n)*\
        (d*(K[4][3]-K[3][3])+n)/abs(d*(K[4][3]-K[3][3])+n)
    rerr[22]=math.sin(2*math.asin(abs(rerr[6])/rerr[21]))*\
        (rerr[6]+n)/abs(rerr[6]+n)*\
        (-R[4][1]+n)/abs(R[4][1]+n)
    rerr[15]=math.sqrt(1-rerr[14]**2)
    rerr[19]=math.sqrt(1-rerr[18]**2)
    rerr[23]=math.sqrt(1-rerr[22]**2)

    KCB[0]=R[4][0]-AN1-AS2+R[4][1]*rerr[14]#修改A、C偏距
    KCB[1]=R[4][1]+rerr[13]
    KCB[2]=R[1][2]+E[10]-R[0][7]
    KCB[3]=R[4][3]-CN1-CS2
    KCB[4]=R[5][4]+E[14]
    KCB[5]=R[1][5]
    KCB[6]=R[4][6]-AN1-AS2+R[4][7]*rerr[14]
    KCB[7]=R[4][7]+rerr[13]
    KCB[8]=R[1][8]+E[11]-R[0][8]

    KCF[3]=R[2][3]+CN2-rerr[12]
    KCF[4]=R[2][4]+E[15]
    KCF[5]=R[2][5]
    KCF[0]=KCF[3]-CN2+AN2
    KCF[1]=KCF[4]+B
    KCF[2]=R[2][2]+E[12]-R[0][9]
    KCF[6]=KCF[3]-CN2+AN2
    KCF[7]=KCF[4]-B
    KCF[8]=R[2][8]+E[13]-R[0][10]

    KF[0]=R[2][0]-rerr[12]
    KF[1]=R[2][1]
    KF[2]=R[2][2]+E[4]-H
    KF[3]=R[2][3]-rerr[12]
    KF[4]=R[2][4]
    KF[5]=R[2][5]
    KF[6]=R[2][6]-rerr[12]
    KF[7]=R[2][7]
    KF[8]=R[2][8]+E[5]-H


    KCBr=spaxis(R,rerr,KCB)
    KCFr=spaxis(R,rerr,KCF)
    KFr=spaxis(R,rerr,KF)

    KCBR=revaxischa(K,d,KCBr,cs)
    KCFR=revaxischa(K,d,KCFr,cs)
    KFR=revaxischa(K,d,KFr,cs)

    return KCBR, KCFR, KFR

##修改
def revaxischa(K,d,k,cs):#反向坐标变换
    Sin1=cs[0]
    Cos1=cs[1]
    Sin2=cs[2]
    Cos2=cs[3]
    Sin3=cs[4]
    Cos3=cs[5]

    K0=copy.copy(k)
    for i in range(3):
        K0[3*i+1]=Cos3*k[3*i+1]+Sin3*k[3*i+2]
        K0[3*i+2]=-Sin3*k[3*i+1]+Cos3*k[3*i+2]

    K1=copy.copy(K0)
    for i in range(3):
        K1[3*i]=Cos2*K0[3*i]+Sin2*K0[3*i+2]
        K1[3*i+2]=-Sin2*K0[3*i]+Cos2*K0[3*i+2]
    
    K2=copy.copy(K1)
    for i in range(3):
        K2[3*i]=Cos1*K1[3*i]+Sin1*K1[3*i+1]
        K2[3*i+1]=-Sin1*K1[3*i]+Cos1*K1[3*i+1]

    K3=[]
    for i in range(3):
        for j in range(3):
            if (3*i+j+1)%3 == 0:
                K3.append((K2[3*i+j]+K[1][3+j]))
            else:
                K3.append(d*K2[3*i+j]+K[1][3+j])

    return K3

def spaxis(K,E,k):#误差修正时额外的坐标变换
    ka=[]
    rk=[0]*9

    for i in range(3):
        ka.append(K[3][3+i])
    
    for i in range(3):
        rk[3*i]=k[3*i]-ka[0]
        rk[3*i+1]=k[3*i+1]-ka[1]-E[20]
        rk[3*i+2]=k[3*i+2]-ka[2]-E[8]

    sin1=E[14]
    cos1=E[15]
    sin2=E[18]
    cos2=E[19]
    sin3=E[22]
    cos3=E[23]

    rk1=copy.copy(rk)
    for i in range(3):
        rk1[3*i]=rk[3*i]*cos1-rk[3*i+1]*sin1
        rk1[3*i+1]=rk[3*i]*sin1+rk[3*i+1]*cos1

    rk2=copy.copy(rk1)
    for i in range(3):
        rk2[3*i]=rk1[3*i]*cos2-rk1[3*i+2]*sin2
        rk2[3*i+2]=rk1[3*i]*sin2+rk1[3*i+2]*cos2

    rk3=copy.copy(rk2)
    for i in range(3):
        rk3[3*i+1]=rk2[3*i+1]*cos3-rk2[3*i+2]*sin3
        rk3[3*i+2]=rk2[3*i+1]*sin3+rk2[3*i+2]*cos3
    
    for i in range(3):
        rk[3*i]=rk3[3*i]+ka[0]
        rk[3*i+1]=rk3[3*i+1]+ka[1]
        rk[3*i+2]=rk3[3*i+2]+ka[2]

    rk[2]=rk[2]+E[9]/2
    rk[8]=rk[8]-E[9]/2

    return rk

def judge(k,m,s):#判定函数
    if s==0:
        for i in range(len(k)):
            if k[i][0]==0:
                if m-1<=i:
                    print('判定通过！')
                    J=True
                else:
                    print('判定不通过！当前记录允许计算第{}节段'.format(i+1))
                    J=False
                break
    else:
        if m==0:
            if len(k)==14:
                print('实测数据输入正确')
                J=True
            else:
                print('请检查实测数据输入格式！')
                J=False
        else:
            if len(k)==16:
                print('实测数据输入正确')
                J=True
            else:
                print('请检查实测数据输入格式！')
                J=False
    return J

#输入参数
path1=r'D:\projects\三航局\宝山\短线预制线型控制算法\Kr.xlsx'#桥梁线形参数表
pathA=r'D:\projects\三航局\宝山\短线预制线型控制算法\K.xlsx'
pathB=r'D:\projects\三航局\宝山\短线预制线型控制算法\ZZYGD.xlsx'
A=0.1#参数A
B=2.9#参数B
C=0.1#参数C
H=1#相对标高H
m=1#查询的节段号
first=False#是否新建工程
st=1#st=0，仅查询预制匹配参数；st=1，查询并输入实测数据进行修正
bo=[0.1,0.1,0.1,0.1,0.1,0.1]#bo：标准段测点偏位[aN1,cN1,aN2,aS2,cN2,cS2] !需要保存
#bo=[0.1,0.1,0.1,0.1]#bo：起始段测点偏位[aN,aS,cN,cS]

#E=[0.001,2.8,-0.0028,2.798,1.0063,1.0005,1.0060,1.0039, 1.01, 1.0051,1,1,-0.001,-0.001]
#0号段实测数据：[Fixed Bulkhead Center Offset,West Length,Floating Bulkhead Center Offset,East Length，
#  Wet-Cast South East Elev,Wet-Cast North East Elev,Wet-Cast South West Elev,Wet-Cast North West Elev,
# Floating Bulkhead East Elev,Floating Bulkhead West Elev,
# Fixed Bulkhead East Elev,Fixed Bulkhead West Elev,
# Wet-Cast South Offset,Wet-Cast North Offset]

E=[0,2.9950,0,-0.0043,0.9996,1.0004,0.9992,1.0025,0.9986,1.005,0.9944,1.0059,0.9985,1.005,0,0]
#E=[0,3.025,0.0031,0.0049,1,0.9999,0.9915,1.0017,1.0004,1.0036,1.0023,1.0116,1.0064,1.0066,0,0]
#E=[0,3.005,0.0017,0.0042,1,1,1.0055,1.0155,1.0084,1.0085,1.0119,1.0078,1.0052,1.0033,0,0]

#标准段实测数据：[Bulkhead Center Offset，Wet-Cast Length，Match-Cast Center Offset(South)，Match-Cast Center Offset(North)，Bulkhead East Elev，Bulkhead West Elev，
# Match-Cast South East Elev,Match-Cast South West Elev,Match-Cast North East Elev,Match-Cast North West Elev,
# Wet-Cast South East Elev,Wet-Cast South West Elev,Wet-Cast North East Elev,Wet-Cast North West Elev,
# Wet-Cast Center Offset(South)，Wet-Cast Center Offset(North)]

main(path1,m,A,B,C,H,first,st,E,bo,pathA,pathB)