#if defined(EDG_TERMINATE_C_ORACLE)
typedef int sig_atomic_t;
#define SIGABRT 6
#define SIG_DFL ((void (*)(int))0)
extern "C" {void (*signal(int,void (*)(int)))(int);void _Exit(int);int atoi(const char*);}
namespace std {
using terminate_handler=void(*)();
terminate_handler set_terminate(terminate_handler) noexcept;
terminate_handler get_terminate() noexcept;
[[noreturn]] void terminate() noexcept;
int uncaught_exceptions() noexcept;
}
#else
#include <signal.h>
#include <stdlib.h>
#include <exception>
#endif
static volatile sig_atomic_t owner_status=70,callback_status,inspect_context;
static std::terminate_handler saved_default;
static int custom_mode;
extern "C" void handle_abort(int sig)
{
 int status=owner_status+callback_status;
 if(sig!=SIGABRT)status=99;
#if !defined(EDG_TERMINATE_C_ORACLE)
 // Explicit synchronous profile observation, not an async signal-safe claim.
 if(inspect_context)status+=(std::current_exception() && std::uncaught_exceptions()==0)?1:20;
#endif
 _Exit(status);
}
static void wrapper()
{
 if(custom_mode==1)return;
 if(custom_mode==2)throw 9;
 if(custom_mode==3)std::terminate();
#if !defined(EDG_TERMINATE_C_ORACLE)
 if(!std::current_exception() || std::uncaught_exceptions()!=0)_Exit(98);
#endif
 callback_status=3;saved_default();
}
static void fail_noexcept() noexcept {throw 7;}
extern "C" void terminate_signal_install(int status)
{owner_status=status;callback_status=0;inspect_context=0;custom_mode=0;signal(SIGABRT,handle_abort);saved_default=std::get_terminate();}
extern "C" void terminate_signal_case(int scenario)
{
 switch(scenario) {
 case 0:std::terminate();
 case 1:saved_default();break;
 case 2:std::set_terminate(wrapper);std::set_terminate(saved_default);std::terminate();
 case 3:inspect_context=1;fail_noexcept();break;
 case 4:std::set_terminate(wrapper);fail_noexcept();break;
 case 5:case 6:case 7:custom_mode=scenario-4;std::set_terminate(wrapper);std::terminate();
 case 8:signal(SIGABRT,SIG_DFL);std::terminate();
 }
 _Exit(97);
}
