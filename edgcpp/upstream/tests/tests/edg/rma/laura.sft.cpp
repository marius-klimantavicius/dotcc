//options_all:-r -x -tused --diag_suppress=172
//options: --strict;cn:;cp

#ifdef __APPLE__
typedef __EDG_SIZE_TYPE__ size_t;
#else
typedef __EDG_SIZE_TYPE__ size_t;
#endif
class A {
public:
  void operator delete(void*);
  void* operator new(size_t);
};
class B {
  friend void operator delete(void*);
  friend void* operator new(size_t);
  friend void A::operator delete(void*);
  friend void* A::operator new(size_t);
};
static void operator delete(void*) { }
static void* operator new(size_t) { return 0; }
