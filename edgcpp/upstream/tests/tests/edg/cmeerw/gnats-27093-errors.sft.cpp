//type:fn
//options:--gn 130200:--clang_version 180100
//options_all:--target linux_aarch64 -w --c++17

namespace dependent_vector_types
{
  template<int I, typename T>
  void g()
  {
#if defined(__clang__)
    {
      __attribute__((neon_vector_type(I))) short s; // error
    }
#endif

#if defined(__clang__)
    {
      __attribute__((neon_vector_type(4))) T s; // error
    }
#endif
  }

  template void g<0, short>();
}

namespace neon_underlying_types
{
  enum E { };

  using SHORT = short;

#if defined(__clang__)
  void f1()
  {
    { char __attribute__((neon_vector_type(8))) v; } // error
    { bool __attribute__((neon_vector_type(8))) v; } // error
    { E __attribute__((neon_vector_type(2))) v; }    // error
    { SHORT __attribute__((neon_vector_type(4))) v; } // Okay
  }
#endif

  void f2()
  {
    { __edg_neon_vector_type__(char, 8) v; }  // error
    { __edg_neon_vector_type__(bool, 8) v; }  // error
    { __edg_neon_vector_type__(E, 2) v; }     // error

    { __edg_neon_polyvector_type__(char, 8) v; }        // error
    { __edg_neon_polyvector_type__(signed char, 8) v; } // error
    { __edg_neon_polyvector_type__(unsigned int, 2) v; } // error

    { __edg_neon_vector_type__(SHORT, 8) v; } // Okay
  }
}

namespace scalable_vector_underlying_types
{
  enum E { };
  struct C { };

  using SHORT = short;

  void f()
  {
    { __edg_scalable_vector_type__(short, 0) v; } // error
    { __edg_scalable_vector_type__(short, 5) v; } // error
    { __edg_scalable_vector_type__(bool, 3) v; }  // error

    { __edg_scalable_vector_type__(E, 1) v; }           // error
    { __edg_scalable_vector_type__(C, 1) v; }           // error
    { __edg_scalable_vector_type__(char, 1) v; }        // error
    { __edg_scalable_vector_type__(const short, 1) v; } // error
    { __edg_scalable_vector_type__(void, 1) v; }        // error
    { __edg_scalable_vector_type__(char *, 1) v; }      // error

    {
      const int i = 2;
      __edg_scalable_vector_type__(short, i) v; // error
    }

    // following are okay
    { __edg_scalable_vector_type__(SHORT, 1) v; }
    { __edg_scalable_vector_type__(signed char, 1) v; }
    { __edg_scalable_vector_type__(unsigned char, 1) v; }
    { __edg_scalable_vector_type__(short, 1) v; }
    { __edg_scalable_vector_type__(unsigned short, 1) v; }
    { __edg_scalable_vector_type__(int, 1) v; }
    { __edg_scalable_vector_type__(unsigned int, 1) v; }
    { __edg_scalable_vector_type__(long, 1) v; }
    { __edg_scalable_vector_type__(unsigned long, 1) v; }
    { __edg_scalable_vector_type__(float, 1) v; }
    { __edg_scalable_vector_type__(double, 1) v; }
    { __edg_scalable_vector_type__(__fp16, 1) v; }
    { __edg_scalable_vector_type__(__bf16, 1) v; }

    { __edg_scalable_vector_type__(bool, 1) v; }
  }
}

namespace use_scalable_vector
{
  auto sizeof_val = sizeof(__SVInt8_t);    // error
  auto alignof_val = alignof(__SVInt8_t);  // error
  alignas(__SVInt8_t) int int_val;         // error

  template<__SVInt8_t v>          // error
  struct Tmpl
  { };

  __SVInt8_t g;                   // error

  void alloc()
  {
    auto *p = new __SVInt8_t;     // error
    delete p;                     // error
  }

  struct C
  {
    __SVInt8_t m;                 // error
  };

  void init()
  {
    __SVInt8_t v = { };           // error
  }

  void array()
  {
    __SVInt8_t arr[2];            // error
  }

  void pointer_arith(__SVInt8_t *v)
  {
    v + 0;                        // error
    v + 1;                        // error
    0 + v;                        // error
    1 + v;                        // error
    v[1];                         // error
    v[0];                         // error
    *v;                           // Okay
  }

  void lambda_capture(__SVInt8_t v)
  {
    [&v] () { };                  // Okay
    [v] () { };                   // error
    [c = v] () { };               // error
  }

  void operator +(__SVInt8_t);    // error

  void expr_use(__SVInt8_t v)
  {
    v + 1;                        // error
    +v;                           // error
  }
}

namespace cannot_apply_to_arrays
{
#if defined(__clang__)
  unsigned short arr[2] __attribute__((neon_vector_type(4))); // error
#endif
}
