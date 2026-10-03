/* Synthetic volatile scalar/packed bitfield semantics. No racing accesses or
   target MMIO access-width assumptions enter this oracle. */
#ifdef __cplusplus
#define VBOOL bool
#define VTLS thread_local
extern "C" {
#else
#define VBOOL _Bool
#define VTLS _Thread_local
#endif
struct __attribute__((packed)) Packed {
 unsigned char before;
 unsigned long long guard:3;
 volatile unsigned long long u:13;
 volatile signed long long s:11;
 unsigned long long tail:5;
 unsigned char after;
};
struct Plain { unsigned first:5; signed second:6; VBOOL flag:1; unsigned last:4; };
struct Boolean { volatile VBOOL flag:1; unsigned neighbor:7; };
static struct Packed packed;
static volatile struct Plain qualified;
static VTLS volatile struct Plain tls;
static unsigned receivers,arguments;
static unsigned long long trace;
static long long trace_values[8192];
static unsigned trace_count;
static void record(long long value){trace_values[trace_count++]=value;trace^=(unsigned long long)value;trace*=1099511628211ULL;}
unsigned volatile_bitfield_trace_count(void){return trace_count;}
long long volatile_bitfield_trace_value(unsigned index){return trace_values[index];}
static struct Packed *receiver(void){++receivers;return &packed;}
static volatile struct Plain *qualified_receiver(void){++receivers;return &qualified;}
static unsigned long long argument(unsigned long long value){++arguments;return value;}
static void counts(void){record(receivers);record(arguments);receivers=arguments=0;}
static void reset(void){
 packed.before=0xa5;packed.guard=5;packed.u=17;packed.s=-19;packed.tail=21;packed.after=0x5a;
 qualified.first=3;qualified.second=-7;qualified.flag=0;qualified.last=13;
 receivers=arguments=0;
}
static int neighbors(void){return packed.before==0xa5&&packed.guard==5&&packed.tail==21&&packed.after==0x5a&&qualified.last==13;}
unsigned long long volatile_bitfield_cases(void){
 trace=1469598103934665603ULL;trace_count=0;reset();
 record(receiver()->u);counts();record(receiver()->s);counts();
 record(receiver()->u=argument(8195));counts();record(packed.u);
 record(receiver()->s=(long long)argument(2047));counts();record(packed.s);
 record(receiver()->u+=argument(8191));counts();record(packed.u);
 record(receiver()->u-=argument(19));counts();record(packed.u);
 record(receiver()->u*=argument(7));counts();record(packed.u);
 record(receiver()->u/=argument(3));counts();record(packed.u);
 record(receiver()->u%=argument(23));counts();record(packed.u);
 record(receiver()->u|=argument(0x1700));counts();record(packed.u);
 record(receiver()->u&=argument(0x1f7f));counts();record(packed.u);
 record(receiver()->u^=argument(0x03a5));counts();record(packed.u);
 record(receiver()->u<<=argument(2));counts();record(packed.u);
 record(receiver()->u>>=argument(3));counts();record(packed.u);
 packed.u=8191;record(receiver()->u++);counts();record(packed.u);
 packed.u=8191;record(++receiver()->u);counts();record(packed.u);
 packed.u=0;record(receiver()->u--);counts();record(packed.u);
 packed.u=0;record(--receiver()->u);counts();record(packed.u);
 packed.s=1023;record(receiver()->s++);counts();record(packed.s);
 packed.s=1023;record(++receiver()->s);counts();record(packed.s);
 packed.s=-1024;record(receiver()->s--);counts();record(packed.s);
 packed.s=-1024;record(--receiver()->s);counts();record(packed.s);
 packed.s=-13;record(receiver()->s+=(long long)argument(2041));counts();record(packed.s);
 record(receiver()->s-=(long long)argument(57));counts();record(packed.s);
 record(receiver()->s*=-(long long)argument(3));counts();record(packed.s);
 record(receiver()->s/=-(long long)argument(7));counts();record(packed.s);
 record(receiver()->s%=-(long long)argument(11));counts();record(packed.s);
 /* Narrow unsigned fields promote to signed int with this target ABI. */
 packed.u=37;record(receiver()->u/=-1);counts();record(packed.u);
 packed.u=37;record(receiver()->u%=-11);counts();record(packed.u);
 packed.s=17;record(receiver()->s<<=argument(3));counts();record(packed.s);
 packed.s=-257;record(receiver()->s>>=argument(4));counts();record(packed.s);
 record(receiver()->u=42.75);counts();record(receiver()->u+=1.75);counts();
 record(receiver()->s=-19.75);counts();record(receiver()->s+=0.5);counts();
 (void)receiver()->u;counts();receiver()->u=7;counts();++receiver()->u;counts();receiver()->s--;counts();
 record(qualified_receiver()->first);counts();record(qualified_receiver()->second);counts();
 record(qualified_receiver()->first=argument(37));counts();record(qualified_receiver()->first+=argument(29));counts();
 record(qualified_receiver()->second=(signed)argument(63));counts();record(qualified_receiver()->second--);counts();
 const volatile struct Plain *view=&qualified;record(view->first);record(view->second);
 struct Boolean booleans={0,85};record(booleans.flag=2);record(booleans.flag);record(booleans.neighbor);
 record(qualified_receiver()->flag=argument(2));counts();record(qualified.flag);
#ifdef __cplusplus
 record((++receiver()->u)=argument(9));counts();record(packed.u);
 record((receiver()->u=argument(8191))+=argument(3));counts();record(packed.u);
 record((--receiver()->s)=-17);counts();record(packed.s);
 for(int choose=0;choose<2;++choose){record((choose?receiver()->u:receiver()->u)+=argument(1));counts();}
#endif
 /* A deterministic model independently checks sign extension and untouched
    neighbor bits across 512 different stored bit patterns. */
 unsigned long long state=0x9e3779b97f4a7c15ULL;
 for(unsigned i=0;i<512;++i){
  state^=state<<13;state^=state>>7;state^=state<<17;
  packed.u=state;packed.s=(long long)state;
  unsigned long long u=state&8191ULL;long long s=(long long)(state&2047ULL);if(s&1024)s-=2048;
  if(packed.u!=u||packed.s!=s||!neighbors())return 0;
  record(packed.u);record(packed.s);
  const volatile unsigned char *bytes=(const volatile unsigned char*)&packed;
  for(unsigned long n=0;n<sizeof packed;++n)record(bytes[n]);
 }
 return trace;
}
int volatile_bitfield_set(int value){qualified.first=value;return qualified.first;}
int volatile_bitfield_get(void){return qualified.first;}
int volatile_bitfield_tls_set(int value){tls.first=value;return tls.first;}
int volatile_bitfield_tls_get(void){return tls.first;}
#ifdef __cplusplus
}
#endif
