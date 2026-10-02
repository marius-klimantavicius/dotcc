//type:fp
//options:--gn 140100 --target linux_x86_64:--gn 140100 --target linux_i686:--gn 140100 --target linux_aarch64:--gn 140100 --target linux_armv7:--gn 90500 --target linux_x86_64;fn:--gn 90500 --target linux_i686;fn:--gn 90500 --target linux_aarch64;fn:--gn 90500 --target linux_armv7;fn
//options_all:--c++

void f()
{
  __builtin_coro_resume(0);
  __builtin_coro_done(0);
  __builtin_coro_destroy(0);
  __builtin_coro_promise(0, 0, false);
}
