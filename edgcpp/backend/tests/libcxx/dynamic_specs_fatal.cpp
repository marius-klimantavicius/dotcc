#include <exception>
static void (*observer)(int,int);
static int scenario;
static bool armed;
static void report(int event){observer(event,std::uncaught_exception());}
struct Scope {
  int value;
  explicit Scope(int n):value(n){report(n);}
  ~Scope(){report(-value);}
};
struct Original {
  ~Original() noexcept(false) {
    if(armed && scenario==3){report(31);Scope guard(32);throw 33;}
  }
};
struct Replacement {
  ~Replacement() noexcept(false) {
    if(armed && scenario==4){report(41);Scope guard(42);throw 43;}
  }
};
static void unexpected_handler() {
  report(100);armed=true;
  if(scenario==1)return;
  if(scenario==2)throw 2.5;
  if(scenario==4)throw Replacement();
  throw 7;
}
static void terminate_handler() {
  report(900);
#if !defined(EDG_DYNAMIC_C_ORACLE)
  observer(901,!!std::current_exception());
#endif
}
static void empty_spec() throw(){Scope guard(10);throw 1;}
static void int_spec() throw(int){Scope guard(20);throw 2.5;}
static void original_cleanup() throw(int){Scope guard(30);throw Original();}
static void replacement_cleanup() throw(std::bad_exception){Scope guard(40);throw 4;}
extern "C" void dynamic_fatal_install(void(*callback)(int,int),int which) {
  observer=callback;scenario=which;armed=false;
  std::set_unexpected(unexpected_handler);std::set_terminate(terminate_handler);
}
extern "C" void dynamic_fatal_case() {
  try {
    switch(scenario) {
      case 0:empty_spec();break;
      case 1:case 2:int_spec();break;
      case 3:original_cleanup();break;
      case 4:replacement_cleanup();break;
    }
  }catch(...){report(-999);}
  report(-998);
}
