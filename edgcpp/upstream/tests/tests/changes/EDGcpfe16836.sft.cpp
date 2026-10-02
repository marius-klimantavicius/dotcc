//type:fp
//options_all:--c++11
//remark:[4.11] Thread-local specifiers and anonymous unions/structs
// 2/25/16  [EDGcpfe/16836]
//
// Thread-local specifiers and anonymous unions/structs
//
// A number of changes have been made to the handling of thread-local specifiers
// (i.e., _Thread_local in C11 mode, or thread_local in C++11 mode) and
// anonymous unions/structs.  These changes are bring the front end closer to
// standard compliance as well as to emulating the behavior of other compilers.
static thread_local union {
  char x;
  int y;
};
