//type: fp
//options: 
# 1 "SemaCXX/builtins-va_arg.cpp"
# 1 "<built-in>" 1
# 1 "<built-in>" 3
# 482 "<built-in>" 3
# 1 "<command line>" 1
# 1 "<built-in>" 2
# 1 "SemaCXX/builtins-va_arg.cpp" 2








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
# 10 "SemaCXX/builtins-va_arg.cpp" 2

int int_accumulator = 0;
double double_accumulator = 0;

int test_vprintf(const char *fmt, va_list ap) {
  char ch;
  int result = 0;
  while (*fmt != '\0') {
    ch = *fmt++;
    if (ch != '%') {
      continue;
    }

    ch = *fmt++;
    switch (ch) {
    case 'd':
      int_accumulator += __builtin_va_arg(ap, int);
      result++;
      break;

    case 'f':
      double_accumulator += __builtin_va_arg(ap, double);
      result++;
      break;

    default:
      break;
    }

    if (ch == '0') {
      break;
    }
  }
  return result;
}

int test_printf(const char *fmt, ...) {
  va_list ap;
  __builtin_va_start(ap, fmt);
  int result = test_vprintf(fmt, ap);
  __builtin_va_end(ap);
  return result;
}
