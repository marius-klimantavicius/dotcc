unsigned long long trace;
int live, fail_at;
int deletes;
void* last_deleted;
alignas(64) unsigned char arena[512];
void event(int n) { trace=trace*131+(unsigned)n; }
void reset() { trace=0;live=0;fail_at=0;deletes=0;last_deleted=nullptr; }
unsigned long long read_trace() { return trace; }
int read_live() { return live; }
// Deliberately supplied by the program: no native allocator or libc binding.
void* operator new(unsigned long n) { if(n>512)throw 201;event(201);return arena; }
void operator delete(void* p) noexcept { ++deletes;last_deleted=p;event(202); }
void operator delete(void* p,unsigned long) noexcept { operator delete(p); }
struct Base {
  int value;
  Base(int n):value(n) { event(get(0)); ++live; if(fail_at==1){--live;throw 101;} }
  Base(const Base& r):value(r.value) { ++live; event(get(1)); }
  ~Base() { event(get(2)); --live; }
  virtual int get(int n) { return value+n; }
  virtual Base* self() { return this; }
};
struct Other {
  int other;
  Other(int n):other(n){event(get(0));++live;}
  Other(const Other& r):other(r.other){event(get(1));++live;}
  ~Other(){event(get(2));--live;}
  virtual int get(int n) { return other+100+n; }
  virtual Other* self() { return this; }
};
struct Derived: Base {
  int extra;
  Derived(int n):Base(n),extra(n+1){event(get(0));if(fail_at==2)throw 102;}
  ~Derived(){event(get(3));}
  int get(int n) override { return value+extra+n; }
  Derived* self() override { return value<0?nullptr:this; }
};
struct Multi: Base,Other {
  int last;
  Multi(int n):Base(n),Other(n+1),last(n+2){event(get(0));if(fail_at==3)throw 103;}
  ~Multi(){event(get(3));}
  int get(int n) override { return value+other+last+n; }
  Multi* self() override { return value<0?nullptr:this; }
};
struct Further: Multi {
  Further(int n):Multi(n){event(get(0));}
  ~Further(){event(get(3));}
  int get(int n) override { return value+500+n; }
};
struct Abstract { virtual int read(int n)=0; ~Abstract(){event(90);} };
struct Concrete: Abstract { int n; Concrete(int v):n(v){} int read(int x) override {return n+x;} };
struct PureAbort { virtual void call()=0; ~PureAbort(); };
void invoke_abort(PureAbort* p){p->call();}
PureAbort::~PureAbort(){invoke_abort(this);}
struct ConcreteAbort:PureAbort { void call() override {} };
void terminate_case(){ConcreteAbort value;}
struct VBase { int v; VBase(int n):v(n){++live;event(41);} virtual ~VBase() noexcept(false){--live;event(42);} virtual int get(){return v;} };
struct VRight { int r; VRight(int n):r(n){++live;event(43);} virtual ~VRight() noexcept(false){--live;event(44);} virtual int get(){return r;} };
struct VDerived:VBase,VRight {
  VDerived():VBase(11),VRight(12){event(45);if(fail_at==4)throw 104;}
  ~VDerived() noexcept(false) override {event(46);if(fail_at==5)throw 105;}
  int get() override{return v+r;}
};
int invoke(Base* p,int n) { return p->get(n); }
int invoke_other(Other* p,int n) { return p->get(n); }
Other* identity_other(Other* p) { return p->self(); }
Base* identity_base(Base* p) { return p->self(); }
int same_token(Base* a,Base* b) { return (*(long**)a)[-1]==(*(long**)b)[-1] && (*(long**)a)[-1]!=0; }
unsigned long long type_token(){Derived d(10);return (unsigned long long)(*(long**)&d)[-1];}
void again() { throw; }
void throw_multi() { throw Multi(70); }
int catch_other(void (*hook)()) {
  try { hook(); } catch(Other& r) { return r.get(0)==213; }
  return 0;
}
int run_case(int which,void (*hook)()) {
  reset();
  if(which==0) { Base b(10); hook(); return invoke(&b,2); }
  if(which==1) { Derived d(10); hook(); return invoke(&d,2); }
  if(which==2) { Multi d(10); hook(); return invoke(&d,2)+invoke_other(&d,3); }
  if(which==3) { Further d(10); hook(); return invoke(&d,2)+invoke_other(&d,3); }
  if(which==4) { Multi d(10); hook(); return identity_other(&d)==static_cast<Other*>(&d); }
  if(which==5) { Multi d(10); hook(); return identity_base(&d)==static_cast<Base*>(&d); }
  if(which==6) { Multi d(-10); hook(); return identity_other(&d)==nullptr && identity_base(&d)==nullptr; }
  if(which==7) { Derived d(10); Base b=d; hook(); return invoke(&b,2); }
  if(which==8) { Multi d(10); Multi copy=d; hook(); return invoke_other(&copy,2); }
  if(which==9) { Multi d[2]={Multi(10),Multi(20)}; hook(); return invoke_other(&d[1],2); }
  if(which==10) { try { Multi d(10); hook(); throw 110; } catch(int n) { return n; } }
  if(which==11) { fail_at=1; try { Multi d(10); } catch(int n){return n;} }
  if(which==12) { fail_at=2; try { Derived d(10); } catch(int n){return n;} }
  if(which==13) { fail_at=3; try { Multi d(10); } catch(int n){return n;} }
  if(which==14) { try { throw Multi(10); } catch(Other& r) { hook(); return r.get(2); } }
  if(which==15) { try { throw Multi(10); } catch(Other r) { hook(); return r.get(2); } }
  if(which==16) { try { try { throw Further(10); } catch(Other& r) { hook(); r.other=77; again(); } } catch(Further& d) { return d.other+invoke(&d,1); } }
  if(which==17) { Multi d(10); try { throw &d; } catch(Other* r) { hook(); return r->get(2); } }
  if(which==18) { struct Local:Base { Local():Base(10){} int get(int n) override {return value+700+n;} }; Local d; hook(); return invoke(&d,2); }
  if(which==19) { struct Local:Base { Local():Base(10){} int get(int n) override {return value+800+n;} }; Local d; hook(); return invoke(&d,2); }
  if(which==20) { Concrete d(50); Abstract* p=&d; hook(); return p->read(7); }
  if(which==21) { Concrete d[2]={Concrete(50),Concrete(60)}; Abstract* p=&d[1]; hook(); return p->read(7); }
  if(which==22) { Derived a(10),b(20); hook(); return same_token(&a,&b); }
  if(which==23) { Derived a(10); Multi b(20); hook(); return !same_token(&a,&b); }
  if(which==24) { VDerived d; VRight* p=&d; hook(); return p->get(); }
  if(which==25) { VBase* p=new VDerived(); hook(); delete p; return deletes==1 && last_deleted==arena; }
  if(which==26) { VRight* p=new VDerived(); hook(); delete p; return deletes==1 && last_deleted==arena; }
  if(which==27) { try {throw VDerived();}catch(VRight& r){hook();return r.get();} }
  if(which==28) { fail_at=4;try { VBase* p=new VDerived();delete p; }catch(int n){return n+1000*deletes;} }
  if(which==29) { VBase* p=new VBase(9);hook();delete p;return deletes==1 && last_deleted==arena; }
  if(which==30) { VRight* p=new VDerived();fail_at=5;try {hook();delete p;}catch(int n){return n+1000*deletes;} }
  if(which==31) { VDerived d[2]; VBase* p=&d[1];hook();return p->get(); }
  return -99;
}
int run_checked(int which,void (*hook)()) {
  try { return run_case(which,hook); } catch(int n) { return n; }
}
