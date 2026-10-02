//type:fp
//options_all:--c++20 --no_exceptions
//remark:[6.6] Abort on coroutine with non-trivially destructible automatic variables with
// 6/22/23  [EDGcpfe/26288]
//
// Abort on coroutine with non-trivially destructible automatic variables with
// exceptions disabled
//
// With exceptions disabled, the front end previously passed an incorrect entity
// pointer to bind the object lifetimes in a coroutine function body to.  This
// could result in failed assertions, IL write-read errors, or potentially
// segfaults.
#include <coroutine>
struct my_task {
  struct promise_type {
    my_task get_return_object();
    std::suspend_never initial_suspend();
    std::suspend_never final_suspend() noexcept;
    void return_void();
    void unhandled_exception();
  };
};
my_task f() {
  struct C {
    ~C() { }
  } c;
  co_return;
}
