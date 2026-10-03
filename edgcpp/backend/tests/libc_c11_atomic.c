/* Direct C11 storage semantics; deliberately independent of stdatomic macros. */
typedef struct { long first,second; } AtomicPairValue;
static _Atomic(int) counter;
static _Atomic(int*) address;
static _Atomic(double) real_value;
static _Atomic(long double) extended_value;
static _Atomic(unsigned __int128) very_wide;
static _Atomic(AtomicPairValue) pair_value;
static volatile _Atomic(unsigned char) tiny;
static struct {unsigned char before;_Atomic(unsigned char) value;unsigned char after;} guarded={0x5a,0,0xa5};
void c11_byte_increment(void){++guarded.value;}
int c11_byte_guard(void){return guarded.before==0x5a && guarded.after==0xa5;}

void c11_increment(void){counter++;}
int c11_counter(void){return counter;}
void c11_reset(void){counter=0;}
int c11_implicit_cases(void){
 guarded.value=0;for(int i=0;i<1024;++i)++guarded.value;if(guarded.value!=0||!c11_byte_guard())return __LINE__;
 counter=3;if(counter!=3)return __LINE__;
 if(counter++!=3||++counter!=5||counter--!=5||--counter!=3)return __LINE__;
 if((counter+=7)!=10||(counter-=3)!=7)return __LINE__;
 if((counter|=8)!=15||(counter&=11)!=11||(counter^=3)!=8)return __LINE__;
 if((counter*=3)!=24||(counter/=2)!=12||(counter%=5)!=2)return __LINE__;
 if((counter<<=3)!=16||(counter>>=2)!=4)return __LINE__;
 counter=2147483647;if(++counter!=(-2147483647-1))return __LINE__;
 tiny=255;if(++tiny!=0||tiny--!=0||tiny!=255)return __LINE__;
 int buffer[12];address=buffer+3;
 if(address++!=buffer+3||address!=buffer+4||++address!=buffer+5)return __LINE__;
 if((address+=3)!=buffer+8||(address-=2)!=buffer+6||address--!=buffer+6||--address!=buffer+4)return __LINE__;
 real_value=1.5;if(real_value++!=1.5||real_value!=2.5||--real_value!=1.5)return __LINE__;
 if((real_value+=0.25)!=1.75||(real_value*=2)!=3.5||(real_value/=2)!=1.75)return __LINE__;
 union {unsigned long bits;double value;} nan,observed;nan.bits=0x7ff8000000001234UL;
 real_value=nan.value;observed.value=real_value;if(observed.bits!=nan.bits)return __LINE__;
 double expected=0,replacement=-0.0;
 if(__atomic_compare_exchange(&real_value,&expected,&replacement,0,__ATOMIC_SEQ_CST,__ATOMIC_SEQ_CST))return __LINE__;
 observed.value=expected;if(observed.bits!=nan.bits)return __LINE__;
 if(!__atomic_compare_exchange(&real_value,&expected,&replacement,0,__ATOMIC_SEQ_CST,__ATOMIC_SEQ_CST))return __LINE__;
 observed.value=real_value;if(observed.bits!=0x8000000000000000UL)return __LINE__;
 extended_value=1.5L;if(extended_value++!=1.5L||extended_value!=2.5L)return __LINE__;
 if((extended_value+=0.25L)!=2.75L||(extended_value*=2)!=5.5L)return __LINE__;
 very_wide=((unsigned __int128)1<<100);if(very_wide++!=((unsigned __int128)1<<100)||very_wide!=(((unsigned __int128)1<<100)+1))return __LINE__;
 AtomicPairValue initial={17,29},snapshot,desired={31,41};pair_value=initial;snapshot=pair_value;
 if(snapshot.first!=17||snapshot.second!=29)return __LINE__;
 if(!__atomic_compare_exchange(&pair_value,&snapshot,&desired,0,__ATOMIC_SEQ_CST,__ATOMIC_SEQ_CST))return __LINE__;
 snapshot=pair_value;if(snapshot.first!=31||snapshot.second!=41)return __LINE__;
 return 0;
}
#ifdef C11_ATOMIC_ORACLE_MAIN
int main(void){return c11_implicit_cases();}
#endif
