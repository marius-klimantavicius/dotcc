//type:fn
//options_all:--microsoft_version 1900 --set_flag coroutines -tused

struct void_task {
  struct promise_type {
    void_task get_return_object();
    bool initial_suspend() { return true; }
    bool final_suspend() noexcept { return true; }
    void return_void() {}
  };
};

struct S {};
bool await_ready(S) { return false; }
void await_suspend(S, coroutine_handle<>) {}
void await_resume(S) {}
 
S f();
task<void> g()
{
  co_await f();
  co_return 5;  //(!)
}
