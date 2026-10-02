#include <typeinfo>
#include "dynamic_cast.cpp"
enum Colour { red,blue };
union Choice { int n;long m; };
struct Incomplete;
using Function=int(int);
const std::type_info* saved_type=&typeid(int);
const std::type_info& info(int n) {
  if(n==0)return typeid(int);
  if(n==1)return typeid(const int&);
  if(n==2)return typeid(long);
  if(n==3)return typeid(void);
  if(n==4)return typeid(bool);
  if(n==5)return typeid(double);
  if(n==6)return typeid(decltype(nullptr));
  if(n==7)return typeid(int*);
  if(n==8)return typeid(const int*);
  if(n==9)return typeid(volatile int*);
  if(n==10)return typeid(B);
  if(n==11)return typeid(D);
  if(n==12)return typeid(Diamond);
  if(n==13)return typeid(Colour);
  if(n==14)return typeid(Choice);
  if(n==15)return typeid(int[3]);
  if(n==16)return typeid(Function);
  if(n==17)return typeid(Function*);
  if(n==18)return typeid(int B::*);
  if(n==19)return typeid(Incomplete*);
  if(n==20)return typeid(char);
  if(n==21)return typeid(unsigned long long);
  if(n==22){struct Local {};return typeid(Local);}
  {struct Local {};return typeid(Local);}
}
const char* type_name(int n){return info(n).name();}
const void* type_address(int n){return &info(n);}
void again_typeid(){throw;}
void fail_typeid(){B* p=nullptr;(void)typeid(*p);}
struct GuardTypeid { B first; GuardTypeid():first(99){fail_typeid();} };
struct PhaseBase;
int type_phase(PhaseBase*);
struct PhaseBase {
  PhaseBase(){event(type_phase(this));}
  virtual ~PhaseBase(){event(type_phase(this));}
};
struct PhaseDerived:PhaseBase {
  PhaseDerived(){event(type_phase(this));}
  ~PhaseDerived(){event(type_phase(this));}
};
int type_phase(PhaseBase* p){return typeid(*p)==typeid(PhaseDerived)?2:1;}
int run_type_case(int which,void (*hook)()) {
  reset();
  if(which==0){hook();return typeid(int)==typeid(const int&) && &typeid(int)==&typeid(const int);}
  if(which==1){return typeid(int)!=typeid(long) && !(typeid(int)==typeid(long));}
  if(which==2){A a;B* b=&a;hook();return typeid(*b)==typeid(A) && &typeid(*b)==&typeid(A);}
  if(which==3){D d;B* b=(C*)&d;hook();return typeid(*b)==typeid(D);}
  if(which==4){Diamond d;B* b=&d;hook();return typeid(*b)==typeid(Diamond);}
  if(which==5){int count=0;Plain* p=nullptr;return typeid(*(++count,p))==typeid(Plain) && count==0;}
  if(which==6){A a;int count=0;return typeid(*once(&a,&count))==typeid(A) && count==1;}
  if(which==7){int count=0;try{(void)typeid(*once(nullptr,&count));}catch(std::bad_typeid& e){hook();return count==1 && e.what()[0]==0 && typeid(e)==typeid(std::bad_typeid);}}
  if(which==8){try{fail_typeid();}catch(std::exception& e){hook();return typeid(e)==typeid(std::bad_typeid) && dynamic_cast<std::bad_typeid*>(&e)!=nullptr;}}
  if(which==9){try{fail_typeid();}catch(std::bad_typeid e){hook();return e.what()[0]==0;}}
  if(which==10){try{fail_typeid();}catch(std::exception e){hook();return typeid(e)==typeid(std::exception);}}
  if(which==11){try{try{fail_typeid();}catch(...){hook();again_typeid();}}catch(std::bad_typeid&){return 1;}}
  if(which==12){void* seen=nullptr;try{try{fail_typeid();}catch(std::bad_typeid& e){seen=&e;hook();throw;}}catch(std::bad_typeid& e){return seen==&e;}}
  if(which==13){try{try{throw 11;}catch(int){try{fail_typeid();}catch(std::bad_typeid&){hook();}again_typeid();}}catch(int n){return n==11;}}
  if(which==14){const A a;const B* p=&a;hook();return typeid(*p)==typeid(A);}
  if(which==15){D d;B* b=(C*)&d;return typeid(*dynamic_cast<A*>(b))==typeid(D);}
  // The original EDG C backend resolves these explicit-cast operands to L/R
  // descriptors. Keep its observed result; the pointer-variable case is below.
  if(which==16){Diamond d;return &typeid(*(L*)&d)==&typeid(*(R*)&d);}
  if(which==17){PhaseDerived d;hook();return type_phase(&d)==2;}
  if(which==18){hook();return saved_type==&typeid(int) && *saved_type==typeid(int);}
  if(which==19){std::bad_typeid a,b;b=a;hook();return b.what()[0]==0 && typeid(b)==typeid(std::bad_typeid);}
  if(which==20){for(int i=0;i<24;++i)for(int j=0;j<24;++j){const auto& a=info(i);const auto& b=info(j);if(a.before(b)!=(a.name()<b.name()) || (a==b)!=(i==j || (i<2 && j<2)) || a.before(a) || (a.before(b) && b.before(a)))return 0;}return 1;}
  if(which==21){try{fail_typeid();}catch(std::bad_typeid&){try{hook();}catch(std::bad_typeid&){return 1;}}}
  if(which==22){try{GuardTypeid g;}catch(std::bad_typeid&){hook();return live==0;}}
  if(which==23){A a;const auto& t=typeid(a);const char* n=t.name();hook();return &t==&typeid(A) && n==typeid(A).name();}
  if(which==24){int n=0;return typeid(++n)==typeid(int) && n==0;}
  if(which==25){int a[3];return typeid(a)==typeid(int[3]) && typeid(a)!=typeid(int*);}
  if(which==26){return typeid(const int*)!=typeid(int*) && typeid(int* const)==typeid(int*);}
  if(which==27){return typeid(Colour)!=typeid(int) && typeid(Choice)!=typeid(long);}
  if(which==28){return typeid(Function)!=typeid(Function*) && typeid(Function)==typeid(int(int));}
  if(which==29){return typeid(int B::*)!=typeid(int*) && typeid(Incomplete*)!=typeid(void*);}
  if(which==30){try{throw std::bad_typeid();}catch(std::exception& e){hook();return typeid(e)==typeid(std::bad_typeid);}}
  if(which==31){try{fail_typeid();}catch(std::bad_typeid&){B guard(88);hook();return 1;}}
  if(which==32){Diamond d;L* l=&d;R* r=&d;hook();return &typeid(*l)==&typeid(*r);}
  // EDG discards the metadata lvalue but retains the evaluated pointer/null check.
  if(which==33){A a;int count=0;(void)typeid(*once(&a,&count));(void)typeid(int);hook();return count==1;}
  return 0;
}
void fail_typeid_noexcept() noexcept {B guard(77);fail_typeid();}
