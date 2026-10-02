//type: fp
//options:  --c++03: --c++11
# 1 "SemaCXX/printf-cstr.cpp"
# 1 "<built-in>" 1
# 1 "<built-in>" 3
# 431 "<built-in>" 3
# 1 "<command line>" 1
# 1 "<built-in>" 2
# 1 "SemaCXX/printf-cstr.cpp" 2




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
# 6 "SemaCXX/printf-cstr.cpp" 2

extern "C" {
extern int printf(const char *restrict, ...);
extern int sprintf(char *, const char *restrict, ...);
}

class HasCStr {
  const char *str;
 public:
  HasCStr(const char *s): str(s) { }
  const char *c_str() {return str;}
};

class HasNoCStr {
  const char *str;
 public:
  HasNoCStr(const char *s): str(s) { }
  const char *not_c_str() {return str;}
};

extern const char extstr[16];
void pod_test() {
  char str[] = "test";
  char dest[32];
  char formatString[] = "non-const %s %s";
  HasCStr hcs(str);
  HasNoCStr hncs(str);
  int n = 10;

  printf("%d: %s\n", n, hcs.c_str());
  printf("%d: %s\n", n, hcs);







  printf("%d: %s\n", n, hncs);






  sprintf(str, "%d: %s", n, hcs);







  printf(formatString, hcs, hncs);





  printf(extstr, hcs, n);



}

struct Printf {
  Printf();
  Printf(const Printf&);
  Printf(const char *,...) __attribute__((__format__(__printf__,2,3)));
};

void constructor_test() {
  const char str[] = "test";
  HasCStr hcs(str);
  Printf p("%s %d %s", str, 10, 10);
  Printf q("%s %d", hcs, 10);






}
