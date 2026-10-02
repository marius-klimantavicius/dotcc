//type: fp
//options: 
# 0 "./lto/pr113208_1.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./lto/pr113208_1.C"

# 1 "./lto/pr113208.h" 1
template <typename _Tp> struct _Vector_base {
  int g() const;
  _Vector_base(int, int);
};
template <typename _Tp>
struct vector : _Vector_base<_Tp> {
  vector(const vector &__x)
      : _Vector_base<_Tp>(1, __x.g()) {}
 vector() : _Vector_base<_Tp>(1, 2) {}
};
# 3 "./lto/pr113208_1.C" 2

struct QualityValue;
vector<QualityValue> values1;
vector<QualityValue> values{values1};
