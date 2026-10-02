//type:fp
//options_all:--c++17
//remark:[4.14] C++17: Inline variables
// 7/18/17  [EDGcpfe/17704,EDGcpfe/17954]
//
// C++17: Inline variables
//
// As described in C++ Committee document P0386R2, the front end now supports
// inline variables and static data members.  Like inline functions, inline
// variables can be defined in multiple translation units without creating
// linker errors, but have a unique identity (address and value) in the
// resulting program.  Such variables are identified in the IL via the
// is_inline field of a_variable.  During lowering, inline variables are
// assigned to COMDAT groups, even in the Cfront ABI (COMDAT groups were
// previously used only in the IA-64 ABI).
inline const int i = 15;
struct S {
  inline static int j = 25;
};
