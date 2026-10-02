//type:fp
//options_all:--microsoft
//remark:[4.7] Microsoft C++: __is_convertible_to with reference type as second argument
// 5/1/13   [EDGcpfe/13960]
//
// Microsoft C++: __is_convertible_to with reference type as second argument
//
// The implementation of __is_convertible_to in Microsoft mode (which has
// nonstandard behavior) has been modified so that it produces false if the
// destination type is a reference to a class or array type that is less
// qualified than the source type.
//
// See also the entry for EDGcpfe/13625 (dated 2/12/13).
struct S {};
static_assert(!__is_convertible_to(S const, S&),
              "Cannot drop type-qualifiers");
