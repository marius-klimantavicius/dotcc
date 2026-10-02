//options_all:-r -x -tused
//options: --microsoft -n;cn

__declspec(selectany) int x = 1;
struct __declspec(uuid("xxx")) A;
struct B {
  int __declspec(property(get=f)) i;
};
void __declspec(nothrow) f();

