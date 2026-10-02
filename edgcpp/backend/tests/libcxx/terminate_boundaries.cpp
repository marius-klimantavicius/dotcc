#include <exception>
#include <cstdio>
#include <cstdlib>
#include <__edg_thread_abi.h>
// Synthetic allocation adapter for this EH component fixture.
void* operator new[](std::size_t n) {return ::operator new(n);}
void operator delete[](void* p) noexcept {::operator delete(p);}
void operator delete[](void* p,std::size_t n) noexcept {::operator delete(p,n);}
static int scenario;
static char* decimal(char* output,int value) {
  if(value<0){*output++='-';value=-value;}
  char digits[12];int count=0;
  do {digits[count++]=char('0'+value%10);value/=10;}while(value);
  while(count)*output++=digits[--count];return output;
}
static void report(int event,int value) {
  char text[32];char* end=decimal(text,event);*end++='/';end=decimal(end,value);*end=0;
  std::puts(text);std::fflush(stdout);
}
struct Trace {
  int event;
  explicit Trace(int n):event(n){report(event,std::uncaught_exceptions());}
  ~Trace(){report(-event,std::uncaught_exceptions());}
};
static void terminate_handler(){report(900+std::uncaught_exceptions(),!!std::current_exception());}
struct Setup {
  Setup() {
    std::set_terminate(terminate_handler);
#if defined(EDG_TEST_GLOBAL_FAILURE)
    scenario=4;
#endif
    if(scenario==4){Trace trace{90};throw 91;}
  }
  ~Setup() noexcept(false) {if(scenario==2){Trace trace{80};throw 81;}}
} setup;
static void* thread_body(void*) {Trace trace{70};throw 71;}
static int next_element;
struct Element {
  int id=++next_element;
  ~Element() noexcept(false) {Trace trace{100+id};throw id;}
};
int main(int argc,char** argv) {
  if(argc>1)scenario=argv[1][0]-'0';
  switch(scenario) {
    case 0:{Trace trace{60};throw 61;}
    case 1:{unsigned long thread=0;if(__edg_libcpp_thread_create(&thread,thread_body,nullptr))return 1;__edg_libcpp_thread_join(&thread);break;}
    case 2:return 0;
    case 3:{Element* elements=new Element[2];try{delete[] elements;}catch(...){}break;}
  }
  report(-998,-998);return 2;
}
