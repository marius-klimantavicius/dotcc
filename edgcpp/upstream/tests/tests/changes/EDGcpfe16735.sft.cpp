//type:fp
//options_all:--clang
//remark:[5.0] Clang compatibility: better support for certain clang builtins
// 12/19/17 [EDGcpfe/16735,EDGcpfe/17501,EDGcpfe/18076,EDGcpfe/18599,
//           EDGcpfe/18926,EDGcpfe/18988,EDGcpfe/19013,EDGcpfe/19041]
//
// Clang compatibility: better support for certain clang builtins
//
// There are a number of clang builtins (__builtin_nontemporal_store,
// __builtin_nontemporal_load, __c11_atomic_init, __c11_atomic_load,
// __c11_atomic_store, __c11_atomic_exchange,
// __c11_atomic_compare_exchange_strong, __c11_atomic_compare_exchange_weak,
// __c11_atomic_fetch_add, __c11_atomic_fetch_sub, __c11_atomic_fetch_and,
// __c11_atomic_fetch_or, __c11_atomic_fetch_xor, and __sync_swap) where
// the builtin type can vary from one invocation of the builtin to another.
// A change has been made to handle these cases (and restructure the way
// the front end currently handles __sync_* and __atomic_* cases to make
// it easier to handle other cases like this).
//
// A consequence of this change is that a back end may now see multiple routines
// with the same name and different types.
//
// In this case there will be two __c11_atomic_load a_routine entries in the
// IL, with different types.
//
// 7/6/16   [EDGcpfe/14807,EDGcpfe/16063,EDGcpfe/16627, EDGcpfe/16735,
//           EDGcpfe/17005,EDGcpfe/17359]
//
// C11 _Atomic types
//
// In C11 mode, the front end now accepts _Atomic types.  Such types are modeled
// as qualified types; e.g., _Atomic(int) is type int with an _Atomic qualifier
// (a tk_typeref entry) on top.  The size and alignment of an _Atomic type must
// therefore match the underlying non-_Atomic type.  That matches the behavior of,
// e.g., the GCC compiler, but not of the Clang compiler.  In Clang mode, the
// front end therefore disables _Atomic class types, because Clang treats them
// differently (their sizes differ from those of the underlying class types, and
// some operations -- like member access -- are not allowed on them).  A global
// variable c11_atomic_classes_disabled controls that behavior.  However, all
// Clang modes accept _Atomic scalar types (not just Clang C11 mode).
// Furthermore, the _Atomic feature is supported in all GNU C modes with
// gnu_version >= 40900 (not just GNU C11 mode).
int f(_Atomic(char) *pac, char c,
      _Atomic(int)  *pai, int  i) {
return __c11_atomic_load(pac, c) +
       __c11_atomic_load(pai, i);
}
