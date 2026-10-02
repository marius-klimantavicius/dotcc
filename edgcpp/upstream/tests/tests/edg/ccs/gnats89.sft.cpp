//type:fn

class A {
 public:
  virtual void foo(void);
  virtual void bar(void);
};
class B : public A {
  virtual void foo(void);
};
#define VFS(CLASS) void CLASS::foo(void){} void CLASS::bar(void){}

VFS(B)
  
