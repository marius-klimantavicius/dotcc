#include <exception>
#include <setjmp.h>
static void* target;
static int handlers,entered;
static void forbidden_handler(){++handlers;}
static void setup(){handlers=entered=0;std::set_terminate(forbidden_handler);std::set_unexpected(forbidden_handler);}
static int recovered(int site,int which) {
  bool okay=site==which+1 && !handlers;
#if !defined(EDG_JUMP_C_ORACLE)
  okay=okay && !std::uncaught_exception();
#if defined(EDG_JUMP_NATIVE_ORACLE)
  // Retained native ABI observation: raw longjmp bypasses end-catch bookkeeping
  // for case 3. The managed runtime's explicit handler lifetime contract below
  // still requires releasing its catch frame; this is not a changed EDG rule.
  okay=okay && (!!std::current_exception()==(which==3));
#else
  okay=okay && !std::current_exception();
#endif
#endif
  if(which==3)okay=okay && entered==1;
  target=nullptr;return okay;
}
static void through_catch_all(){try{longjmp(*(jmp_buf*)target,1);}catch(...){++handlers;}}
static void through_noexcept() noexcept{longjmp(*(jmp_buf*)target,2);}
static void through_specification() throw(int){longjmp(*(jmp_buf*)target,3);}
static void from_scalar_handler(){try{throw 17;}catch(int n){if(n==17)++entered;longjmp(*(jmp_buf*)target,4);}}
extern "C" void jump_other_owner(void* environment){longjmp(*(jmp_buf*)environment,5);}
extern "C" int run_nonlocal_cpp(int which,void(*bridge)(void*)) {
  jmp_buf environment;setup();target=&environment;
  switch(setjmp(environment)) {
    case 0:break;
    case 1:return recovered(1,which);
    case 2:return recovered(2,which);
    case 3:return recovered(3,which);
    case 4:return recovered(4,which);
    case 5:return recovered(5,which);
    default:return 0;
  }
  if(which==0)through_catch_all();
  if(which==1)through_noexcept();
  if(which==2)through_specification();
  if(which==3)from_scalar_handler();
  if(which==4)bridge(environment);
  return 0;
}
