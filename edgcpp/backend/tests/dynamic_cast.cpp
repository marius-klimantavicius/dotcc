unsigned long long trace;
int live;
void event(int n){trace=trace*131+(unsigned)n;}
unsigned long long read_trace(){return trace;}
int read_live(){return live;}
void reset(){trace=0;live=0;}
struct B;
int phase(B*);
struct B {
  int value;
  B(int n=10); B(const B& b):value(b.value){++live;}
  ~B(); virtual int get(){return value;}
};
struct A:B { A(int n=20):B(n){} };
struct C:B { C():B(30){} };
struct D:A,C {};
struct E:D {};
struct Wrong:B {};
struct Plain { int plain; Plain():plain(91){} };
struct Hybrid:Plain,B {};
struct L:virtual B { L();~L(); };
struct R:virtual B { R();~R(); };
struct Diamond:L,R { Diamond();~Diamond(); };
int phase(B* p){return (dynamic_cast<L*>(p)!=nullptr)+2*(dynamic_cast<R*>(p)!=nullptr)+4*(dynamic_cast<Diamond*>(p)!=nullptr);}
B::B(int n):value(n){++live;event(100+phase(this));}
B::~B(){event(200+phase(this));--live;}
L::L(){event(110+phase(this));}
L::~L(){event(210+phase(this));}
R::R(){event(120+phase(this));}
R::~R(){event(220+phase(this));}
Diamond::Diamond(){event(130+phase(this));}
Diamond::~Diamond(){event(230+phase(this));}
struct Mid1:A {};
struct Mid2:A {};
struct Repeat:Mid1,Mid2 {};
struct VMid1:L {};
struct VMid2:L {};
struct VRepeat:VMid1,VMid2 {};
struct Hidden:private A { B* base(){return this;} A* a(){return this;} };
struct Protected:protected A { B* base(){return this;} };
struct PrivateL:private virtual B { B* base(){return this;} };
struct Mixed:PrivateL,R {};
struct Nested:Diamond { int data[20]; };
struct Extra { virtual int other(){return 9;} int pad[10]; };
struct Shifted:Extra,L {};
struct CrossRepeat:Repeat,Extra {};
struct PrivateA:private A {};
struct AccessMix:PrivateA,Extra {};
struct AccessAmbiguous:PrivateA,A,Extra {};
struct PlainEmpty {};
struct WithEmpty:B,PlainEmpty {};
int cast_a(B* p){return dynamic_cast<A*>(p)!=nullptr;}
int cast_d(B* p){return dynamic_cast<D*>(p)!=nullptr;}
B* once(B* p,int* count){++*count;return p;}
int run_case(int which,void (*hook)()){
  reset();
  if(which==0){A a;B* b=&a;hook();return dynamic_cast<A*>(b)==&a;}
  if(which==1){B b;hook();return dynamic_cast<A*>(&b)==nullptr;}
  if(which==2){B* b=nullptr;hook();return dynamic_cast<A*>(b)==nullptr;}
  if(which==3){D d;B* b=static_cast<C*>(&d);hook();return dynamic_cast<D*>(b)==&d;}
  if(which==4){D d;B* b=static_cast<A*>(&d);hook();return dynamic_cast<C*>(b)==static_cast<C*>(&d);}
  if(which==5){D d;B* b=static_cast<C*>(&d);hook();return dynamic_cast<A*>(b)==static_cast<A*>(&d);}
  if(which==6){E e;B* b=static_cast<C*>(&e);hook();return dynamic_cast<E*>(b)==&e;}
  if(which==7){D d;B* b=static_cast<A*>(&d);hook();return dynamic_cast<Wrong*>(b)==nullptr;}
  if(which==8){D d;B* b=static_cast<C*>(&d);hook();return dynamic_cast<void*>(b)==&d;}
  if(which==9){D d;const B* b=static_cast<C*>(&d);hook();return dynamic_cast<const void*>(b)==&d;}
  if(which==10){Hybrid h;B* b=&h;hook();Plain* p=dynamic_cast<Plain*>(b);return p==static_cast<Plain*>(&h) && p->plain==91;}
  if(which==11){Repeat r;B* b=static_cast<Mid1*>(&r);hook();return dynamic_cast<A*>(b)==static_cast<A*>(static_cast<Mid1*>(&r));}
  if(which==12){Repeat r;B* b=static_cast<Mid2*>(&r);hook();return dynamic_cast<Mid1*>(b)==static_cast<Mid1*>(&r);}
  if(which==13){Repeat r;B* b=static_cast<Mid1*>(&r);hook();return dynamic_cast<Repeat*>(b)==&r;}
  if(which==14){Diamond d;B* b=&d;hook();return dynamic_cast<Diamond*>(b)==&d;}
  if(which==15){Diamond d;B* b=&d;hook();return dynamic_cast<L*>(b)==static_cast<L*>(&d);}
  if(which==16){Diamond d;L* l=&d;hook();return dynamic_cast<R*>(l)==static_cast<R*>(&d);}
  if(which==17){Diamond d;R* r=&d;hook();return dynamic_cast<L*>(r)==static_cast<L*>(&d);}
  if(which==18){Diamond d;B* b=&d;hook();return dynamic_cast<void*>(b)==&d;}
  if(which==19){VRepeat d;B* b=&d;hook();return dynamic_cast<L*>(b)==nullptr;}
  if(which==20){VRepeat d;B* b=&d;hook();return dynamic_cast<VMid1*>(b)==static_cast<VMid1*>(&d);}
  if(which==21){Hidden d;B* b=d.base();hook();return dynamic_cast<Hidden*>(b)==nullptr;}
  if(which==22){Hidden d;B* b=d.base();hook();return dynamic_cast<A*>(b)==d.a();}
  if(which==23){Protected d;B* b=d.base();hook();return dynamic_cast<Protected*>(b)==nullptr;}
  if(which==24){Mixed d;B* b=static_cast<R*>(&d);hook();return dynamic_cast<R*>(b)==static_cast<R*>(&d);}
  if(which==25){Mixed d;B* b=static_cast<R*>(&d);hook();return dynamic_cast<Mixed*>(b)==&d;}
  if(which==26){Mixed d;B* b=d.base();hook();return dynamic_cast<PrivateL*>(b)==static_cast<PrivateL*>(&d);}
  if(which==27){Nested d;B* b=&d;hook();return dynamic_cast<Diamond*>(b)==static_cast<Diamond*>(&d);}
  if(which==28){Shifted d;B* b=&d;hook();return dynamic_cast<L*>(b)==static_cast<L*>(&d);}
  if(which==29){Shifted d;B* b=&d;hook();return dynamic_cast<Extra*>(b)==static_cast<Extra*>(&d);}
  if(which==30){WithEmpty d;B* b=&d;hook();return dynamic_cast<PlainEmpty*>(b)==static_cast<PlainEmpty*>(&d);}
  if(which==31){D d;const B* b=static_cast<C*>(&d);hook();return dynamic_cast<const A*>(b)==static_cast<A*>(&d);}
  if(which==32){D d;B* b=static_cast<C*>(&d);int count=0;hook();return dynamic_cast<D*>(once(b,&count))==&d && count==1;}
  if(which==33){int count=0;hook();return dynamic_cast<D*>(once(nullptr,&count))==nullptr && count==1;}
  if(which==34){try{throw Diamond();}catch(B& b){hook();return dynamic_cast<Diamond*>(&b)!=nullptr && phase(&b)==7;}}
  if(which==35){Diamond d;try{hook();throw 9;}catch(int n){return phase(&d)==7 && n==9;}}
  if(which==36){struct Local:B {};Local d;B* b=&d;hook();return dynamic_cast<Local*>(b)==&d;}
  if(which==37){struct Local:B {};Local d;B* b=&d;hook();return dynamic_cast<Local*>(b)==&d;}
  if(which==38){CrossRepeat d;Extra* p=&d;hook();return dynamic_cast<A*>(p)==nullptr;}
  if(which==39){CrossRepeat d;Extra* p=&d;hook();return dynamic_cast<Mid1*>(p)==static_cast<Mid1*>(&d);}
  if(which==40){AccessMix d;Extra* p=&d;hook();return dynamic_cast<A*>(p)==nullptr;}
  if(which==41){AccessAmbiguous d;Extra* p=&d;hook();return dynamic_cast<A*>(p)==nullptr;}
  return -99;
}
// Host calls the cast through another owner while the object is alive.
int cast_void(void* p){return cast_d((B*)p);}
int cross_owner(int (*callback)(void*)){reset();D d;return callback(static_cast<C*>(&d));}
