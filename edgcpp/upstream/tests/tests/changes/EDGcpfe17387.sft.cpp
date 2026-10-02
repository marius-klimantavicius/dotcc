//type:fp
//options_all:--gnu_version 60100
//remark:[6.1] __is_pod and trivial default constructors
// 3/3/20   [EDGcpfe/17387,EDGcpfe/21970,EDGcpfe/22117,EDGcpfe/22118]
//
// __is_pod and trivial default constructors
//
// Some tweaks were made to the processes determining whether a defaulted default
// constructor is trivial or not.  These changes affect the outcome of the
// __is_pod type trait helper, sometimes for better conformance to the standard,
// and sometimes for better compatibility with GCC and Clang.
struct S1 {
  int i;
  S1() = delete;
};
static_assert(__is_pod(S1), "");  // Previously an error in all modes.
                                  // Now accepted in some GCC modes.
