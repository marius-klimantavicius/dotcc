//type:rp
//options:--c++20 --no_exceptions --set_flag=no_checking_pragmas

#include <coroutine>

struct R
{
  struct promise_type
  {
    R get_return_object() { return { }; }
    auto initial_suspend() noexcept { return std::suspend_never{}; }
    auto final_suspend() noexcept { return std::suspend_never{}; }
    void unhandled_exception() noexcept { }
    void return_void() noexcept { }
  };
};

extern "C" int printf(const char *, ...);

// dummy co_await implementation
extern "C"
void co_await_ZN1R12promise_type15initial_suspendEv(const void *)
{ }
extern "C"
void co_await_ZN1R12promise_type13final_suspendEv(const void *)
{ }

struct C
{
  int m;

  C(int i)
    : m(i)
  {
    printf("C::C %d\n", m);
  }

  ~C()
  {
    printf("C::~C %d\n", m);
  }
};

R coro(int i)
{
  C c1(1);
  if (i == 0)
  {
    co_return;
  }

  {
    C c2(2);
    if (i == 1)
    {
      co_return;
    }
  }

  C c3(3);
  co_return;
}

int main()
{
  for (int i = 0; i < 3; ++i)
  {
    printf("coro(%d)\n", i);
    coro(i);
  }
}
