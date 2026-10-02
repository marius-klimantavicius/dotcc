#if defined(EDG_TERMINATE_C_ORACLE)
namespace std {
using terminate_handler=void(*)();
terminate_handler set_terminate(terminate_handler) noexcept;
terminate_handler get_terminate() noexcept;
[[noreturn]] void terminate() noexcept;
int uncaught_exceptions() noexcept;
}
#else
#include <exception>
#endif
static void (*observer)(int,int);
static int mode;
struct Trace {
  int value;
  explicit Trace(int v):value(v){if(observer)observer(value,std::uncaught_exceptions());}
  ~Trace(){if(observer)observer(-value,std::uncaught_exceptions());}
};
static void on_terminate() {
  if(observer)observer(900+std::uncaught_exceptions(),
#if defined(EDG_TERMINATE_C_ORACLE)
      -1 // The original generic EDG runtime has no persistent-handle provider.
#else
      !!std::current_exception()
#endif
  );
  if(mode==1)throw 404;
  if(mode==2)std::terminate();
}
static void no_throw() noexcept {Trace guard{10};throw 11;}
struct Secondary {
  ~Secondary() noexcept(false) {Trace guard{20};throw 21;}
};
struct CatchCopy {
  CatchCopy()=default;
  CatchCopy(const CatchCopy&) {Trace guard{30};throw 31;}
};
struct ThrowCopy {
  ThrowCopy()=default;
  ThrowCopy(const ThrowCopy&) {Trace guard{40};throw 41;}
};
struct ExplicitDuringUnwind {
  ~ExplicitDuringUnwind(){Trace guard{50};std::terminate();}
};
extern "C" void terminate_install(void(*report)(int,int),int handler_mode) {
  observer=report;mode=handler_mode;
  auto prior=std::set_terminate(on_terminate);
  if(std::get_terminate()!=on_terminate || !prior)report(-999,-999);
}
extern "C" void terminate_case(int which) {
  switch(which) {
    case 0:std::terminate();
    case 1:no_throw();break;
    case 2:try {Secondary guard;throw 22;}catch(...){}break;
    case 3:try {throw CatchCopy();}catch(CatchCopy){}break;
    case 4:{ThrowCopy original;try{throw original;}catch(...){}break;}
    case 5:try {ExplicitDuringUnwind guard;throw 51;}catch(...){}break;
    case 6:throw; // No active handler: language terminate, not invariant failure.
#if !defined(EDG_TERMINATE_C_ORACLE)
    case 7:std::rethrow_exception(std::exception_ptr());break;
#endif
  }
  if(observer)observer(-998,-998);
}
