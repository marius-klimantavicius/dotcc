//type:fp
//options_all:--c++20 -tused -A
     int f() noexcept { return 0; }
     int (*fp)() noexcept(false) = f;
     int i = fp();  // Call f via a different function type

//cwg: 2486
//title: Call to noexcept function via noexcept(false) pointer/lvalue
//meeting: Virtual 10/21
//edg_status: Passes
