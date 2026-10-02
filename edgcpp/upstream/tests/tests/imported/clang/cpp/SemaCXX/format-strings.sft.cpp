//type: fn
//options:  --c++03: --c++11
# 1 "SemaCXX/format-strings.cpp"
# 1 "<built-in>" 1
# 1 "<built-in>" 3
# 431 "<built-in>" 3
# 1 "<command line>" 1
# 1 "<built-in>" 2
# 1 "SemaCXX/format-strings.cpp" 2




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
# 6 "SemaCXX/format-strings.cpp" 2

extern "C" {
extern int scanf(const char *restrict, ...);
extern int printf(const char *restrict, ...);
extern int vprintf(const char *restrict, va_list);
}

void f(char **sp, float *fp) {
  scanf("%as", sp);






  printf("%a", 1.0);
  scanf("%afoobar", fp);
}

void g() {
  printf("%ls", "foo");
}


class Foo {
public:
  const char *gettext(const char *fmt) __attribute__((format_arg(2)));

  int scanf(const char *, ...) __attribute__((format(scanf, 2, 3)));
  int printf(const char *, ...) __attribute__((format(printf, 2, 3)));
  int printf2(const char *, ...);

  static const char *gettext_static(const char *fmt) __attribute__((format_arg(1)));
  static int printf_static(const char *fmt, ...) __attribute__((format(printf, 1, 2)));
};

void h(int *i) {
  Foo foo;
  foo.scanf("%d");
  foo.printf("%d", i);
  Foo::printf_static("%d", i);

  printf(foo.gettext("%d"), i);
  printf(Foo::gettext_static("%d"), i);
}


extern "C" {
  int test_null_format(const char *format, ...) __attribute__((__format__ (__printf__, 1, 2)));



}

void rdar8269537(const char *f)
{
  test_null_format(false);





  test_null_format(0);
  test_null_format(__null);
  test_null_format(f);

}

int Foo::printf(const char *fmt, ...) {
  va_list ap;
  __builtin_va_start(ap, fmt);
  const char * const format = fmt;
  vprintf(format, ap);

  const char *format2 = fmt;
  vprintf(format2, ap);

  return 0;
}

int Foo::printf2(const char *fmt, ...) {
  va_list ap;
  __builtin_va_start(ap, fmt);
  vprintf(fmt, ap);

  return 0;
}


namespace Templates {
  template<typename T>
  void my_uninstantiated_print(const T &arg) {
    printf("%d", arg);
  }

  template<typename T>
  void my_print(const T &arg) {
    printf("%d", arg);
  }

  void use_my_print() {
    my_print("abc");
  }


  template<typename T>
  class UninstantiatedPrinter {
  public:
    static void print(const T &arg) {
      printf("%d", arg);
    }
  };

  template<typename T>
  class Printer {
    void format(const char *fmt, ...) __attribute__((format(printf,2,3)));
  public:

    void print(const T &arg) {
      format("%d", arg);
    }
  };

  void use_class(Printer<const char *> &p) {
    p.print("abc");
  }


  extern void (^block_print)(const char * format, ...) __attribute__((format(printf, 1, 2)));

  template<typename T>
  void uninstantiated_call_block_print(const T &arg) {
    block_print("%d", arg);
  }

  template<typename T>
  void call_block_print(const T &arg) {
    block_print("%d", arg);
  }

  void use_block_print() {
    call_block_print("abc");
  }
}

namespace implicit_this_tests {
struct t {
    void func1(const char *, ...) __attribute__((__format__(printf, 1, 2)));
    void (*func2)(const char *, ...) __attribute__((__format__(printf, 1, 2)));
    static void (*func3)(const char *, ...) __attribute__((__format__(printf, 1, 2)));
    static void func4(const char *, ...) __attribute__((__format__(printf, 1, 2)));
};

void f() {
  t t1;
  t1.func2("Hello %s");
  t::func3("Hello %s");
  t::func4("Hello %s");
}
}
