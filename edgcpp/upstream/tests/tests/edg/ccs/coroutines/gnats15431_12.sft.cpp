//type:fp
//options_all:--microsoft_version 1900 --set_flag coroutines -tused

#include <coroutine>
using namespace std;
 
struct S {};
bool await_ready(S) { return false; }
void await_suspend(S, coroutine_handle<>) {}
void await_resume(S) {}
 
template<class T> T f(T);
template<class T> auto g(T p)
{
  try {}
  catch(...) {
    __await f(p); //(!)
  }
}
