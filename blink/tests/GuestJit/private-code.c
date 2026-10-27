/* Ordinary finite generated guest code, called on Linux natively as a witness.
 * RW fill -> RX call; patch and remap at the same valid guest address. */
typedef unsigned long u64;
static long sc(long n, long a, long b, long c, long d, long e, long f) {
  register long r10 __asm__("r10")=d, r8 __asm__("r8")=e, r9 __asm__("r9")=f;
  long result;
  __asm__ volatile("syscall":"=a"(result):"a"(n),"D"(a),"S"(b),"d"(c),"r"(r10),"r"(r8),"r"(r9):"rcx","r11","memory");
  return result;
}
#define SC(n,a,b,c,d,e,f) sc(n,(long)(a),(long)(b),(long)(c),(long)(d),(long)(e),(long)(f))
static __attribute__((noreturn)) void finish(int status) { SC(231,status,0,0,0,0,0);__builtin_unreachable(); }
#define CHECK(x) do { if (!(x)) finish(__LINE__); } while (0)
static void fill(unsigned char *p, unsigned char step) {
  static const unsigned char code[]={0xb9,0xe8,3,0,0,0x31,0xc0,0x48,0x83,0xc0,1,0xff,0xc9,0x75,0xf8,0xc3};
  for (int i=0;i<sizeof(code);++i) p[i]=code[i];
  p[10]=step;
}
void _start(void) {
  long p=SC(9,0,4096,3,34,-1,0);CHECK((u64)p<(u64)-4095);
  fill((unsigned char*)p,1);CHECK(SC(10,p,4096,5,0,0,0)==0);
  CHECK(((long(*)(void))p)()==1000);
  CHECK(SC(10,p,4096,3,0,0,0)==0);((unsigned char*)p)[10]=2;
  CHECK(SC(10,p,4096,5,0,0,0)==0);CHECK(((long(*)(void))p)()==2000);
  CHECK(SC(10,p,4096,0,0,0,0)==0);CHECK(SC(10,p,4096,5,0,0,0)==0);
  CHECK(((long(*)(void))p)()==2000);
  CHECK(SC(11,p,4096,0,0,0,0)==0);
  CHECK(SC(9,p,4096,3,50,-1,0)==p);fill((unsigned char*)p,3);
  CHECK(SC(10,p,4096,5,0,0,0)==0);CHECK(((long(*)(void))p)()==3000);
  CHECK(SC(11,p,4096,0,0,0,0)==0);
  finish(0);
}
