#include <coroutine>
#include <cstdlib>
#include <cstdio>
static int events[128],count,alive,result,failid;
static void note(int id){events[count++]=id;}
struct Life {int id; Life(int n):id(n){++alive;note(n);} ~Life(){note(-id);--alive;}};
struct Value {
  int value,id;
  Value(int v,int n):value(v),id(n){note(n+4);if(failid==n)throw n;}
  bool await_ready() const noexcept {note(id);return false;}
  void await_suspend(std::coroutine_handle<>)const noexcept{note(id+1);}
  int await_resume()const noexcept{note(id+2);return value;}
  ~Value(){note(-id);}
};
struct Reference {
  int* value;int id;
  Reference(int*p,int n):value(p),id(n){note(n+4);if(failid==n)throw n;}
  bool await_ready()const noexcept{note(id);return false;}
  void await_suspend(std::coroutine_handle<>)const noexcept{note(id+1);}
  int* await_resume()const noexcept{note(id+2);return value;}
  ~Reference(){note(-id);}
};
struct Pair {int items[2];};
struct PairAwaiter {
  PairAwaiter(){note(34);if(failid==30)throw 30;}
  bool await_ready()const noexcept{note(30);return false;}
  void await_suspend(std::coroutine_handle<>)const noexcept{note(31);}
  Pair await_resume()const noexcept{note(32);return {{17,29}};}
  ~PairAwaiter(){note(-30);}
};
struct Task {
  struct promise_type;using Handle=std::coroutine_handle<promise_type>;Handle handle;
  ~Task(){handle.destroy();}
  struct promise_type {
    static void* operator new(decltype(sizeof(0)) n){return std::malloc(n);}
    static void operator delete(void*p)noexcept{std::free(p);}
    Task get_return_object(){return {Handle::from_promise(*this)};}
    std::suspend_always initial_suspend()noexcept{return {};}
    std::suspend_always final_suspend()noexcept{return {};}
    void return_void()noexcept{}
    void unhandled_exception()noexcept{result=-99;}
  };
};
static Task work(){
  Life local(100);int target=7;
  *(co_await Reference{&target,20}) += co_await Value{5,10};
  int chosen=(co_await PairAwaiter{}).items[co_await Value{1,40}];
  result=target+chosen;
  co_return;
}
static void number(int value){if(value<0){std::putchar('-');value=-value;}if(value>9)number(value/10);std::putchar('0'+value%10);}
int main(int argc,char**argv){
  int limit=argc>1?argv[1][0]-'0':99;
  if(argc>1 && argv[1][0]>='a'){failid=(argv[1][0]-'a'+1)*10;limit=99;}
  {auto task=work();for(int i=0;i<limit && !task.handle.done();++i)task.handle.resume();}
  if(alive)return 1;
  if(limit>=5 && result!=(failid?-99:41))return 2;
  for(int i=0;i<count;++i){number(events[i]);std::putchar(',');}number(result);std::putchar('\n');
  return 0;
}
