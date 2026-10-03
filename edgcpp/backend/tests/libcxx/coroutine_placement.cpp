#include <coroutine>
#include <cstdlib>
static int events[64],event_count,fail_allocation,alternate=37;
static void note(int n){events[event_count++]=n;}
struct Task {
  struct promise_type;using Handle=std::coroutine_handle<promise_type>;Handle handle;
  explicit Task(Handle h):handle(h){} Task(const Task&)=delete;
  ~Task(){if(handle)handle.destroy();}
  struct promise_type {
    static void* operator new(decltype(sizeof(0)) size,int& value,int& reference,int*& pointer){
      note(1);value+=10;reference+=20;pointer=&alternate;
      note(value);note(reference);if(fail_allocation)throw 71;
      return std::malloc(size);
    }
    static void operator delete(void* p,decltype(sizeof(0)))noexcept{note(-1);std::free(p);}
    promise_type(int& value,int& reference,int*& pointer){note(2);note(value);note(reference);note(*pointer);}
    ~promise_type(){note(-2);}
    Task get_return_object(){note(3);return Task(Handle::from_promise(*this));}
    std::suspend_always initial_suspend()noexcept{return {};}
    std::suspend_always final_suspend()noexcept{return {};}
    void return_void()noexcept{note(5);}
    void unhandled_exception()noexcept{note(99);}
  };
};
static Task work(int& value,int& reference,int*& pointer){note(4);note(value);note(reference);note(*pointer);co_return;}
static Task(*volatile factory)(int&,int&,int*&)=&work;
int main(){
  int value=3,reference=7,original=11;int* pointer=&original;
  {Task task=factory(value,reference,pointer);if(reference!=27 || pointer!=&alternate || value!=13)return 1;task.handle.resume();if(!task.handle.done())return 2;}
  const int normal[]={1,13,27,2,13,27,37,3,4,13,27,37,5,-2,-1};
  if(event_count!=15)return 3;
  for(int i=0;i<15;++i)if(events[i]!=normal[i])return 10+i;
  event_count=0;fail_allocation=1;value=3;reference=7;bool caught=false;
  try{Task task=factory(value,reference,pointer);(void)task;}catch(int n){caught=n==71;}
  if(!caught || reference!=27 || event_count!=3 || events[0]!=1 || events[1]!=13 || events[2]!=27)return 40;
  return 0;
}
