#include <coroutine>
#include <cstdlib>

static int events[128], event_count;
static void note(int value) { if(event_count<128)events[event_count++]=value; }
struct Await {
  int code;
  bool await_ready() const noexcept { note(code);return false; }
  void await_suspend(std::coroutine_handle<>) const noexcept { note(code+1); }
  void await_resume() const noexcept { note(code+2); }
};
struct Guard {
  Guard() { note(100); }
  ~Guard() { note(-100); }
};
struct Task {
  struct promise_type;
  using Handle=std::coroutine_handle<promise_type>;
  Handle handle;
  explicit Task(Handle h):handle(h) {}
  Task(const Task&)=delete;
  ~Task() { if(handle)handle.destroy(); }
  struct promise_type {
    int value;
    static void* operator new(decltype(sizeof(0)) bytes) { note(1);return std::malloc(bytes); }
    static void operator delete(void* pointer,decltype(sizeof(0))) noexcept { note(-1);std::free(pointer); }
    promise_type():value(-1) { note(2); }
    ~promise_type() { note(-2); }
    Task get_return_object() { note(3);return Task(Handle::from_promise(*this)); }
    Await initial_suspend() noexcept { return {10}; }
    Await final_suspend() noexcept { return {40}; }
    Await yield_value(int next) noexcept { value=next;return {20}; }
    void return_void() noexcept { note(30); }
    void unhandled_exception() noexcept { note(99); }
  };
};
static Task count(int n) {
  Guard guard;
  for(int i=0;i<n;++i)co_yield i;
  co_return;
}
// An unused coroutine must not become a backend reachability root. This value
// expression requires the later continuation-expression slice if ever called.
struct UnusedValueAwait {
  bool await_ready() const noexcept { return false; }
  void await_suspend(std::coroutine_handle<>) const noexcept {}
  int await_resume() const noexcept { return 1; }
};
[[maybe_unused]] static Task unused_value_coroutine() {
  int value=co_await UnusedValueAwait{};
  if(value)co_return;
}
// Prevent permitted factory allocation elision in the allocation-trace oracle.
static Task (*volatile count_entry)(int)=&count;
int main() {
  {
    auto task=count_entry(2);
    if(task.handle.done())return 1;
    task.handle.resume();if(task.handle.done() || task.handle.promise().value!=0)return 2;
    task.handle.resume();if(task.handle.done() || task.handle.promise().value!=1)return 3;
    task.handle.resume();if(!task.handle.done())return 4;
  }
  const int expected[]={1,2,3,10,11,12,100,20,21,22,20,21,22,30,-100,40,41,-2,-1};
  if(event_count!=int(sizeof(expected)/sizeof(*expected)))return 5;
  for(int i=0;i<event_count;++i)if(events[i]!=expected[i])return 10+i;
  return 0;
}
