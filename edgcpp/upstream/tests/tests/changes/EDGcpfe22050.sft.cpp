//type:fp
//options_all:--c++20
//remark:Defaulted constexpr constructor in a union
// 4/13/26  [EDGcpfe/22050,EDGcpfe/24177,EDGcpfe/28705]
//
// Defaulted constexpr constructor in a union
//
// Previously, the front end issued a spurious error for this case in all C++
// modes accepting constexpr constructors.  Now, the error is no longer issued
// in C++20 modes because C++20 no longer requires objects during constant
// evaluation to be fully initialized.  This was already handled for non-union
// class types (see the entry for EDGcpfe/23646), but the union case was
// overlooked.  These changes also eliminate a spurious error on constexpr
// constructor instantiations that turn out not to initialize a member.  For
// example:
//
// Previously, this elicited an error about S<int>::x not being initialized by
// the default constructor, but that is not a requirement for instantiated
// constructors such as this one.
union U {
  int x;
  constexpr U() = default;
};
