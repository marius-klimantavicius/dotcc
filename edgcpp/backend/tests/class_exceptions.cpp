// Exact class exception ownership, compared to the unmodified EDG C backend.
unsigned long long trace;
int live, bad, alignment_bad, fail_at, remaining;
void (*observer)(int);
void mark(int n) { trace=trace*10+n; }
void hit(int n) { mark(n); observer(n); if(n==fail_at && --remaining==0)throw n; }
unsigned long long read_trace(){return trace;}
int read_live(){return live;}
int read_bad(){return bad;}
int read_alignment_bad(){return alignment_bad;}
void reset(int fail,int occurrence,void (*hook)(int)){trace=0;live=0;bad=0;alignment_bad=0;fail_at=fail;remaining=occurrence;observer=hook;}
void again(){throw;}
struct Packet {
  long long value;
  Packet* self;
  Packet(long long n):value(n),self(this){hit(1);++live;}
  Packet(const Packet& p):value(p.value),self(this){if(p.self!=&p)++bad;hit(2);++live;}
  Packet(Packet&& p):value(p.value),self(this){if(p.self!=&p)++bad;hit(3);++live;}
  ~Packet() noexcept(false){if(self!=this)++bad;--live;hit(4);}
};
struct Other {long long value; Other* self;};
struct Empty {Empty(){hit(5);++live;} ~Empty() noexcept(false){--live;hit(6);}};
struct Plain {int x; int y;};
struct alignas(64) Aligned {
  Aligned* self;
  int n;
  Aligned(int v):self(this),n(v){hit(5);if(((unsigned long long)this & 63)!=0)++alignment_bad;++live;}
  ~Aligned(){if(self!=this)++bad;if(((unsigned long long)this & 63)!=0)++alignment_bad;--live;hit(6);}
};
struct Members {Empty a; Packet b; Members():b(9){hit(7);} ~Members() noexcept(false){hit(8);}};
struct CopyFail {
  Empty member;
  CopyFail(){}
  CopyFail(const CopyFail&):member(){try{Empty local;hit(2);}catch(long){}}
  ~CopyFail() noexcept(false){hit(8);}
};
struct HandledCopy {
  Empty member;
  HandledCopy(){}
  HandledCopy(const HandledCopy&){try{throw 6;}catch(int){mark(7);}}
  ~HandledCopy() noexcept(false){mark(8);}
};
union Value {int n; long long wide;};
Packet result(){Packet p(25);return p;}
void raise(){throw Packet(17);}
int scenario(int test){
  if(test==0){try{throw Packet(7);}catch(Packet& p){observer(9);return (int)p.value;}}
  if(test==1){try{Packet p(7);throw p;}catch(Packet& p){return (int)p.value;}}
  if(test==2){try{throw Packet(7);}catch(Packet p){observer(9);return (int)p.value;}}
  if(test==3){try{try{throw Packet(7);}catch(Packet p){p.value=19;again();}}catch(Packet& p){return (int)p.value;}}
  if(test==4){Packet* original=0;try{try{throw Packet(7);}catch(Packet& p){original=&p;++p.value;again();}}catch(Packet& p){if(&p!=original)++bad;return (int)p.value;}}
  if(test==5){try{throw Packet(7);}catch(Packet& p){try{again();}catch(Packet& q){if(&q!=&p)++bad;++q.value;}observer(9);return (int)p.value;}}
  if(test==6){try{try{throw Packet(7);}catch(Packet& p){throw Packet(p.value+2);}}catch(Packet& p){return (int)p.value;}}
  if(test==7){try{throw Packet(7);}catch(Packet& p){throw (int)p.value;}}
  if(test==8){try{try{throw Packet(7);}catch(...){again();}}catch(Packet& p){return (int)p.value;}}
  if(test==9){try{throw Packet(7);}catch(Other&){return 90;}catch(Packet& p){return (int)p.value;}}
  if(test==10){try{throw Packet(7);}catch(const Packet& p){observer(9);if(p.self!=&p)++bad;return (int)p.value;}}
  if(test==11){try{raise();}catch(Packet& p){return (int)p.value;}}
  if(test==12){try{throw Packet(7);}catch(...){observer(9);return 12;}}
  if(test==13){try{throw Empty();}catch(Empty&){return 13;}}
  if(test==14){try{throw Plain{14,3};}catch(Plain p){return p.x+p.y;}}
  if(test==15){try{throw Aligned(15);}catch(Aligned& p){observer(9);if(p.self!=&p)++bad;return p.n;}}
  if(test==16){try{throw Members();}catch(Members& p){return (int)p.b.value;}}
  if(test==17){try{throw Packet(7);}catch(Packet& p){return (int)p.value;}}
  if(test==18){try{throw Packet(7);}catch(Packet p){return (int)p.value;}}
  if(test==19){try{throw Packet(7);}catch(Packet&){goto out;}out:mark(9);return 19;}
  if(test==20){int sum=0;for(int i=0;i<3;++i){try{throw Packet(i);}catch(Packet& p){sum+=(int)p.value;}}return sum;}
  if(test==21){try{(test?throw Packet(21):throw Packet(90));}catch(Packet& p){return (int)p.value;}}
  if(test==22){try{throw result();}catch(Packet& p){return (int)p.value;}}
  if(test==23){try{Packet p(23);throw static_cast<Packet&&>(p);}catch(Packet& p){return (int)p.value;}}
  if(test==24){struct Local{int n;};try{throw Local{24};}catch(Local p){return p.n;}}
  if(test==25){try{Value v;v.wide=25;throw v;}catch(Value v){return (int)v.wide;}}
  if(test==26){try{throw Packet(26);}catch(Packet& p){try{throw Empty();}catch(Empty&){mark(9);}return (int)p.value;}}
  if(test==27){try{throw Packet(27);}catch(Packet& p){try{again();}catch(Packet q){q.value=90;}return (int)p.value;}}
  if(test==28){try{throw Packet(28);}catch(Packet& p){try{again();}catch(Packet& q){try{again();}catch(Packet& r){if(&r!=&p || &q!=&p)++bad;}}return (int)p.value;}}
  if(test==29){try{throw HandledCopy();}catch(HandledCopy p){return 29;}}
  if(test==30){try{const Packet p(30);throw p;}catch(Packet& p){return (int)p.value;}}
  if(test==31){struct A{int n;};typedef A First;try{{struct A{int n;};throw A{31};}}catch(First&){return 90;}catch(...){return 31;}}
  return 89;
}
int run_case(int test,int fail,int occurrence,void (*hook)(int)){
  reset(fail,occurrence,hook);
  try{return scenario(test);}catch(int n){return -n;}
}
// Cross-owner: the exception destructor must use the throwing owner's globals.
void export_raise(){throw Packet(31);}
int export_catch(void (*hook)()) {try{hook();}catch(Packet& p){observer(9);return (int)p.value;}return 90;}
int through_foreign(void (*hook)()) {try{throw Packet(32);}catch(Packet&){hook();}return 91;}
void terminate_case(int which,void (*hook)(int)){
  if(which==0){run_case(2,2,1,hook);} // Copying the catch parameter.
  if(which==1){run_case(23,3,1,hook);} // Moving into the throw payload.
  if(which==2){run_case(7,4,1,hook);} // Payload destruction during another throw.
  if(which==3){run_case(18,4,1,hook);} // Normal catch-parameter destructor can throw: this one is caught.
  if(which==4){reset(0,0,hook);try{throw Packet(1);}catch(Packet&){try{Packet p(2);fail_at=4;remaining=1;throw 42;}catch(long){}}}
  if(which==5){reset(2,1,hook);try{throw CopyFail();}catch(CopyFail p){}}
  if(which==6){reset(2,1,hook);try{const CopyFail p;throw p;}catch(CopyFail&){}}
}
