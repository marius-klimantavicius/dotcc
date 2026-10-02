//type: fp
//options: 
# 0 "./warn/Warray-bounds-13.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./warn/Warray-bounds-13.C"
# 11 "./warn/Warray-bounds-13.C"
namespace std {

typedef long unsigned int size_t;
struct nothrow_t { };
extern const nothrow_t nothrow;

}

void* operator new (std::size_t, const std::nothrow_t &) throw ()
  __attribute__ ((__alloc_size__ (1), __malloc__));
void* operator new[] (std::size_t, const std::nothrow_t &) throw ()
    __attribute__ ((__alloc_size__ (1), __malloc__));



typedef int int32_t;

void sink (void*);

template <int N> struct S { char a[N]; };

void sink (void*);
# 41 "./warn/Warray-bounds-13.C"
void warn_nothrow_new ()
{
  do { int32_t *p = (int32_t*)new (std::nothrow) S<0>; p[0] = 0; sink (p); } while (0);

  do { int32_t *p = (int32_t*)new (std::nothrow) S<1>; p[0] = 0; sink (p); } while (0);
  do { int32_t *p = (int32_t*)new (std::nothrow) S<2>; p[0] = 0; sink (p); } while (0);
  do { int32_t *p = (int32_t*)new (std::nothrow) S<3>; p[0] = 0; sink (p); } while (0);

  do { int32_t *p = (int32_t*)new (std::nothrow) S<4>; p[0] = 0; sink (p); } while (0);

  do { int32_t *p = (int32_t*)new (std::nothrow) S<0>; p[1] = 0; sink (p); } while (0);
  do { int32_t *p = (int32_t*)new (std::nothrow) S<1>; p[1] = 0; sink (p); } while (0);
  do { int32_t *p = (int32_t*)new (std::nothrow) S<2>; p[1] = 0; sink (p); } while (0);
  do { int32_t *p = (int32_t*)new (std::nothrow) S<3>; p[1] = 0; sink (p); } while (0);
  do { int32_t *p = (int32_t*)new (std::nothrow) S<4>; p[1] = 0; sink (p); } while (0);
  do { int32_t *p = (int32_t*)new (std::nothrow) S<5>; p[1] = 0; sink (p); } while (0);
  do { int32_t *p = (int32_t*)new (std::nothrow) S<6>; p[1] = 0; sink (p); } while (0);
  do { int32_t *p = (int32_t*)new (std::nothrow) S<7>; p[1] = 0; sink (p); } while (0);

  do { int32_t *p = (int32_t*)new (std::nothrow) S<8>; p[1] = 0; sink (p); } while (0);
}


void warn_nothrow_array_new ()
{



  do { int32_t *p = (int32_t*)new (std::nothrow) char [0]; p[0] = 0; sink (p); } while (0);

  do { int32_t *p = (int32_t*)new (std::nothrow) char [1]; p[0] = 0; sink (p); } while (0);
  do { int32_t *p = (int32_t*)new (std::nothrow) char [2]; p[0] = 0; sink (p); } while (0);
  do { int32_t *p = (int32_t*)new (std::nothrow) char [3]; p[0] = 0; sink (p); } while (0);

  do { int32_t *p = (int32_t*)new (std::nothrow) char [4]; p[0] = 0; sink (p); } while (0);

  do { int32_t *p = (int32_t*)new (std::nothrow) char [0]; p[1] = 0; sink (p); } while (0);
  do { int32_t *p = (int32_t*)new (std::nothrow) char [1]; p[1] = 0; sink (p); } while (0);
  do { int32_t *p = (int32_t*)new (std::nothrow) char [2]; p[1] = 0; sink (p); } while (0);
  do { int32_t *p = (int32_t*)new (std::nothrow) char [3]; p[1] = 0; sink (p); } while (0);
  do { int32_t *p = (int32_t*)new (std::nothrow) char [4]; p[1] = 0; sink (p); } while (0);
  do { int32_t *p = (int32_t*)new (std::nothrow) char [5]; p[1] = 0; sink (p); } while (0);
  do { int32_t *p = (int32_t*)new (std::nothrow) char [6]; p[1] = 0; sink (p); } while (0);
  do { int32_t *p = (int32_t*)new (std::nothrow) char [7]; p[1] = 0; sink (p); } while (0);

  do { int32_t *p = (int32_t*)new (std::nothrow) char [8]; p[1] = 0; sink (p); } while (0);
}
