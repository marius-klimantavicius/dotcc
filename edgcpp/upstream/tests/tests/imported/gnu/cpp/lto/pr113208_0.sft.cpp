//type: fp
//options: 
# 0 "./lto/pr113208_0.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./lto/pr113208_0.C"






# 1 "./lto/pr113208.h" 1
template <typename _Tp> struct _Vector_base {
  int g() const;
  _Vector_base(int, int);
};
template <typename _Tp>
struct vector : _Vector_base<_Tp> {
  constexpr vector(const vector &__x)
      : _Vector_base<_Tp>(1, __x.g()) {}
 vector() : _Vector_base<_Tp>(1, 2) {}
};
# 8 "./lto/pr113208_0.C" 2

struct QualityValue;
struct k : vector<QualityValue> {};

void m(k);
void n(k i) { m(i); }
