//type: fp
//options:  --c++14: --c++14
# 1 "SemaCXX/constexpr-unsigned-high-bit.cpp"
# 1 "<built-in>" 1
# 1 "<built-in>" 3
# 461 "<built-in>" 3
# 1 "<command line>" 1
# 1 "<built-in>" 2
# 1 "SemaCXX/constexpr-unsigned-high-bit.cpp" 2



# 1 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/limits.h" 1
# 25 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/limits.h"
# 1 "/usr/include/limits.h" 1 3 4
# 26 "/usr/include/limits.h" 3 4
# 1 "/usr/include/features.h" 1 3 4
# 345 "/usr/include/features.h" 3 4
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 346 "/usr/include/features.h" 2 3 4
# 375 "/usr/include/features.h" 3 4
# 1 "/usr/include/sys/cdefs.h" 1 3 4
# 392 "/usr/include/sys/cdefs.h" 3 4
# 1 "/usr/include/bits/wordsize.h" 1 3 4
# 393 "/usr/include/sys/cdefs.h" 2 3 4
# 376 "/usr/include/features.h" 2 3 4
# 399 "/usr/include/features.h" 3 4
# 1 "/usr/include/gnu/stubs.h" 1 3 4
# 10 "/usr/include/gnu/stubs.h" 3 4
# 1 "/usr/include/gnu/stubs-64.h" 1 3 4
# 11 "/usr/include/gnu/stubs.h" 2 3 4
# 400 "/usr/include/features.h" 2 3 4
# 27 "/usr/include/limits.h" 2 3 4
# 144 "/usr/include/limits.h" 3 4
# 1 "/usr/include/bits/posix1_lim.h" 1 3 4
# 160 "/usr/include/bits/posix1_lim.h" 3 4
# 1 "/usr/include/bits/local_lim.h" 1 3 4
# 38 "/usr/include/bits/local_lim.h" 3 4
# 1 "/usr/include/linux/limits.h" 1 3 4
# 39 "/usr/include/bits/local_lim.h" 2 3 4
# 161 "/usr/include/bits/posix1_lim.h" 2 3 4
# 145 "/usr/include/limits.h" 2 3 4



# 1 "/usr/include/bits/posix2_lim.h" 1 3 4
# 149 "/usr/include/limits.h" 2 3 4



# 1 "/usr/include/bits/xopen_lim.h" 1 3 4
# 33 "/usr/include/bits/xopen_lim.h" 3 4
# 1 "/usr/include/bits/stdio_lim.h" 1 3 4
# 34 "/usr/include/bits/xopen_lim.h" 2 3 4
# 153 "/usr/include/limits.h" 2 3 4
# 26 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/limits.h" 2
# 5 "SemaCXX/constexpr-unsigned-high-bit.cpp" 2

constexpr unsigned inc() {
  unsigned i = 2147483647;
  ++i;
  return i;
}

constexpr unsigned dec() {
  unsigned i = (-2147483647 -1);
  --i;
  return i;
}
