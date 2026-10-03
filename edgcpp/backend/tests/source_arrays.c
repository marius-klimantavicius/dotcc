/* Named fixed-array views retain layout, aliasing and expression evaluation. */
struct Item { long long value; int extra; };
static int global_values[5]={2,3,5,7,11};
#ifdef __cplusplus
static thread_local int thread_values[3]={13,17,19};
#else
static _Thread_local int thread_values[3]={13,17,19};
#endif
static int calls;
static int next_index(void){++calls;return 2;}
static int named_arrays(void){
 int values[5]={1,2,3,4,5};int *pointer=values;
 values[2]+=4;if(pointer[2]!=7)return 1;
 int *one_past=&values[5];if(one_past-pointer!=5)return 2;
 unsigned long index=3;values[index]=9;if(values[3]!=9)return 3;
 calls=0;int *selected=&values[next_index()];if(calls!=1 || selected!=values+2)return 4;
 calls=0;int prior=values[next_index()]++;if(calls!=1 || prior!=7 || values[2]!=8)return 5;
 int *pointers[3]={values,values+1,values+2};
 pointers[1]+=2;if(pointers[1]!=values+3 || *pointers[1]!=9)return 6;
 struct Item items[3]={{2,1},{3,2},{5,4}};items[1].value+=7;
 if(items[1].value!=10 || items[2].extra!=4)return 7;
 int matrix[2][3]={{1,2,3},{4,5,6}};int (*rows)[3]=matrix;
 matrix[1][2]+=8;if(rows[1][2]!=14)return 8;
 global_values[1]+=3;thread_values[1]+=6;
 if(global_values[1]!=6 || thread_values[1]!=23)return 9;
 global_values[1]-=3;thread_values[1]-=6;
 return 0;
}
static int copy_buffer(void){
 unsigned char buf[10];int count=0;unsigned value=0x4321;
 do{buf[count++]=(unsigned char)(value&127);value>>=7;}while(value);
 unsigned sum=0;for(int index=0;index<count;++index)sum+=buf[index];
 return sum==40?0:10;
}
int main(void){int result=named_arrays();return result?result:copy_buffer();}
