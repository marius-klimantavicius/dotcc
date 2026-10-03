#include <coroutine>
#include <cstdlib>
static int events[16],event_count;
static void note(int n){events[event_count++]=n;}
struct Token{int value;operator int()const{note(6);return value;}};
struct Task {
  struct promise_type;using Handle=std::coroutine_handle<promise_type>;Handle handle;
  explicit Task(Handle h):handle(h){}Task(const Task&)=delete;
  ~Task(){if(handle)handle.destroy();}
  struct promise_type {
    static void* operator new(decltype(sizeof(0)) size,int value){note(1);note(value);return std::malloc(size);}
    static void operator delete(void*p)noexcept{note(-1);std::free(p);}
    promise_type(){note(2);}~promise_type(){note(-2);}
    Task get_return_object(){note(3);return Task(Handle::from_promise(*this));}
    std::suspend_always initial_suspend()noexcept{return {};}
    std::suspend_always final_suspend()noexcept{return {};}
    void return_void()noexcept{}
    void unhandled_exception()noexcept{note(99);}
  };
};
static Task work(Token token){(void)token;co_return;}
static Task(*volatile factory)(Token)=&work;
int main(){
  {Task task=factory(Token{7});}
  const int expected[]={6,1,7,2,3,-2,-1};
  if(event_count!=7)return 1;
  for(int i=0;i<7;++i)if(events[i]!=expected[i])return 2+i;
  return 0;
}
