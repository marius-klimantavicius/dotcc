//type:fp
//options_all:--c++11
//remark:[6.8] Microsoft compatibility: pack expansions and Microsoft nonreal base classes
// 9/8/25   [EDGcpfe/24065,EDGcpfe/24735,EDGcpfe/25677]
//
// Microsoft compatibility: pack expansions and Microsoft nonreal base classes
//
// In Microsoft mode, the front end emulates a Microsoft feature that finds
// certain names in dependent base classes.  That emulation could cause incorrect
// behavior when a pack is provided for a non-pack parameter in the template
// argument list of a dependent base class specifier.
// --microsoft:
template<typename T, typename ...>
struct B {
  template<typename U = T>  // Previously a spurious error.  Now okay.
  void foo();
};
template<typename ... Ts>
struct D : B<Ts ...>
{ };
