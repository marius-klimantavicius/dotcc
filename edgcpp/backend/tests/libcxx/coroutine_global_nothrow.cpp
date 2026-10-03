#include <coroutine>
#include <new>
#include <cstdlib>
static int events[32],event_count,fail_allocation,wrong_tag;
static void note(int n){events[event_count++]=n;}
// Complete replacement allocation family needed by the pinned library helpers.
// The coroutine itself selects the nothrow overload below.
void* operator new(decltype(sizeof(0)) size){
  void* p=std::malloc(size?size:1);if(!p)throw std::bad_alloc();return p;
}
void* operator new(decltype(sizeof(0)) size,std::align_val_t alignment){
  auto a=static_cast<decltype(sizeof(0))>(alignment);if(!size)size=1;
  if(size>static_cast<decltype(sizeof(0))>(-1)-(a-1))throw std::bad_alloc();
  void* p=std::aligned_alloc(a,(size+a-1)&~(a-1));if(!p)throw std::bad_alloc();return p;
}
void operator delete(void* p,std::align_val_t)noexcept{std::free(p);}
void operator delete(void* p,decltype(sizeof(0)),std::align_val_t)noexcept{std::free(p);}
void* operator new(decltype(sizeof(0)) size,const std::nothrow_t& tag)noexcept{
#ifndef EDG_TEST_UNUSED_NOTHROW
  if(&tag!=&std::nothrow)wrong_tag=1;
#else
  (void)tag;
#endif
  note(1);return fail_allocation?nullptr:std::malloc(size);
}
void operator delete(void* p)noexcept{note(-1);std::free(p);}
void operator delete(void* p,decltype(sizeof(0)))noexcept{note(-1);std::free(p);}
struct Task {
  struct promise_type;using Handle=std::coroutine_handle<promise_type>;Handle handle;
  explicit Task(Handle h):handle(h){}Task(const Task&)=delete;
  ~Task(){if(handle)handle.destroy();}
  struct promise_type {
    static Task get_return_object_on_allocation_failure()noexcept{note(9);return Task(nullptr);}
    promise_type(){note(2);}~promise_type(){note(-2);}
    Task get_return_object(){note(3);return Task(Handle::from_promise(*this));}
    std::suspend_always initial_suspend()noexcept{return {};}
    std::suspend_always final_suspend()noexcept{return {};}
    void return_void()noexcept{note(5);}
    void unhandled_exception()noexcept{note(99);}
  };
};
static Task work(){note(4);co_return;}
static Task(*volatile factory)()=&work;
int main(){
  event_count=0;fail_allocation=1;
  {Task task=factory();if(task.handle)return 1;}
  if(wrong_tag || event_count!=2 || events[0]!=1 || events[1]!=9)return 2;
  event_count=0;fail_allocation=0;
  {Task task=factory();task.handle.resume();if(!task.handle.done())return 3;}
  const int expected[]={1,2,3,4,5,-2,-1};
  if(wrong_tag || event_count!=7)return 4;
  for(int i=0;i<7;++i)if(events[i]!=expected[i])return 5+i;
  return 0;
}
