#include <coroutine>
#include <cstdlib>
static int progressed;
struct Transfer {
  bool await_ready() const noexcept {return false;}
  std::coroutine_handle<> await_suspend(std::coroutine_handle<>) const noexcept {++progressed;return std::noop_coroutine();}
  void await_resume() const noexcept {++progressed;}
};
struct Task {
  struct promise_type;
  using Handle=std::coroutine_handle<promise_type>;
  Handle handle;
  ~Task(){handle.destroy();}
  struct promise_type {
    static void* operator new(decltype(sizeof(0)) n){return std::malloc(n);}
    static void operator delete(void* p)noexcept{std::free(p);}
    Task get_return_object(){return Task{Handle::from_promise(*this)};}
    std::suspend_always initial_suspend()noexcept{return {};}
    std::suspend_always final_suspend()noexcept{return {};}
    void return_void()noexcept{++progressed;}
    void unhandled_exception()noexcept{std::abort();}
  };
};
static Task work(){co_await Transfer{};co_return;}
int main(){
  auto noop=std::noop_coroutine();
  if(!noop || noop.done() || !noop.address())return 1;
  noop.resume();noop.destroy();noop();
  std::coroutine_handle<> erased=noop;
  erased.resume();erased.destroy();erased();
  if(erased.done() || erased.address()!=noop.address())return 2;
  auto& promise=noop.promise();
  if(&promise!=&std::noop_coroutine().promise())return 3;
  auto task=work();task.handle.resume();
  if(progressed!=1 || task.handle.done())return 4;
  task.handle.resume();
  if(progressed!=3 || !task.handle.done())return 5;
  return 0;
}
