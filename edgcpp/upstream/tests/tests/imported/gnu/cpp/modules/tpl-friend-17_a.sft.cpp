//type: fp
//options:  --c++20 --modules
# 0 "./modules/tpl-friend-17_a.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./modules/tpl-friend-17_a.C"




module;
# 1 "./modules/tpl-friend-17.h" 1


template <typename> struct unique_ptr {
  template <typename> friend class out_ptr_t;
};
template <typename> struct shared_ptr {
  template <typename> friend class out_ptr_t;
};
# 7 "./modules/tpl-friend-17_a.C" 2
export module M;
unique_ptr<int> s;
export template <typename> void foo() { shared_ptr<int> u; }
