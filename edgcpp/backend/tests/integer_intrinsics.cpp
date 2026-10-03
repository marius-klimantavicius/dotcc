static unsigned long long state=0x2545f4914f6cdd1dULL;
static unsigned long long next_word() {state^=state<<13;state^=state>>7;state^=state<<17;return state;}
static int leading(unsigned long long value,int width) {
 int n=0;for(int i=width-1;i>=0;--i){if(value&(1ULL<<i))break;++n;}return n;
}
static int trailing(unsigned long long value,int width) {
 int n=0;while(n<width && !(value&(1ULL<<n)))++n;return n;
}
static int population(unsigned long long value) {int count=0;while(value){count+=(int)(value&1);value>>=1;}return count;}
static unsigned long long reverse(unsigned long long value,int width) {
 unsigned long long result=0;for(int i=0;i<width;++i){result=(result<<8)|(value&255);value>>=8;}return result;
}
static int calls;
static unsigned observed() {++calls;return 17;}
int main() {
 for(unsigned i=0;i<10000;++i) {
  unsigned long long x=i<64?(1ULL<<i):next_word();unsigned y=(unsigned)x;
  if(x && (__builtin_clzl((unsigned long)x)!=leading(x,64) || __builtin_clzll(x)!=leading(x,64) ||
           __builtin_ctzl((unsigned long)x)!=trailing(x,64) || __builtin_ctzll(x)!=trailing(x,64)))return 1;
  if(y && (__builtin_clz(y)!=leading(y,32) || __builtin_ctz(y)!=trailing(y,32)))return 2;
  if(__builtin_popcount(y)!=population(y) || __builtin_popcountl((unsigned long)x)!=population(x) ||
     __builtin_popcountll(x)!=population(x))return 3;
  if(__builtin_parity(y)!=(population(y)&1) || __builtin_parityl((unsigned long)x)!=(population(x)&1) ||
     __builtin_parityll(x)!=(population(x)&1))return 4;
  if(__builtin_ffs((int)y)!=(y?trailing(y,32)+1:0) || __builtin_ffsl((long)x)!=(x?trailing(x,64)+1:0) ||
     __builtin_ffsll((long long)x)!=(x?trailing(x,64)+1:0))return 5;
  int a=(int)y;long long b=(long long)x;
  if(__builtin_clrsb(a)!=leading(a<0?(unsigned)~a:(unsigned)a,32)-1 ||
     __builtin_clrsbl((long)b)!=leading(b<0?(unsigned long long)~b:(unsigned long long)b,64)-1 ||
     __builtin_clrsbll(b)!=leading(b<0?(unsigned long long)~b:(unsigned long long)b,64)-1)return 6;
  if(__builtin_bswap16((unsigned short)y)!=(unsigned short)reverse(y,2) ||
     __builtin_bswap32(y)!=(unsigned)reverse(y,4) || __builtin_bswap64(x)!=reverse(x,8))return 7;
  unsigned __int128 wide=((unsigned __int128)x<<64)|y;
  if(__builtin_bswap128(wide)!=(((unsigned __int128)reverse(y,8)<<64)|reverse(x,8)))return 8;
 }
 if(__builtin_ffs(0) || __builtin_ffsll(0) || __builtin_clrsb(0)!=31 || __builtin_clrsb(-1)!=31)return 9;
 if(__builtin_clz(observed())!=27 || calls!=1)return 10;
 return 0;
}
