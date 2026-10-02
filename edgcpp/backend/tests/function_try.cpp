// Function-try-blocks: EDG supplies the constructor/destructor wrappers and
// implicit rethrows. Handler observations never access destroyed subobjects.
unsigned long long trace;
int live, fail, handler_mode;
void (*observe)(int);
void mark(int n) { trace=trace*131+(unsigned)n; observe(n); }
void hit(int n) { mark(n); if(n==fail) throw n; }
struct Part {
  int id;
  Part(int n):id(n) {hit(n); ++live;}
  ~Part() noexcept(false) {--live; hit(id+10);}
};
struct Construct : Part {
  Part member;
  Construct() try : Part(1), member(2) {Part local(3); hit(4);}
  catch(int n) {mark(20+n); if(handler_mode==3)fail=5; Part local(5); if(handler_mode==1)throw n+100; if(handler_mode==2)throw;}
  ~Construct() noexcept(false) {mark(6);}
};
struct Destroy : Part {
  Part member;
  Destroy():Part(1),member(2) {}
  ~Destroy() noexcept(false) try {Part local(3); hit(4); if(handler_mode==3)return;}
  catch(int n) {mark(20+n); Part local(5); if(handler_mode==1)throw n+100; if(handler_mode==2)return;}
};
struct Virtual : virtual Part {
  Part member;
  Virtual() try : Part(1),member(2) {hit(4);}
  catch(...) {mark(30);}
  ~Virtual() noexcept(false) try {hit(6);} catch(...) {mark(31);}
};
struct Delegate : Part {
  Delegate(int) : Part(1) {hit(2);}
  Delegate() try : Delegate(0) {hit(4);} catch(int n) {mark(20+n);}
  ~Delegate() noexcept(false) {mark(6);}
};
int ordinary(int mode) try {
  Part first(1); hit(2); if(mode==1)throw 70; return 41;
} catch(int n) {
  Part second(3); mark(40); if(mode==2)throw; return n+100;
}
void ordinary_void() try { Part first(1); hit(2); } catch(...) {mark(40);}
struct NoexceptConstruct : Part {
  NoexceptConstruct() noexcept try : Part(1) {throw 70;} catch(...) {mark(40);}
};
struct NoexceptDestroy : Part {
  NoexceptDestroy():Part(1) {}
  ~NoexceptDestroy() noexcept try {throw 70;} catch(...) {mark(40);}
};
int run_function_try(int test,void (*hook)(int)) {
  trace=0;live=0;fail=0;handler_mode=0;observe=hook;
  int result=0;
  try {
    if(test<8) {
      const int failures[8]={0,1,2,3,4,4,4,4};fail=failures[test];
      if(test==5)handler_mode=1;if(test==6)handler_mode=2;
      if(test==7)handler_mode=3; // Exception replacing the original inside the handler.
      Construct value; result=1;
    } else if(test<18) {
      const int failures[10]={0,4,13,12,11,4,4,12,0,3};fail=failures[test-8];
      if(test==13)handler_mode=1;if(test==14 || test==15)handler_mode=2;
      if(test==16)handler_mode=3;
      Destroy value;result=2;
    } else if(test<22) {
      fail=test==18?1:test==19?2:test==20?4:6;Virtual value;result=3;
    } else if(test<25) {
      fail=test==22?1:test==23?2:4;Delegate value;result=4;
    } else if(test<29) {
      fail=test==26?2:test==28?1:0;result=ordinary(test==27?1:test==28?2:0);
    } else if(test==29) {fail=2;ordinary_void();result=5;}
    else if(test==30) {fail=4;try {Construct value;}catch(int n){mark(50);throw;} }
    else if(test==31) {fail=12;handler_mode=2;{Destroy value;}mark(50);result=6;}
  } catch(int n) {mark(60);result=n;}
  return result;
}
unsigned long long read_function_trace(){return trace;}
int read_function_live(){return live;}
void terminate_function_try(int test,void (*hook)(int)) {
  trace=0;live=0;fail=0;handler_mode=0;observe=hook;
  if(test==0) {NoexceptConstruct value;}
  if(test==1) {NoexceptDestroy value;}
  if(test==2) {try {Destroy value; fail=12;throw 70;} catch(int) {mark(99);}}
}
