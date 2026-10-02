//type:fp
//options:--gnu_version 130100:--gnu_version 140100:--clang_version 190100:--clang_version 180100:--clang_version 170000:--clang_version 160000
//options_all:--c++20 -w

struct B
{
  int i;
};

struct D : B
{ };

enum E
{ };

enum class EC
{ };

#if (__GNUC__ >= 14) || (__clang_major__ >= 16)
static_assert(!__is_scoped_enum(E), "");
static_assert( __is_scoped_enum(EC), "");
#else
int __is_scoped_enum;
#endif

#if !defined(__clang__) || (__clang_major__ >= 18)
static_assert(!__reference_constructs_from_temporary(int, int), "");
#else
int __reference_constructs_from_temporary;
#endif

#if !defined(__clang__) || (__clang_major__ >= 19)
static_assert(__is_nothrow_convertible(B, B), "");
static_assert(__is_layout_compatible(B, B), "");
static_assert(__is_pointer_interconvertible_base_of(B, D), "");
static_assert(!__reference_converts_from_temporary(int, int), "");
#else
int __is_nothrow_convertible;
int __is_layout_compatible;
int __is_pointer_interconvertible_base_of;
int __reference_converts_from_temporary;
#endif

#if defined(__clang__)
int __builtin_is_corresponding_member;
int __builtin_is_pointer_interconvertible_with_class;
#else
static_assert(__builtin_is_corresponding_member(&B::i, &B::i), "");
static_assert(__builtin_is_pointer_interconvertible_with_class(&B::i), "");
#endif
