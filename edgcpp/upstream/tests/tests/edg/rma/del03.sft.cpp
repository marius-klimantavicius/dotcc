//options_all:-r -x -tused
//options: --strict;cp

#ifdef __APPLE__
typedef __EDG_SIZE_TYPE__ size_t;
#else
typedef __EDG_SIZE_TYPE__ size_t;
#endif
void * operator new(size_t);
void operator delete(void *);
struct A {
  void * operator new(size_t, char *);
  void operator delete(void *, char *);
};
struct B : A {
  void * operator new(size_t, char *);
};
struct C : A {
  void * operator new(size_t, char *);
  using A::operator delete;
};
void operator delete(void *, char *);
struct D {
  void * operator new(size_t, char *);
};

