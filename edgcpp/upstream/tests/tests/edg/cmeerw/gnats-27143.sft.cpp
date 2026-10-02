//type:fp
//options:--c++20 --no_il_lower --il_display
//filter:awk '/^func-scope variable@/{f=1}/^$/{if (f) print $0; f=0}f' | awk '/"marker"/{m=1}/^$/{if (m) f=1}f' | grep -E -e '^(  decl_position.seq|  name|  parent_scope|  enclosing_routine|  referenced|  needed|  is_local_to_function|type|storage_class|init_kind|is_parameter):' -e '^func-scope ' -e '^$' | sed -e 's/@[0-9a-f]*//'
#include <coroutine>

struct R
{
  struct promise_type
  {
    promise_type();
    ~promise_type();

    auto get_return_object() { return R{}; }
    auto initial_suspend() noexcept { return std::suspend_always{}; }
    auto final_suspend() noexcept { return std::suspend_always{}; }
    void unhandled_exception() {}
    void return_void() {}
  };
};

void f(int marker)
{ }

R nontmpl(int p)
{
  int i;
  co_return;
}

template<typename T>
R tmpl(T q)
{
  T j;
  co_return;
}

template R tmpl<int>(int);
