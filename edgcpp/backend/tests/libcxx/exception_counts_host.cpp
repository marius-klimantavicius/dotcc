#include <exception>
extern "C" int counts_now() {return std::uncaught_exceptions();}
extern "C" int handled_now() {return !!std::current_exception();}
struct HostGuard {
  void (*report)(int,int);
  ~HostGuard() {report(std::uncaught_exceptions(),!!std::current_exception());}
};
extern "C" void counts_escape(void(*report)(int,int)) {
  HostGuard guard{report};throw 101;
}
extern "C" int counts_callback(void(*callback)(),void(*report)(int,int)) {
  try {HostGuard guard{report};callback();}
  catch(int x) {return x==101 && std::uncaught_exceptions()==0 && std::current_exception()?0:1;}
  catch(...) {return std::uncaught_exceptions()==0 && !std::current_exception()?0:2;}
  return 3;
}
extern "C" int counts_nested_callback(void(*callback)(),void(*report)(int,int)) {
  try {throw 202;}catch(int) {
    if(std::uncaught_exceptions()!=0 || !std::current_exception())return 1;
    try {HostGuard guard{report};callback();}
    catch(int x) {if(x!=101 || std::uncaught_exceptions()!=0 || !std::current_exception())return 2;}
    if(std::uncaught_exceptions()!=0 || !std::current_exception())return 3;
  }
  return std::uncaught_exceptions()==0 && !std::current_exception()?0:4;
}
