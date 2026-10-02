#include <exception>
static int bad,handlers,destructions,mode;
static void check(bool okay){if(!okay)++bad;}
struct Guard {~Guard(){check(std::uncaught_exception()==1);++destructions;}};
struct Base {int value=31;};
struct Derived:Base {int extra=32;};
struct Member {int value;};
static int object=19;
static void on_unexpected() {
  ++handlers;check(std::uncaught_exception()==0);
#if !defined(EDG_DYNAMIC_C_ORACLE)
  check(!!std::current_exception());
#endif
  if(mode==0)throw 7;
  if(mode==1)throw 4.5;
  if(mode==2)throw;
  if(mode==3)return;
  if(mode==4)throw Derived();
  if(mode==5)throw &object;
  if(mode==6)throw &Member::value;
}
static void allowed() throw(int){Guard guard;throw 5;}
static void reference() throw(int&){Guard guard;throw 6;}
static void replace_int() throw(int){Guard guard;throw 2.5;}
static void replace_bad() throw(std::bad_exception){Guard guard;throw 2;}
static void replace_bad_base() throw(std::exception){Guard guard;throw 2;}
static void replace_base() throw(Base){Guard guard;throw 2;}
static void replace_pointer() throw(const void*){Guard guard;throw 2;}
static void replace_member() throw(int Member::*){Guard guard;throw 2;}
static void nested_inner() throw(int){Guard guard;throw 3;}
static void nested_outer() throw(double){Guard guard;nested_inner();}
static void allowed_derived() throw(Base){Guard guard;throw Derived();}
using Array=int[2];
using Function=void();
static void function_target() {}
static void adjusted_array() throw(Array) {Guard guard;throw &object;}
static void adjusted_function() throw(Function) {Guard guard;throw &function_target;}
extern "C" int dynamic_spec_cases() {
  bad=handlers=destructions=mode=0;
  auto old=std::set_unexpected(on_unexpected);check(old!=nullptr);
#if !defined(EDG_DYNAMIC_C_ORACLE)
  check(std::get_unexpected()==on_unexpected);
#endif
  try{allowed();}catch(int n){check(n==5);}check(handlers==0 && destructions==1);
  try{reference();}catch(int n){check(n==6);}check(handlers==0 && destructions==2);
  try{replace_int();}catch(int n){check(n==7);}check(handlers==1 && destructions==3);
  mode=1;try{replace_bad();}catch(const std::bad_exception&){check(std::uncaught_exception()==0);}check(handlers==2);
  try{replace_bad_base();}catch(const std::bad_exception&){check(std::uncaught_exception()==0);}check(handlers==3);
  mode=2;try{replace_bad();}catch(const std::bad_exception&){check(std::uncaught_exception()==0);}check(handlers==4);
  mode=4;try{replace_base();}catch(const Derived& d){check(d.value==31 && d.extra==32);}check(handlers==5);
  mode=5;try{replace_pointer();}catch(int* p){check(p==&object);}check(handlers==6);
  mode=6;try{replace_member();}catch(int Member::* p){check(p==&Member::value);}check(handlers==7);
  mode=1;try{nested_outer();}catch(double n){check(n==4.5);}check(handlers==8);
  try{allowed_derived();}catch(const Derived& d){check(d.value==31);}check(handlers==8);
  try{adjusted_array();}catch(int* p){check(p==&object);}check(handlers==8);
  try{adjusted_function();}catch(void(*p)()){check(p==function_target);}check(handlers==8);
  check(destructions==14 && std::uncaught_exception()==0);
  std::set_unexpected(old);return bad;
}
int main(){return dynamic_spec_cases();}
