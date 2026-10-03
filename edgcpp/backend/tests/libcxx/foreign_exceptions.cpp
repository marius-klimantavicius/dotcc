#include <exception>
static void(*observer)(int,int);
static void(*foreign_hook)();
static int unexpected_mode,bad;
static void report(int value){observer(value,10*std::uncaught_exception()+!!std::current_exception());}
static void check(bool condition){if(!condition)++bad;}
struct Scope {
  int value;
  explicit Scope(int n):value(n){report(n);}
  ~Scope(){report(-value);}
};
static void terminate_handler(){report(900);}
static void unexpected_handler(){report(100);if(unexpected_mode)foreign_hook();throw 7;}
extern "C" void foreign_install(void(*callback)(int,int),void(*hook)()) {
  observer=callback;foreign_hook=hook;bad=unexpected_mode=0;
  std::set_terminate(terminate_handler);std::set_unexpected(unexpected_handler);
}
extern "C" int foreign_state(){return 10*std::uncaught_exception()+!!std::current_exception();}
extern "C" void foreign_escape() {
  Scope guard(11);
  try{foreign_hook();}catch(int){report(-999);}catch(...){report(101);throw;}
}
extern "C" int foreign_nested() {
  try{throw 19;}catch(int value){
    check(value==19 && !!std::current_exception());
    auto enclosing=std::current_exception();
    try{foreign_hook();}catch(int){++bad;}catch(...){
      check(!std::current_exception() && !std::uncaught_exception());
      std::nested_exception captured;check(!captured.nested_ptr());
      try{throw;}catch(...){check(!std::current_exception());}
      check(!std::current_exception());
    }
    check(std::current_exception()==enclosing && !std::uncaught_exception());
  }
  check(!std::current_exception() && !std::uncaught_exception());return bad;
}
static void dynamic_allowed() throw(int){Scope guard(12);foreign_hook();}
static void dynamic_bad() throw(std::bad_exception){Scope guard(13);throw 8;}
extern "C" int foreign_replacement(int mode) {
  unexpected_mode=mode;
  if(!mode){try{dynamic_allowed();}catch(int n){check(n==7);report(200);}}
  else {try{dynamic_bad();}catch(const std::bad_exception&){report(201);}}
  check(!std::current_exception() && !std::uncaught_exception());return bad;
}
static void no_throw() noexcept{Scope guard(20);foreign_hook();}
struct Secondary {
  ~Secondary() noexcept(false){Scope guard(30);foreign_hook();}
};
extern "C" void foreign_fatal(int which) {
  if(which==0)no_throw();
  if(which==1){try{Secondary guard;throw 31;}catch(...){report(-998);}}
  if(which==2){try{foreign_hook();}catch(...){report(40);std::nested_exception captured;captured.rethrow_nested();}}
  if(which==3){unexpected_mode=1;dynamic_allowed();}
  report(-997);
}
