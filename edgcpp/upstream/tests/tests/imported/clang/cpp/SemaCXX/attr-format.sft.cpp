//type: fn
//options:  --c++23
# 1 "SemaCXX/attr-format.cpp"
# 1 "<built-in>" 1
# 1 "<built-in>" 3
# 499 "<built-in>" 3
# 1 "<command line>" 1
# 1 "<built-in>" 2
# 1 "SemaCXX/attr-format.cpp" 2

# 1 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/stdarg.h" 1
# 47 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/stdarg.h"
# 1 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/__stdarg_header_macro.h" 1
# 48 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/stdarg.h" 2



# 1 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/__stdarg___gnuc_va_list.h" 1
# 12 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/__stdarg___gnuc_va_list.h"
typedef __builtin_va_list __gnuc_va_list;
# 52 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/stdarg.h" 2




# 1 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/__stdarg_va_list.h" 1
# 12 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/__stdarg_va_list.h"
typedef __builtin_va_list va_list;
# 57 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/stdarg.h" 2




# 1 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/__stdarg_va_arg.h" 1
# 62 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/stdarg.h" 2




# 1 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/__stdarg___va_copy.h" 1
# 67 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/stdarg.h" 2




# 1 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/__stdarg_va_copy.h" 1
# 72 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/stdarg.h" 2
# 3 "SemaCXX/attr-format.cpp" 2

int printf(const char *fmt, ...) __attribute__((format(printf, 1, 2)));

struct S {
  static void f(const char *, ...) __attribute__((format(printf, 1, 2)));
  static const char *f2(const char *) __attribute__((format_arg(1)));



  void g(const char*, ...) __attribute__((format(printf, 2, 3)));
  const char* g2(const char*) __attribute__((format_arg(2)));

  void g3(this S&, const char *, ...) __attribute__((format(printf, 2, 3)));
  void g4(this const char* s, ...) __attribute__((format(printf, 1, 2)));
  consteval operator const char*() const { return "%f"; }

  void h(const char*, ...) __attribute__((format(printf, 1, 4)));

  void h2(const char*, ...) __attribute__((format(printf, 2, 1)));

  const char* h3(const char*) __attribute__((format_arg(1)));

  void h4(this S&, const char *, ...) __attribute__((format(printf, 1, 3)));


  void operator() (const char*, ...) __attribute__((format(printf, 2, 3)));
};

void s() {
  S().g4(4);

}


struct A { void a(const char*,...) __attribute((format(printf,2,3))); };
void b(A x) {
  x.a("%d", 3);
}



namespace PR8625 {
  struct S {
    static void f(const char*, const char*, ...)
      __attribute__((format(printf, 2, 3)));
  };
  void test(S s, const char* str) {
    s.f(str, "%s", str);
  }
}



void test_operator_call(S s, const char *str) {
  s("%s", str);
}

template <typename... Args>
void format(const char *fmt, Args &&...args)
    __attribute__((format(printf, 1, 2)));

template <typename Arg>
Arg &expand(Arg &a) { return a; }

struct foo {
  int big[10];
  foo();
  ~foo();

  template <typename... Args>
  void format(const char *const fmt, Args &&...args)
      __attribute__((format(printf, 2, 3))) {
    printf(fmt, expand(args)...);
  }
};

void format_invalid_nonpod(const char *fmt, struct foo f)
    __attribute__((format(printf, 1, 2)));

void do_format() {
  int x = 123;
  int &y = x;
  const char *s = "world";
  bool b = false;
  format("bare string");
  format("%s", 123);
  format("%s %s %u %d %i %p\n", "hello", s, 10u, x, y, &do_format);
  format("%s %s %u %d %i %p\n", "hello", s, 10u, x, y, do_format);
  format("bad format %s");

  format("%c %c %hhd %hd %d\n", (char)'a', 'a', 'a', (short)123, (int)123);
  format("%f %f %f\n", (__fp16)123.f, 123.f, 123.);
  format("%Lf", (__fp16)123.f);
  format("%Lf", 123.f);
  format("%hhi %hhu %hi %hu %i %u", b, b, b, b, b, b);
  format("%li", b);

  struct foo f;
  format_invalid_nonpod("hello %i", f);

  f.format("%s", 123);
  f.format("%s %s %u %d %i %p\n", "hello", s, 10u, x, y, &do_format);
  f.format("%s %s %u %d %i %p\n", "hello", s, 10u, x, y, do_format);
  f.format("bad format %s");
}
