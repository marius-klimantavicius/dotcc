//type: fp
//options: 
# 0 "./lto/pr60150_1.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./lto/pr60150_1.C"

# 1 "./lto/pr60150.H" 1
struct Base {
  virtual void f() = 0;
};

struct X : public Base { };
struct Y : public Base { };
struct Z : public Base { };
struct T : public Base { };

struct S : public X, public Y, public Z



{
  void f()



  ;
};
# 3 "./lto/pr60150_1.C" 2

void S::f() { }
