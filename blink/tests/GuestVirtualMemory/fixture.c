/* Valid ordinary Linux anonymous reservations under explicit process budgets. */
typedef unsigned long u64;
static long call6(long n, long a, long b, long c, long d, long e, long f) {
  register long r10 __asm__("r10") = d, r8 __asm__("r8") = e, r9 __asm__("r9") = f;
  long result;
  __asm__ volatile("syscall" : "=a"(result) : "a"(n), "D"(a), "S"(b), "d"(c),
      "r"(r10), "r"(r8), "r"(r9) : "rcx", "r11", "memory");
  return result;
}
#define SC(n,a,b,c,d,e,f) call6(n,(long)(a),(long)(b),(long)(c),(long)(d),(long)(e),(long)(f))
static __attribute__((noreturn)) void finish(int status) {
  SC(231,status,0,0,0,0,0); __builtin_unreachable();
}
#define CHECK(x) do { if (!(x)) finish(__LINE__); } while (0)
void _start(void) {
  u64 as[2], data[2];
  CHECK(SC(97,9,as,0,0,0,0)==0 && SC(97,2,data,0,0,0,0)==0);
  CHECK(as[0]==as[1] && data[0]==33554432 && data[1]==33554432);
  CHECK(as[0]==33554432 || as[0]==1073741824);
  long address=SC(9,0,536870912,0,34,-1,0);
  if (as[0]==33554432) {
    CHECK(address==-12);
    address=SC(9,0,1048576,3,34,-1,0);
    CHECK((u64)address<(u64)-4095);
    volatile unsigned char *bytes=(volatile unsigned char *)address;
    bytes[0]=17;bytes[1048575]=23;
    CHECK(bytes[0]+bytes[1048575]==40);
    CHECK(SC(11,address,1048576,0,0,0,0)==0);
    static const char message[]="guest-virtual-memory: default limit retained: PASS\n";
    CHECK(SC(1,1,message,sizeof(message)-1,0,0,0)==sizeof(message)-1);
  } else {
    CHECK((u64)address<(u64)-4095);
    CHECK(SC(10,address,4096,3,0,0,0)==0);
    CHECK(SC(10,address+536870912-4096,4096,3,0,0,0)==0);
    volatile unsigned char *bytes=(volatile unsigned char *)address;
    bytes[0]=37;bytes[536870911]=41;
    CHECK(bytes[0]+bytes[536870911]==78);
    CHECK(SC(11,address,536870912,0,0,0,0)==0);
    static const char message[]="guest-virtual-memory: sparse reservation and backing: PASS\n";
    CHECK(SC(1,1,message,sizeof(message)-1,0,0,0)==sizeof(message)-1);
  }
  finish(0);
}
