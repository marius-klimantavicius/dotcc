//type:fp
//options_all:--c++20 -A

int a[3];
auto (*p)[3] = &a;

//cwg: 2397
//title: auto specifier for pointers and references to arrays
//meeting: Virtual 6/21
//edg_status: Passes
