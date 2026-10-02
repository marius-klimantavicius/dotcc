//type: fp
//options:  --c++20 --modules
// { dg-additional-options -fmodules-ts }

export module foo;
// { dg-module-cmi foo }

export struct pthread_attr_t
// guess where this came from?
{
  int m;
};
typedef struct pthread_attr_t pthread_attr_t;

struct bob 
{
};
export typedef struct bob bob;
