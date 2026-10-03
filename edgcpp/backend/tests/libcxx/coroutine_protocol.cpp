#include <coroutine>
#include <cstdlib>
#include <cstdio>
static int mode;
static void note(int value) {
  if(value<0){std::putchar('-');value=-value;}
  if(value>=100)std::putchar('0'+value/100);
  if(value>=10)std::putchar('0'+value/10%10);
  std::putchar('0'+value%10);std::putchar(',');
}
struct Await {
  int code;
  bool await_ready() const {
    note(code);
    if(code==10 && mode==1)throw 71;
    return code==20 && mode==4;
  }
  bool await_suspend(std::coroutine_handle<>) const {
    note(code+1);
    if(code==10 && mode==2)throw 72;
    if(code==20 && mode==5)throw 75;
    return !(code==20 && mode==3);
  }
  void await_resume() const {
    note(code+2);
    if(code==10 && mode==6)throw 76;
    if(code==20 && mode==7)throw 77;
  }
};
struct Final {
  bool await_ready() const noexcept {note(40);return false;}
  void await_suspend(std::coroutine_handle<>) const noexcept {note(41);}
  void await_resume() const noexcept {note(42);}
};
struct Guard {Guard(){note(100);}~Guard(){note(-100);}};
struct Task {
  struct promise_type;
  using Handle=std::coroutine_handle<promise_type>;
  Handle handle;
  explicit Task(Handle value):handle(value){note(3);}
  Task(const Task&)=delete;
  ~Task(){note(-3);if(handle)handle.destroy();}
  struct promise_type {
    static void* operator new(decltype(sizeof(0)) bytes){note(1);return std::malloc(bytes);}
    static void operator delete(void* p,decltype(sizeof(0)))noexcept{note(-1);std::free(p);}
    promise_type(){note(2);}
    ~promise_type(){note(-2);}
    Task get_return_object(){return Task(Handle::from_promise(*this));}
    Await initial_suspend(){return {10};}
    Final final_suspend()noexcept{return {};}
    void return_void()noexcept{note(30);}
    void unhandled_exception(){note(99);if(mode==8)throw 78;}
  };
};
static Task work(){Guard guard;co_await Await{20};if(mode==8)throw 80;co_return;}
int main(int argc,char**argv){
  mode=argc>1?argv[1][0]-'0':0;
  try {
    auto task=work();note(4);
    if(mode!=9){task.handle.resume();note(5);if(!task.handle.done()){task.handle.resume();note(6);}}
  }catch(int value){note(value);}
  std::puts("");return 0;
}
