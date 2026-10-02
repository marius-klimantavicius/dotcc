//type: rp
//options: 
# 0 "./torture/pr90313.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./torture/pr90313.C"


# 1 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stddef.h" 1 3 4
# 145 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stddef.h" 3 4

# 145 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stddef.h" 3 4
typedef long int ptrdiff_t;
# 214 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stddef.h" 3 4
typedef long unsigned int size_t;
# 425 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stddef.h" 3 4
typedef struct {
  long long __max_align_ll __attribute__((__aligned__(__alignof__(long long))));
  long double __max_align_ld __attribute__((__aligned__(__alignof__(long double))));
# 436 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stddef.h" 3 4
} max_align_t;






  typedef decltype(nullptr) nullptr_t;
# 4 "./torture/pr90313.C" 2


# 5 "./torture/pr90313.C"
namespace std {
  template<typename T, size_t N> struct array {
    T elems[N];
    const T &operator[](size_t i) const { return elems[i]; }
  };
}

using Coordinates = std::array<double, 3>;

Coordinates map(const Coordinates &c, size_t level)
{
  Coordinates result{ c[1], c[2], c[0] };

  if (level != 0)
    result = map (result, level - 1);

  return result;
}

int main()
{
  Coordinates vecOfCoordinates = { 1.0, 2.0, 3.0 };

  auto result = map(vecOfCoordinates, 1);
  if (result[0] != 3 || result[1] != 1 || result[2] != 2)
    __builtin_abort ();

  return 0;
}
