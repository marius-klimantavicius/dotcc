typedef int* IntPointer;
static void copy(int* __restrict target,const int* __restrict source,int n) {
 for(int i=0;i<n;++i)target[i]=source[i]*3+1;
}
static int sum(IntPointer __restrict data,int n) {
 int result=0;for(int i=0;i<n;++i)result+=data[i];return result;
}
int main(void) {
 int source[31],target[31];for(int i=0;i<31;++i)source[i]=i;
 void (*callback)(int* __restrict,const int* __restrict,int)=copy;
 callback(target,source,31);
 if(sum(target,31)!=1426)return 1;
 for(int i=0;i<31;++i)if(source[i]!=i || target[i]!=i*3+1)return 2;
 return 0;
}
