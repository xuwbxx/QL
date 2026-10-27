import math

#[Shen, Lin] k的格式是: Xe,Ye,Ze,Xc,Yc,Zc,Xw,Yw,Zw
def normalize(k,B):#输入1行数据以及参数B，对该行数据归一化
    for i in range(2):
        klx=k[0+6*i]-k[3] #[Shen, Lin] Xw到Xc的距离？
        kly=k[1+6*i]-k[4]
        klz=k[2+6*i]-k[5]
        b=math.sqrt(klx**2+kly**2+klz**2) #[Shen, Lin] 我记得定义上b是w到中线垂线距离，这里好像就是取的w到c的距离？
        k[0+6*i]=B/b*klx+k[3]
        k[1+6*i]=B/b*kly+k[4]
        k[2+6*i]=B/b*klz+k[5]

        # [Shen, Lin] 因为偏移是6，所以0是E点，1是W点，已经是两个点了
    print(k)
    return k

k=[1,2,3,0,0,0,-1,-2,-3]
B=5
normalize(k,B)