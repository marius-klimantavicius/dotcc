//type: fp
//options: 
# 0 "./lto/20090311-1_0.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./lto/20090311-1_0.C"

# 1 "./lto/20090311-1.h" 1
typedef unsigned long uint32;
typedef int JSIntn;

typedef JSIntn JSBool;
typedef struct JSContext JSContext;
typedef struct JSObject JSObject;
typedef long long JSInt64;
typedef JSInt64 JSWord;
typedef JSWord jsword;
typedef jsword jsval;

typedef JSBool
(* JSPropertyOp)(JSContext *cx, JSObject *ojb, jsval id,
     jsval *vp);

struct JSClass {
    const char *name;
    uint32 flags;
    JSPropertyOp addProperty;
};

extern struct JSClass K;
# 3 "./lto/20090311-1_0.C" 2
bool flag;

struct B {
    int a;
    enum { ANOTHER, ONE } f2_;
    float c;
};

extern struct B x[];

struct C {
    int x;
    struct B *p;
    float d;
};

C c = { 0, 0, 3.4 };

struct A {
    enum { UNO, DOS, TRES } f1_;
    int x;
};

A a;

extern int foo();

int
main()
{
  a.x = 4 + c.x;
  foo();
  return 0;
}
