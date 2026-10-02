//type: fn
//options:  --c++14
# 1 "SemaCXX/literal-operators.cpp"
# 1 "<built-in>" 1
# 1 "<built-in>" 3
# 461 "<built-in>" 3
# 1 "<command line>" 1
# 1 "<built-in>" 2
# 1 "SemaCXX/literal-operators.cpp" 2


# 1 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/stddef.h" 1
# 84 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/stddef.h"
# 1 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/__stddef_header_macro.h" 1
# 85 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/stddef.h" 2



# 1 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/__stddef_ptrdiff_t.h" 1
# 18 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/__stddef_ptrdiff_t.h"
typedef long int ptrdiff_t;
# 89 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/stddef.h" 2




# 1 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/__stddef_size_t.h" 1
# 18 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/__stddef_size_t.h"
typedef long unsigned int size_t;
# 94 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/stddef.h" 2
# 103 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/stddef.h"
# 1 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/__stddef_wchar_t.h" 1
# 104 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/stddef.h" 2




# 1 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/__stddef_null.h" 1
# 109 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/stddef.h" 2




# 1 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/__stddef_nullptr_t.h" 1
# 114 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/stddef.h" 2
# 123 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/stddef.h"
# 1 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/__stddef_max_align_t.h" 1
# 19 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/__stddef_max_align_t.h"
typedef struct {
  long long __clang_max_align_nonce1
      __attribute__((__aligned__(__alignof__(long long))));
  long double __clang_max_align_nonce2
      __attribute__((__aligned__(__alignof__(long double))));
} max_align_t;
# 124 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/stddef.h" 2




# 1 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/__stddef_offsetof.h" 1
# 129 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/stddef.h" 2
# 4 "SemaCXX/literal-operators.cpp" 2

struct tag {
  void operator ""_tag_bad (const char *);
  friend void operator ""_tag_good (const char *);
};

namespace ns { void operator ""_ns_good (const char *); }


extern "C++" void operator ""_extern_good (const char *);
extern "C++" { void operator ""_extern_good (const char *); }

void fn () { void operator ""_fn_good (const char *); }


void operator ""_good (char);
void operator ""_good (wchar_t);
void operator ""_good (char16_t);
void operator ""_good (char32_t);
void operator ""_good (unsigned long long);
void operator ""_good (long double);


void operator ""_good (const char *, size_t);
void operator ""_good (const wchar_t *, size_t);
void operator ""_good (const char16_t *, size_t);
void operator ""_good (const char32_t *, size_t);


void operator ""_good (const char[]);
typedef const char c;
void operator ""_good (c*);


void operator ""_cv_good (volatile const char *, const size_t);


template <char...> void operator ""_good ();

template <typename...> void operator ""_invalid();
template <wchar_t...> void operator ""_invalid();
template <unsigned long long...> void operator ""_invalid();

_Complex float operator""if(long double);
_Complex float test_if_1() { return 2.0f + 1.5if; };
void test_if_2() { "foo"if; }

template<typename T> void dependent_member_template() {
  T().template operator""_foo<int>();
}

namespace PR51142 {

template<typename T>
constexpr auto operator ""_l();
}
