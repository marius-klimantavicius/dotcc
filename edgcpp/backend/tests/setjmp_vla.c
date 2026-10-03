#include <setjmp.h>
static jmp_buf environment[2];
static void child(jmp_buf target,int value,int n) {
  int array[n+2];array[0]=51;
  if(array[0]==51)longjmp(target,value);
}
static int before_vla(int n) {
  volatile int visits=0;
  setjmp(environment[0]);
  if(++visits==5)return visits;
  int array[n];array[0]=37;
  child(environment[0],array[0],n);
  return -1;
}
static int after_vla(int n) {
  int surviving[n];surviving[0]=41;surviving[n-1]=43;
  volatile int visits=0;
  setjmp(environment[0]);
  if(surviving[0]!=41 || surviving[n-1]!=43)return -2;
  if(++visits==4)return visits;
  int temporary[n+1];temporary[0]=17;
  child(environment[0],temporary[0],n);
  return -3;
}
static int vla_buffer(int n) {
  jmp_buf buffers[n];
  int surviving[n];surviving[0]=61;
  if(setjmp(buffers[1]))return surviving[0];
  int temporary[n+3];temporary[0]=1;
  child(buffers[1],temporary[0],n);
  return -4;
}
static int nested_marks(int n) {
  volatile int phase=0;
  switch(setjmp(environment[0])){case 0:break;case 7:return phase;default:return -5;}
  int surviving[n];surviving[0]=71;
  switch(setjmp(environment[1])) {
    case 0:break;
    case 8:if(surviving[0]!=71)return -6;phase=9;longjmp(environment[0],7);
    default:return -7;
  }
  int temporary[n+1];temporary[0]=8;
  child(environment[1],temporary[0],n);
  return -8;
}
static int escaped_scope(int n) {
  {int array[n];array[0]=17;if(setjmp(environment[0]))return array[0];}
  /* Source UB: the variably modified identifier's scope has ended. The
     translated runtime deliberately diagnoses its now-dead allocation mark. */
  longjmp(environment[0],1);return -9;
}
static int vm_pointer(int n) {
  /* A variably modified pointer type needs no VLA allocation. Its scope stays
     active across both returns from setjmp. */
  int (*pointer)[n]=0;
  volatile int visits=0;
  if(setjmp(environment[0]))return sizeof(*pointer)==(unsigned long)n*sizeof(int) && visits==1;
  ++visits;longjmp(environment[0],1);return 0;
}
int main(int argc,char**argv) {
#ifdef __EDG_DOT_JMP_BUF_ABI_VERSION
  if(__EDG_DOT_JMP_BUF_ABI_VERSION!=2 || sizeof(jmp_buf)!=32)return 90;
#endif
  if(argc>1 && argv[1][0]=='e')return escaped_scope(7);
  for(int n=3;n<8;++n) {
    if(before_vla(n)!=5)return 1;
    if(after_vla(n)!=4)return 2;
    if(vla_buffer(n)!=61)return 3;
    if(nested_marks(n)!=9)return 4;
    if(!vm_pointer(n))return 5;
  }
  return 0;
}
