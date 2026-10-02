//type:fp
//options:--gn 150200:--gn 160000:--clang_version 200100:--clang_version 210100:--clang_version 220100
//options_all:-w

constexpr bool base_of_virtual_base_is_virtual =
#if defined(__clang__)
  false;
#else
  true;
#endif

struct X;

struct B
{ };

struct VBB
{ };

struct VB : VBB
{ };

struct D : B, virtual VB
{ };

struct DD : D
{ };

struct DVBB : VBB, virtual VB
{ };


static_assert(!__is_base_of(X, D));
static_assert( __is_base_of(B, D));
static_assert( __is_base_of(B, B));
static_assert( __is_base_of(VB, VB));
static_assert( __is_base_of(VB, D));
static_assert( __is_base_of(VBB, D));
static_assert( __is_base_of(VB, DD));
static_assert( __is_base_of(VBB, DD));
static_assert(!__is_base_of(VB, B));
static_assert( __is_base_of(D, D));
static_assert(!__is_base_of(D, B));
static_assert(!__is_base_of(D, VB));
static_assert(!__is_base_of(B, VB));

static_assert(!__builtin_is_virtual_base_of(X, D));
static_assert(!__builtin_is_virtual_base_of(B, D));
static_assert(!__builtin_is_virtual_base_of(B, B));
static_assert(!__builtin_is_virtual_base_of(VB, VB));
static_assert( __builtin_is_virtual_base_of(VB, D));
static_assert( __builtin_is_virtual_base_of(VBB, D) ==
               base_of_virtual_base_is_virtual);
static_assert( __builtin_is_virtual_base_of(VB, DD));
static_assert( __builtin_is_virtual_base_of(VBB, DD) ==
               base_of_virtual_base_is_virtual);
static_assert( __builtin_is_virtual_base_of(VBB, DVBB) ==
               base_of_virtual_base_is_virtual);
static_assert(!__builtin_is_virtual_base_of(VB, B));
static_assert(!__builtin_is_virtual_base_of(D, D));
static_assert(!__builtin_is_virtual_base_of(D, B));
static_assert(!__builtin_is_virtual_base_of(D, VB));
static_assert(!__builtin_is_virtual_base_of(B, VB));
