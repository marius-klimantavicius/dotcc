//type: fp
//options: 
# 0 "./warn/Warray-bounds-11.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./warn/Warray-bounds-11.C"
# 11 "./warn/Warray-bounds-11.C"
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
# 37 "./warn/Warray-bounds-11.C"
void warn_op_new ()
{
  do { int32_t *p = (int32_t*) operator new (0, std::nothrow); p[0] = 0; sink (p); } while (0);

  do { int32_t *p = (int32_t*) operator new (1, std::nothrow); p[0] = 0; sink (p); } while (0);
  do { int32_t *p = (int32_t*) operator new (2, std::nothrow); p[0] = 0; sink (p); } while (0);
  do { int32_t *p = (int32_t*) operator new (3, std::nothrow); p[0] = 0; sink (p); } while (0);

  do { int32_t *p = (int32_t*) operator new (4, std::nothrow); p[0] = 0; sink (p); } while (0);

  do { int32_t *p = (int32_t*) operator new (0, std::nothrow); p[1] = 0; sink (p); } while (0);
  do { int32_t *p = (int32_t*) operator new (1, std::nothrow); p[1] = 0; sink (p); } while (0);
  do { int32_t *p = (int32_t*) operator new (2, std::nothrow); p[1] = 0; sink (p); } while (0);
  do { int32_t *p = (int32_t*) operator new (3, std::nothrow); p[1] = 0; sink (p); } while (0);
  do { int32_t *p = (int32_t*) operator new (4, std::nothrow); p[1] = 0; sink (p); } while (0);
  do { int32_t *p = (int32_t*) operator new (5, std::nothrow); p[1] = 0; sink (p); } while (0);
  do { int32_t *p = (int32_t*) operator new (6, std::nothrow); p[1] = 0; sink (p); } while (0);
  do { int32_t *p = (int32_t*) operator new (7, std::nothrow); p[1] = 0; sink (p); } while (0);

  do { int32_t *p = (int32_t*) operator new (8, std::nothrow); p[1] = 0; sink (p); } while (0);
}


void warn_op_array_new ()
{



  do { int32_t *p = (int32_t*) operator new[] (0, std::nothrow); p[0] = 0; sink (p); } while (0);

  do { int32_t *p = (int32_t*) operator new[] (1, std::nothrow); p[0] = 0; sink (p); } while (0);
  do { int32_t *p = (int32_t*) operator new[] (2, std::nothrow); p[0] = 0; sink (p); } while (0);
  do { int32_t *p = (int32_t*) operator new[] (3, std::nothrow); p[0] = 0; sink (p); } while (0);

  do { int32_t *p = (int32_t*) operator new[] (4, std::nothrow); p[0] = 0; sink (p); } while (0);

  do { int32_t *p = (int32_t*) operator new[] (0, std::nothrow); p[1] = 0; sink (p); } while (0);
  do { int32_t *p = (int32_t*) operator new[] (1, std::nothrow); p[1] = 0; sink (p); } while (0);
  do { int32_t *p = (int32_t*) operator new[] (2, std::nothrow); p[1] = 0; sink (p); } while (0);
  do { int32_t *p = (int32_t*) operator new[] (3, std::nothrow); p[1] = 0; sink (p); } while (0);
  do { int32_t *p = (int32_t*) operator new[] (4, std::nothrow); p[1] = 0; sink (p); } while (0);
  do { int32_t *p = (int32_t*) operator new[] (5, std::nothrow); p[1] = 0; sink (p); } while (0);
  do { int32_t *p = (int32_t*) operator new[] (6, std::nothrow); p[1] = 0; sink (p); } while (0);
  do { int32_t *p = (int32_t*) operator new[] (7, std::nothrow); p[1] = 0; sink (p); } while (0);

  do { int32_t *p = (int32_t*) operator new[] (8, std::nothrow); p[1] = 0; sink (p); } while (0);
}
