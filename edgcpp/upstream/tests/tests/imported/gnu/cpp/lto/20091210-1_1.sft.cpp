//type: fp
//options: 
# 0 "./lto/20091210-1_1.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./lto/20091210-1_1.C"
# 1 "./lto/20091210-1_0.h" 1
struct Base1 {
    virtual ~Base1() {}
};
struct Base2 {
    virtual void f() = 0;
};
struct Base : Base1, Base2 {
    virtual void f();
};
# 2 "./lto/20091210-1_1.C" 2

struct Foo : Base {
    virtual void g();
};

void Foo::g() {}

int main() {}
