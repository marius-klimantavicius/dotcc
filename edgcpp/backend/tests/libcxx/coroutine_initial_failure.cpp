#include <coroutine>
#include <cstdlib>
static int mode,events[32],count;
static void note(int n){events[count++]=n;}
struct Initial {
  bool await_ready(){note(10);if(mode==1)throw 71;return false;}
  void await_suspend(std::coroutine_handle<>){note(11);if(mode==2)throw 72;}
  void await_resume()noexcept{note(12);}
};
struct Task {
  struct promise_type;
  using Handle=std::coroutine_handle<promise_type>;
  Handle handle;
  struct promise_type {
    static void* operator new(decltype(sizeof(0)) n){note(1);if(mode==5)throw 75;return std::malloc(n);}
    static void operator delete(void*p)noexcept{note(-1);std::free(p);}
    promise_type(){note(2);if(mode==3)throw 73;}
    ~promise_type(){note(-2);}
    Task get_return_object(){note(3);if(mode==4)throw 74;return {Handle::from_promise(*this)};}
    Initial initial_suspend(){return {};}
    std::suspend_always final_suspend()noexcept{return {};}
    void return_void()noexcept{}
    void unhandled_exception()noexcept{note(99);}
  };
};
static Task work(){co_return;}
static int test(int value,const int*expected,int n){
  mode=value;count=0;try{auto task=work();task.handle.destroy();return 80;}catch(int error){note(error);}
  if(count!=n)return 81;
  for(int i=0;i<count;++i)if(events[i]!=expected[i])return 90+i;
  return 0;
}
int main(){
  const int a[]={1,2,3,10,-2,-1,71},b[]={1,2,3,10,11,-2,-1,72},c[]={1,2,-1,73},d[]={1,2,3,-2,-1,74},e[]={1,75};
  int r=test(1,a,7);if(r)return r;r=test(2,b,8);if(r)return r;r=test(3,c,4);if(r)return r;r=test(4,d,6);if(r)return r;return test(5,e,2);
}
