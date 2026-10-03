#include <coroutine>
#include <cstdlib>
static int result,events[64],event_count;
static volatile int seed=3;
static void note(int n){events[event_count++]=n;}
struct Value {
  int value,id;
  bool await_ready()const noexcept{note(id);return false;}
  void await_suspend(std::coroutine_handle<>)const noexcept{note(id+1);}
  int await_resume()const noexcept{note(id+2);if(id==130)seed=99;return value;}
};
struct Task {
  struct promise_type;
  using Handle=std::coroutine_handle<promise_type>;
  Handle handle;
  ~Task(){handle.destroy();}
  struct promise_type {
    static void* operator new(decltype(sizeof(0)) n){return std::malloc(n);}
    static void operator delete(void*p)noexcept{std::free(p);}
    Task get_return_object(){return {Handle::from_promise(*this)};}
    std::suspend_always initial_suspend()noexcept{return {};}
    std::suspend_always final_suspend()noexcept{return {};}
    void return_void()noexcept{}
    void unhandled_exception()noexcept{result=-1;}
  };
};
static int* destination(int*pointer){note(100);return pointer;}
static int read_seed(){note(129);return seed;}
static Task work(){
  int value=5+(co_await Value{7,10});
  value+=co_await Value{3,20};
  int target=0;
  *destination(&target)=co_await Value{9,30};
  int selected=(co_await Value{1,40})?(co_await Value{11,50}):(co_await Value{99,60});
  bool never=(co_await Value{0,70}) && (co_await Value{99,80});
  bool always=(co_await Value{1,110}) || (co_await Value{99,120});
  int shifted=read_seed() << (co_await Value{1,130});
  int loop=0;
  while(co_await Value{loop<2,90})++loop;
  result=value+target+selected+loop+never+always+shifted;
  co_return;
}
int main(){
  {auto task=work();while(!task.handle.done())task.handle.resume();}
  const int expected[]={10,11,12,20,21,22,30,31,32,100,40,41,42,50,51,52,70,71,72,110,111,112,129,130,131,132,90,91,92,90,91,92,90,91,92};
  if(result!=44 || event_count!=int(sizeof(expected)/sizeof(*expected)))return 1;
  for(int i=0;i<event_count;++i)if(events[i]!=expected[i])return 2+i;
  return 0;
}
