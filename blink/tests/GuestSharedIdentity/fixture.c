/* Ordinary Linux memfd aliases, futex synchronization and concurrent dup2.
 * Static valid ELF; all mappings, stacks and words are ordinary guest memory. */
typedef unsigned long u64;
struct timespec { long sec, nsec; };
static long sc(long n, long a, long b, long c, long d, long e, long f) {
  register long r10 __asm__("r10") = d, r8 __asm__("r8") = e, r9 __asm__("r9") = f;
  long result;
  __asm__ volatile("syscall" : "=a"(result) : "a"(n), "D"(a), "S"(b), "d"(c),
                   "r"(r10), "r"(r8), "r"(r9) : "rcx", "r11", "memory");
  return result;
}
#define SC(n,a,b,c,d,e,f) sc(n,(long)(a),(long)(b),(long)(c),(long)(d),(long)(e),(long)(f))
static __attribute__((noreturn)) void finish(int status) {
  SC(231,status,0,0,0,0,0); __builtin_unreachable();
}
#define CHECK(x) do { if (!(x)) finish(__LINE__); } while (0)
static void *mapping(long length, int flags, int fd, long offset) {
  long result=SC(9,0,length,3,flags,fd,offset);
  CHECK((u64)result < (u64)-4095);
  return (void*)result;
}
static void pause(void) {
  struct timespec delay={0,1000000};
  CHECK(SC(35,&delay,0,0,0,0,0)==0);
}
extern long spawn(void*, int*, void (*)(void));
static unsigned char stack[65536] __attribute__((aligned(4096)));
static int tid, ready, done, stop;
static volatile unsigned *waitword;
static int waitop;
static void join(void) {
  for (;;) {
    int value=__atomic_load_n(&tid,__ATOMIC_SEQ_CST);
    if (!value) break;
    struct timespec limit={2,0};
    long rc=SC(202,&tid,0,value,&limit,0,0);
    CHECK(rc==0 || rc==-11 || rc==-4);
  }
}
static void waiter(void) {
  struct timespec limit={2,0};
  if (waitop==9) { CHECK(SC(228,1,&limit,0,0,0,0)==0); limit.sec+=2; }
  __atomic_store_n(&ready,1,__ATOMIC_SEQ_CST);
  CHECK(SC(202,waitword,waitop,0,&limit,0,2)==0);
  __atomic_store_n(&done,1,__ATOMIC_SEQ_CST);
}
static long wake(volatile unsigned *word, int op, int mask) {
  return SC(202,word,op,1,0,0,mask);
}
static void anonymous_futexes(void) {
  unsigned *word=mapping(4096,34,-1,0);
  waitword=word;
  for (int phase=0;phase<2;++phase) {
    ready=done=0; waitop=phase ? 0 : 128;
    CHECK(spawn(stack+sizeof(stack),&tid,waiter)>0);
    while (!__atomic_load_n(&ready,__ATOMIC_SEQ_CST)) pause();
    for (int i=0;i<20;++i) pause();
    CHECK(wake(word,phase ? 129 : 1,-1)==0);
    int selected=0;
    for (int i=0;i<200 && !selected;++i) {
      long rc=wake(word,phase ? 1 : 129,-1); CHECK(rc==0 || rc==1);
      selected=(int)rc; if (!selected) pause();
    }
    CHECK(selected==1); join(); CHECK(done==1);
  }
  CHECK(SC(11,word,4096,0,0,0,0)==0);
}
static void futex_aliases(void) {
  int fd=SC(319,"shared-futex",1,0,0,0,0);
  int other=SC(319,"different-file",1,0,0,0,0);
  CHECK(fd>=0 && other>=0);
  CHECK(SC(77,fd,8192,0,0,0,0)==0 && SC(77,other,4096,0,0,0,0)==0);
  unsigned *a=mapping(8192,1,fd,0), *b=mapping(4096,1,fd,4096);
  unsigned *independent=mapping(4096,1,other,0);
  CHECK(a!=b && SC(3,fd,0,0,0,0,0)==0 && SC(3,other,0,0,0,0,0)==0);
  waitword=a+1024+16;
  for (int phase=0;phase<3;++phase) {
    ready=done=0; waitop=phase==1 ? 128 : phase==2 ? 9 : 0;
    CHECK(spawn(stack+sizeof(stack),&tid,waiter)>0);
    while (!__atomic_load_n(&ready,__ATOMIC_SEQ_CST)) pause();
    for (int i=0;i<20;++i) pause();
    CHECK(wake(b+17,phase==1 ? 129 : 1,-1)==0); /* Different word. */
    CHECK(wake(independent+16,1,-1)==0); /* Different backing. */
    CHECK(wake(b+16,129,-1)==0); /* PRIVATE does not alias across VAs. */
    if (phase==1) CHECK(wake(waitword,1,-1)==0);
    else CHECK(wake(waitword,129,-1)==0);
    if (phase==2) CHECK(wake(b+16,10,1)==0); /* Nonmatching bitset. */
    int selected=0;
    for (int i=0;i<200 && !selected;++i) {
      long rc=phase==1 ? wake(waitword,129,-1) : wake(b+16,phase==2 ? 10 : 1,2);
      CHECK(rc==0 || rc==1); selected=(int)rc;
      if (!selected) pause();
    }
    CHECK(selected==1); join(); CHECK(done==1);
  }
  CHECK(SC(11,a,8192,0,0,0,0)==0 && SC(11,b,4096,0,0,0,0)==0);
  CHECK(SC(11,independent,4096,0,0,0,0)==0);
}
static int file_a, file_b, target;
static void replace(void) {
  __atomic_store_n(&ready,1,__ATOMIC_SEQ_CST);
  while (!__atomic_load_n(&stop,__ATOMIC_SEQ_CST)) {
    CHECK(SC(33,file_a,target,0,0,0,0)==target);
    CHECK(SC(33,file_b,target,0,0,0,0)==target);
  }
}
static void mapping_identity(void) {
  file_a=SC(319,"mapping-A",1,0,0,0,0); file_b=SC(319,"mapping-B",1,0,0,0,0);
  CHECK(file_a>=0 && file_b>=0);
  CHECK(SC(77,file_a,65536,0,0,0,0)==0 && SC(77,file_b,65536,0,0,0,0)==0);
  for (int page=0;page<16;++page) {
    unsigned char a=65,b=66;
    CHECK(SC(18,file_a,&a,1,page*4096,0,0)==1);
    CHECK(SC(18,file_b,&b,1,page*4096,0,0)==1);
  }
  target=SC(32,file_a,0,0,0,0,0); CHECK(target>=0);
  ready=stop=0; CHECK(spawn(stack+sizeof(stack),&tid,replace)>0);
  while (!__atomic_load_n(&ready,__ATOMIC_SEQ_CST)) pause();
  for (int mode=1;mode<=2;++mode) for (int round=0;round<12;++round) {
    volatile unsigned char *bytes=mapping(65536,mode,target,0);
    unsigned char first=bytes[0]; CHECK(first==65 || first==66);
    for (int page=1;page<16;++page) CHECK(bytes[page*4096]==first);
    CHECK(SC(11,bytes,65536,0,0,0,0)==0);
  }
  __atomic_store_n(&stop,1,__ATOMIC_SEQ_CST); join();
  CHECK(SC(3,target,0,0,0,0,0)==0 && SC(3,file_a,0,0,0,0,0)==0);
  CHECK(SC(3,file_b,0,0,0,0,0)==0);
}
void guestmain(void) {
  futex_aliases(); anonymous_futexes(); mapping_identity();
  static const char message[]="guest-shared-identity: alias/private/anon/bitset/offset/dup2 shared+private: PASS\n";
  CHECK(SC(1,1,message,sizeof(message)-1,0,0,0)==sizeof(message)-1);
  finish(0);
}
