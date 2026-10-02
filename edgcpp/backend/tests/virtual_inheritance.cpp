unsigned long long trace;
int live, fail_at;
void event(int n) { trace=trace*131+(unsigned)n; }
void operator delete(void*) noexcept {event(202);}
void operator delete(void* p,unsigned long) noexcept {operator delete(p);}
void reset() { trace=0;live=0;fail_at=0; }
unsigned long long read_trace() { return trace; }
int read_live() { return live; }
struct Root {
  int value;
  Root(int n=10):value(n){event(get());++live;}
  Root(const Root& r):value(r.value){event(get());++live;}
  virtual ~Root(){event(get());--live;}
  virtual int get(){return value;}
  virtual Root* self(){return this;}
};
struct Left: virtual Root {
  int left;
  Left(int n=20):left(n){event(get());++live;if(fail_at==1){--live;throw 101;}}
  Left(const Left& r):Root(r),left(r.left){event(get());++live;}
  ~Left(){event(get());--live;}
  int get() override { return value+left; }
};
struct Right: virtual Root {
  int right;
  Right(int n=30):right(n){event(get());++live;if(fail_at==2){--live;throw 102;}}
  Right(const Right& r):Root(r),right(r.right){event(get());++live;}
  ~Right(){event(get());--live;}
  int get() override { return value+right; }
};
struct Diamond: Left,Right {
  int last;
  Diamond(int n=40):Root(n),Left(n+1),Right(n+2),last(n+3){event(get());if(fail_at==3)throw 103;}
  ~Diamond(){event(get());}
  int get() override { return value+left+right+last; }
  Diamond* self() override { return value<0?nullptr:this; }
};
struct Hidden:private virtual Root { Hidden():Root(80){} int get() override{return value+1;} };
struct PrivatePath:private virtual Root {};
struct MixedPath:PrivatePath,Right { MixedPath():Root(60),Right(31){} };
struct Side:Root { Side():Root(90){} };
struct Ambiguous:Diamond,Side {};
struct ThrowOnly:virtual Root { ThrowOnly():Root(90){event(91);} ~ThrowOnly(){event(92);} int get() override{return value+1;} };
int invoke(Root* p) { return p->get(); }
Root* identity(Root* p) { return p->self(); }
void again(){throw;}
void throw_diamond(){throw Diamond(70);}
int catch_root(void (*hook)()) { try {hook();}catch(Root& r){return r.get()==286;}return 0; }
int run_case(int which,void (*hook)()) {
  reset();
  if(which==0){Diamond d;hook();return invoke(&d);}
  if(which==1){Diamond d;Left* l=&d;Right* r=&d;hook();return static_cast<Root*>(l)==static_cast<Root*>(r);}
  if(which==2){Diamond d;hook();return identity(&d)==static_cast<Root*>(&d);}
  if(which==3){Diamond d(-10);hook();return identity(&d)==nullptr;}
  if(which==4){Diamond d;Diamond copy=d;hook();return invoke(&copy);}
  if(which==5){Diamond d[2];hook();return invoke(&d[1]);}
  if(which==6){fail_at=1;try{Diamond d;}catch(int n){return n;}}
  if(which==7){fail_at=2;try{Diamond d;}catch(int n){return n;}}
  if(which==8){fail_at=3;try{Diamond d;}catch(int n){return n;}}
  if(which==9){try{Diamond d;hook();throw 109;}catch(int n){return n;}}
  if(which==10){try{throw Diamond();}catch(Root& r){hook();return r.get();}}
  if(which==11){try{throw Diamond();}catch(Root r){hook();return r.get();}}
  if(which==12){try{try{throw Diamond();}catch(Root& r){r.value=50;hook();again();}}catch(Diamond& d){return invoke(&d);}}
  if(which==13){Diamond d;try{throw &d;}catch(Root* r){hook();return r==static_cast<Root*>(&d) && r->get()==166;}}
  if(which==14){try{throw (Diamond*)nullptr;}catch(Root* r){return r==nullptr;}}
  if(which==15){Left d;hook();return invoke(&d);}
  if(which==16){try{throw Diamond();}catch(Left l){hook();return l.get();}}
  if(which==17){Left d;Left copy=d;hook();return invoke(&copy);}
  if(which==18){Diamond d;const Diamond* p=&d;try{throw p;}catch(Root*){return -1;}catch(const Root* r){hook();return r->value;}}
  if(which==19){try{throw Hidden();}catch(Root&){return -1;}catch(Hidden& h){hook();return h.get();}}
  if(which==20){try{throw MixedPath();}catch(Root& r){hook();return r.get();}}
  if(which==21){try{throw Ambiguous();}catch(Root&){return -1;}catch(Side& r){hook();return r.get();}}
  if(which==22){try{throw ThrowOnly();}catch(Root& r){hook();return r.get();}}
  if(which==23){Diamond d;try{try{throw &d;}catch(Root*& r){r=nullptr;hook();again();}}catch(Diamond* p){return p==&d;}}
  return -99;
}
int run_checked(int which,void (*hook)()){try{return run_case(which,hook);}catch(int n){return n;}}
