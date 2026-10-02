//type: fp
//options:  --c++20 --modules
# 0 "./modules/pr114630_a.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./modules/pr114630_a.C"



module;
# 1 "./modules/pr114630.h" 1
template <typename _CharT>
void _M_do_parse() {
  struct A {};
  struct B {};
  int x;
}

template <typename> struct formatter;
template <> struct formatter<int> {
  void parse() { _M_do_parse<int>(); }
};
# 6 "./modules/pr114630_a.C" 2
export module X;
formatter<int> a;
