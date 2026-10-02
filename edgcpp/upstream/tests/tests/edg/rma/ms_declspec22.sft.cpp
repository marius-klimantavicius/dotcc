//options_all:-r -x -tused
//options: --microsoft -n;cp

extern int __declspec(allocate("xxx")) x;
int  __declspec(allocate("xxx")) x;

int __declspec(allocate("yyy")) y;
extern int y;

extern int z;
int __declspec(allocate("zzz")) z;

struct S {
  static int x;
  static int __declspec(allocate("xxx")) y;
  static int __declspec(allocate("xxx")) z;
};
int __declspec(allocate("xxx")) S::y = 0;
int __declspec(allocate("xxx")) S::x = 0;
int S::z = 0;

int f() {
  static int __declspec(allocate("xxx")) y;
  return y;
}

