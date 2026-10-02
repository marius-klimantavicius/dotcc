// Nonvirtual object layout and EH, compared with EDG's original C backend.
unsigned long long trace;
int live, fail_at;
void event(int n) { trace=trace*131+(unsigned)n; }
void reset() { trace=0; live=0; fail_at=0; }
unsigned long long read_trace() { return trace; }
int read_live() { return live; }
struct Left {
  int value; Left* self;
  Left(int n=10):value(n),self(this) { event(1); if(fail_at==1)throw 101; ++live; }
  Left(const Left& r):value(r.value),self(this) { event(2); if(fail_at==2)throw 102; ++live; }
  ~Left() { event(self==this?3:99); --live; }
};
struct Right {
  long value; Right* self;
  Right(int n=20):value(n),self(this) { event(4); if(fail_at==4)throw 104; ++live; }
  Right(const Right& r):value(r.value),self(this) { event(5); if(fail_at==5)throw 105; ++live; }
  ~Right() { event(self==this?6:99); --live; }
};
struct Derived: Left, Right {
  int own;
  Derived(int n=30):Left(n),Right(n+1),own(n+2) { event(7); if(fail_at==7)throw 107; }
  Derived(const Derived& r):Left(r),Right(r),own(r.own) { event(8); }
  ~Derived() { event(9); }
};
struct Further: Derived { int last; Further():Derived(40),last(43){} ~Further(){event(10);} };
struct Private: private Left { int own; Private():Left(50),own(51){} };
struct Protected: protected Right { Protected():Right(60){} };
struct Branch1: Left { Branch1():Left(70){} };
struct Branch2: Left { Branch2():Left(80){} };
struct Diamond: Branch1, Branch2 {};
struct PublicStep: Private {};
struct Empty {};
struct EmptyDerived: Empty { int n; };
struct EmptyLeft: Empty {};
struct EmptyRight: Empty {};
struct EmptyDiamond: EmptyLeft, EmptyRight {};
struct Tail { int n; char c; Tail(int v=1):n(v),c(2){} };
struct TailDerived: Tail { char extra; TailDerived():Tail(7),extra(9){} };
struct PlainLeft { int a; };
struct PlainRight { long b; };
struct Plain: PlainLeft, PlainRight { int c; };
struct OffsetTail: PlainRight,TailDerived { char finish; OffsetTail():PlainRight{11},finish(12){} };
Plain global_plain={{3},{4},5};
int mutate_global() { return ++global_plain.c; }
Derived make() { return Derived(90); }
void again() { throw; }
void throw_derived() { throw Derived(91); }
int catch_right(void (*hook)()) {
  try { hook(); } catch(Right& r) { return r.value==92 && r.self==&r; }
  return 0;
}
int run_case(int which,void (*hook)()) {
  reset();
  if(which==0) { Derived d; hook(); return d.Left::value+d.Right::value+d.own; }
  if(which==1) { Derived d; Left* l=&d; Right* r=&d; hook(); return l->self==l && r->self==r && (char*)r-(char*)&d==(long)sizeof(Left); }
  if(which==2) { Derived d; Right* r=&d; Derived* p=static_cast<Derived*>(r); return p==&d; }
  if(which==3) { Derived* p=nullptr; Right* r=p; return r==nullptr && static_cast<Derived*>(r)==nullptr; }
  if(which==4) { Further f; Left* l=&f; Right* r=&f; hook(); return l->value+r->value+f.last; }
  if(which==5) { Derived d; Derived copy=d; hook(); return copy.Left::self==static_cast<Left*>(&copy) && copy.Right::self==static_cast<Right*>(&copy); }
  if(which==6) { Derived d=make(); hook(); return d.own; }
  if(which==7) { Derived d[2]; hook(); return d[1].own; }
  if(which==8) { fail_at=1; try { Derived d; } catch(int n) { return n; } }
  if(which==9) { fail_at=4; try { Derived d; } catch(int n) { return n; } }
  if(which==10) { fail_at=7; try { Derived d; } catch(int n) { return n; } }
  if(which==11) { try { Derived d; throw 111; } catch(int n) { return n; } }
  if(which==12) { try { throw Derived(); } catch(Right& r) { hook(); return r.value==31 && r.self==&r; } }
  if(which==13) { try { throw Derived(); } catch(Left l) { hook(); return l.value==30 && l.self==&l; } }
  if(which==14) { try { throw Derived(); } catch(Right r) { hook(); return r.value==31 && r.self==&r; } }
  if(which==15) { try { throw Further(); } catch(Right& r) { hook(); return r.value; } }
  if(which==16) { try { throw Derived(); } catch(Left&) { return 16; } catch(Derived&) { return -1; } }
  if(which==17) { try { throw Derived(); } catch(Derived&) { return 17; } catch(Left&) { return -1; } }
  if(which==18) { try { throw Private(); } catch(Left&) { return -1; } catch(Private& p) { return p.own; } }
  if(which==19) { try { throw Protected(); } catch(Right&) { return -1; } catch(...) { return 19; } }
  if(which==20) { try { throw Diamond(); } catch(Left&) { return -1; } catch(Branch2& b) { hook(); return b.value; } }
  if(which==21) { try { throw PublicStep(); } catch(Left&) { return -1; } catch(Private& p) { return p.own; } }
  if(which==22) { try { try { throw Derived(); } catch(Right& r) { r.value=122; hook(); again(); } } catch(Derived& d) { return d.Right::value; } }
  if(which==23) { try { try { throw Derived(); } catch(Right r) { r.value=123; hook(); throw; } } catch(Derived& d) { return d.Right::value; } }
  if(which==24) { Derived d; try { throw &d; } catch(Right* r) { hook(); return r==static_cast<Right*>(&d) && r->self==r; } }
  if(which==25) { Derived d; try { throw &d; } catch(const Right* r) { return r==static_cast<Right*>(&d); } }
  if(which==26) { Derived d; const Derived* p=&d; try { throw p; } catch(Right*) { return -1; } catch(const Right* r) { return r->value; } }
  if(which==27) { try { throw (Derived*)nullptr; } catch(Right* r) { return r==nullptr; } }
  if(which==28) { Derived d; Derived* p=&d; try { throw &p; } catch(Right**) { return -1; } catch(Derived** q) { return *q==p; } }
  if(which==29) { Derived d; try { try { throw &d; } catch(Right*& r) { r=nullptr; hook(); throw; } } catch(Derived* p) { return p==&d; } }
  if(which==30) { Private p; try { throw &p; } catch(Left*) { return -1; } catch(Private* q) { return q==&p; } }
  if(which==31) { Diamond d; try { throw &d; } catch(Left*) { return -1; } catch(Branch2* b) { return b==static_cast<Branch2*>(&d); } }
  if(which==32) { Plain p={{10},{20},30}; Plain q=p; q=p; hook(); return q.a+q.b+q.c+global_plain.a+global_plain.b+global_plain.c; }
  if(which==33) { EmptyDerived d; d.n=133; Empty* p=&d; hook(); return (void*)p==(void*)&d && d.n==133; }
  if(which==34) { TailDerived d; TailDerived copy=d; copy.extra=19; static_cast<Tail&>(copy)=static_cast<Tail&>(d); hook(); return copy.n==7 && copy.c==2 && copy.extra==19; }
  if(which==35) { TailDerived d[2]; hook(); return (char*)&d[1]-(char*)&d[0]==sizeof(TailDerived) && d[1].extra==9; }
  if(which==36) { Derived d; try { fail_at=5; Derived copy=d; } catch(int n) { return n; } }
  if(which==37) { try { throw Derived(); } catch(Right& r) { hook(); throw 137; } }
  if(which==38) { Further f; try { throw &f; } catch(Right*& r) { hook(); return r->value; } }
  if(which==39) { Derived d; try { try { throw &d; } catch(Right*& r) { Right* before=r; try { throw; } catch(Left*& l) { l=nullptr; hook(); } return r==before && r->value==31; } } catch(...) { return -1; } }
  if(which==40) { EmptyDerived d; d.n=140; try { throw d; } catch(Empty&) { hook(); return 40; } }
  if(which==41) { try { throw TailDerived(); } catch(Tail& t) { hook(); return t.n==7 && t.c==2; } }
  if(which==42) { OffsetTail d; OffsetTail copy=d; copy.extra=19; copy.finish=29; static_cast<Tail&>(copy)=static_cast<Tail&>(d); hook(); return copy.n==7 && copy.extra==19 && copy.finish==29 && copy.b==11; }
  if(which==43) { try { throw OffsetTail(); } catch(Tail& t) { hook(); return t.n==7 && t.c==2; } }
  if(which==44) { OffsetTail d; try { throw &d; } catch(Tail* t) { hook(); return t==static_cast<Tail*>(&d); } }
  if(which==45) { EmptyDiamond d; try { throw &d; } catch(Empty*) { return -1; } catch(EmptyRight* p) { hook(); return p==static_cast<EmptyRight*>(&d); } }
  if(which==46) { try { throw EmptyDiamond(); } catch(Empty&) { return -1; } catch(EmptyRight&) { return 46; } }
  if(which==47) { Plain p={}; hook(); return p.a==0 && p.b==0 && p.c==0; }
  return -99;
}
int run_checked(int which,void (*hook)()) {
  try { return run_case(which,hook); } catch(int n) { return n; }
}
