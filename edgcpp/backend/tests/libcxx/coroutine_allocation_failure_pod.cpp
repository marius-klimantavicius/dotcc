#include <coroutine>
#include <cstdlib>
static int events[32],count,fail_allocation,fail_hook;
static void note(int n){events[count++]=n;}
struct Task {
  struct promise_type;using Handle=std::coroutine_handle<promise_type>;Handle handle;
  struct promise_type {
    static void* operator new(decltype(sizeof(0)) n)noexcept{note(1);return fail_allocation?nullptr:std::malloc(n);}
    static void operator delete(void*p)noexcept{note(-1);std::free(p);}
    static Task get_return_object_on_allocation_failure(){note(8);note(9);if(fail_hook)throw 77;return Task{nullptr};}
    promise_type(){note(2);}~promise_type(){note(-2);}
    Task get_return_object(){note(3);return Task{Handle::from_promise(*this)};}
    std::suspend_always initial_suspend()noexcept{return {};}
    std::suspend_always final_suspend()noexcept{return {};}
    void return_void()noexcept{note(5);}
    void unhandled_exception()noexcept{note(99);}
  };
};
static Task work(){note(4);co_return;}
int main(){
  Task(*volatile factory)()=&work;
  fail_allocation=1;
  {Task task=factory();if(task.handle)return 1;}
  if(count!=3 || events[0]!=1 || events[1]!=8 || events[2]!=9)return 2;
  count=0;fail_hook=1;bool caught=false;
  try{Task task=factory();(void)task;}catch(int n){caught=n==77;}
  if(!caught || count!=3 || events[0]!=1 || events[1]!=8 || events[2]!=9)return 20;
  fail_hook=0;
  count=0;fail_allocation=0;
  {Task task=factory();if(!task.handle)return 3;task.handle.resume();if(!task.handle.done())return 4;task.handle.destroy();}
  const int expected[]={1,2,3,4,5,-2,-1};
  if(count!=7)return 5;
  for(int i=0;i<7;++i)if(events[i]!=expected[i])return 6+i;
  return 0;
}
