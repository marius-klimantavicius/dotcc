//options_all:-r -x -tused
//options: --microsoft -n;cn

extern int __declspec(allocate) a;                  // Error
extern int __declspec(allocate()) b;                // Error
extern int __declspec(allocate(a)) c;               // Error

extern int __declspec(allocate("xxx")) x;
int  __declspec(allocate("xxx")) x;

extern int __declspec(allocate("yyy")) y;
int __declspec(allocate("yyyyy")) y;                // Error

extern int z;
int __declspec(allocate("zzz")) z;

struct S {
  int __declspec(allocate("xxx")) x;                // Error
  static int __declspec(allocate("xxx")) y;
  static int __declspec(allocate("xxx")) z;
};
int __declspec(allocate("xxx")) S::y = 0;
int __declspec(allocate("yyy")) S::z = 0;           // Error

void __declspec(allocate("xxx")) f() {              // Error
  int __declspec(allocate("xxx")) x;                // Error
  static int __declspec(allocate("xxx")) y;
}

struct __declspec(allocate("xxx")) SS { };          // Error

