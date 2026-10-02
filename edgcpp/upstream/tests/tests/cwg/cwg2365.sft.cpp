//type: fn
//options_all: -A --c++20 -tused -e 200 --no_wrap
//
struct T1 {};
struct T2 {};
T1* p = nullptr;
dynamic_cast<T2*>(p);

//cwg: 2365
//title: Confusing specification for dynamic_cast
//meeting: Kona 02/19
//edg_status: Passes
