import math

def normalize(k,B):#输入1行数据以及参数B，对该行数据归一化
    for i in range(2):
        klx=k[0+6*i]-k[3]
        kly=k[1+6*i]-k[4]
        klz=k[2+6*i]-k[5]
        b=math.sqrt(klx**2+kly**2+klz**2)
        k[0+6*i]=B/b*klx+k[3]
        k[1+6*i]=B/b*kly+k[4]
        k[2+6*i]=B/b*klz+k[5]
    print(k)
    return k

k=[1,2,3,0,0,0,-1,-2,-3]
B=5
normalize(k,B)