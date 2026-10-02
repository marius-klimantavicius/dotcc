//type: fp
//options: 
# 0 "./lto/20090311-1_1.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./lto/20090311-1_1.C"
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
# 2 "./lto/20090311-1_1.C" 2

struct A {
    enum { UNO, DOS, TRES } f1_;
    int x;
};

struct B;

extern struct B x[];

struct C {
    int x;
    struct B *p;
    float d;
};

extern A a;
extern B b;
extern bool flag;
extern C c;

int foo()
{
  if (!flag)
    return a.x - c.x;
  return 0;
}
