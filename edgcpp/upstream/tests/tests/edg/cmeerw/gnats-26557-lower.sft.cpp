//type:fp
//options:--c++20:--ms_c++20 --microsoft_version 1938

#include <coroutine>

namespace minimal
{
  //#include <coroutine>
  template<typename T>
  std::task<void> f(T) {
    co_return;
  }
}

struct C
{
  ~C();
};

template<typename T>
void tmpl_fn()
{
  C c;
  goto done;
done:
  return;
}

void non_tmpl_fn()
{
  C c;
  goto done;
done:
  return;
}

template<typename T>
std::task<int> non_dpdt_params()
{
  C c;
  co_return 1;
}

template<typename T>
std::task<int> dpdt_params(T)
{
  C c;
  co_return 1;
}

std::task<int> non_tmpl()
{
  C c;
  co_return 1;
}
