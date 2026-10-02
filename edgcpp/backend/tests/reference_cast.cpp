#include <typeinfo>
#include <new>
#include "dynamic_cast.cpp"
void again(){throw;}
D& require_d(B& b){return dynamic_cast<D&>(b);}
void fail_reference(){B b;require_d(b);}
struct Failing { B first;D& next;Failing(B& b):first(99),next(require_d(b)){} };
int run_ref_case(int which,void (*hook)()) {
  reset();
  if(which==0){A a;B& b=a;hook();return &dynamic_cast<A&>(b)==&a;}
  if(which==1){B b;try{require_d(b);}catch(std::bad_cast& e){hook();return e.what()[0]==0 && dynamic_cast<std::bad_cast*>((std::exception*)&e)==&e;}}
  if(which==2){B b;try{require_d(b);}catch(std::exception& e){hook();return e.what()[0]==0 && dynamic_cast<std::bad_cast*>(&e)!=nullptr;}}
  if(which==3){B b;try{require_d(b);}catch(std::bad_cast e){hook();return e.what()[0]==0;}}
  if(which==4){B b;try{require_d(b);}catch(std::exception e){hook();return dynamic_cast<std::bad_cast*>(&e)==nullptr && e.what()[0]==0;}}
  if(which==5){try{try{fail_reference();}catch(...){hook();throw;}}catch(std::bad_cast&){return 1;}}
  if(which==6){try{try{fail_reference();}catch(std::exception&){hook();again();}}catch(std::bad_cast&){return 1;}}
  if(which==7){void* seen=nullptr;try{try{fail_reference();}catch(std::bad_cast& e){seen=&e;hook();again();}}catch(std::bad_cast& e){return seen==&e;}}
  if(which==8){try{try{throw 11;}catch(int){try{fail_reference();}catch(std::bad_cast&){hook();}again();}}catch(int n){return n==11;}}
  if(which==9){try{try{fail_reference();}catch(std::bad_cast&){B guard(88);hook();throw 17;}}catch(int n){return n==17;}}
  if(which==10){D d;B& b=*(C*)&d;hook();return &dynamic_cast<D&>(b)==&d;}
  if(which==11){D d;B& b=*(A*)&d;hook();return &dynamic_cast<C&>(b)==(C*)&d;}
  if(which==12){Diamond d;B& b=d;hook();return &dynamic_cast<Diamond&>(b)==&d;}
  if(which==13){L l;B& b=l;try{dynamic_cast<R&>(b);}catch(std::bad_cast&){hook();return 1;}}
  if(which==14){B b;try{Failing f(b);}catch(std::bad_cast&){hook();return live==1;}}
  if(which==15){try{throw std::bad_cast();}catch(std::exception& e){hook();return dynamic_cast<std::bad_cast*>(&e)!=nullptr;}}
  if(which==16){A a;int count=0;hook();return &dynamic_cast<A&>(*once(&a,&count))==&a && count==1;}
  if(which==17){B b;int count=0;try{dynamic_cast<A&>(*once(&b,&count));}catch(std::bad_cast&){hook();return count==1;}}
  if(which==18){try{operator new(~0UL);}catch(std::bad_alloc& e){hook();return e.what()[0]==0;}}
  if(which==19){std::exception* e=new std::bad_cast();hook();int good=e->what()[0]==0 && dynamic_cast<std::bad_cast*>(e)!=nullptr;delete e;return good;}
  if(which==20){void* a=operator new(0);void* b=operator new(0);hook();int good=a && b && a!=b && (((unsigned long)a| (unsigned long)b)&15)==0;operator delete(a);operator delete(b);return good;}
  if(which==21){B b;try{new Failing(b);}catch(std::bad_cast&){hook();return live==1;}}
  if(which==22){try{try{fail_reference();}catch(std::bad_cast&){hook();again();}}catch(std::bad_cast&){return 1;}}
  if(which==23){A a;const B& b=a;hook();return &dynamic_cast<const A&>(b)==&a;}
  return -99;
}
int invoke_handler(void (*hook)()) {try{hook();}catch(std::bad_cast& e){return e.what()[0]==0 && live==0;}return 0;}
void fail_noexcept() noexcept {fail_reference();}
struct DoubleThrow {~DoubleThrow() noexcept(false){fail_reference();}};
void terminate_case(int n){if(n==0)fail_noexcept();else{DoubleThrow d;throw 5;}}
