//type:fp
//options_all:--gnu=70300
//remark:[6.2] Issues with promotion of constants during lowering
// 9/14/20  [EDGcpfe/23310]
//
// Issues with promotion of constants during lowering
//
// A couple of issues were fixed when lowering constants in a function scope
// that are promoted to the file scope during lowering.  Those issues had
// resulted in a segfault (in mangled_variable_name_with_possible_qualification)
// or in IL write-read assertion failures and are now fixed.
// with --gnu_version=70300:
void (*fptr)(void) = [](void){};
struct A {
  void f() {
    static void *b[] = { &&L };
    const char *x = __FUNCTION__;
  L:;
  }
};
