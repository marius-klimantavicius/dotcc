//type: fp
//options: --c++11
# 0 "./ipa/pr106258.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./ipa/pr106258.C"




# 1 "./ipa/ipa-sra-4.C" 1



void __throw_bad_alloc() __attribute__((__noreturn__));
void __throw_bad_array_new_length();
template <typename> class allocator {};
template <typename> struct allocator_traits;
int *allocate___trans_tmp_2;
template <typename _Tp> struct allocator_traits<allocator<_Tp>> {
  using allocator_type = allocator<_Tp>;
  using pointer = _Tp *;
  using size_type = long;
  static pointer allocate(allocator_type &, size_type __n) {
    long __trans_tmp_3 = __n;
    if (__builtin_expect(__trans_tmp_3, false))
      if (__trans_tmp_3)
        __throw_bad_array_new_length();
    operator new(sizeof(int));
    return allocate___trans_tmp_2;
  }
};
class throw_allocator_base {
  allocator<int> _M_allocator;
public:
  int *allocate(long __n) {
    if (__n)
      __throw_bad_alloc();
    int *a = allocator_traits<allocator<int>>::allocate(_M_allocator, __n);
    return a;
  }
};
template <typename Alloc> void check_allocate_max_size() {
  Alloc a;
  long __trans_tmp_1 = 0;
  a.allocate(__trans_tmp_1 + 1);
}
int main() { check_allocate_max_size<throw_allocator_base>(); }
# 6 "./ipa/pr106258.C" 2
