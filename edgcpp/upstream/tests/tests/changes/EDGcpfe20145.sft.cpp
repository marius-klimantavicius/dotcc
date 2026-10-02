//type:fp
//options_all:--c++17
//remark:[5.1] Inline static data members with deduced types
// 9/18/18  [EDGcpfe/20145]
//
// Inline static data members with deduced types
//
// The front end previously rejected an inline static data member with a
// deduced type if the initializer is not a constant expression.  This is now
// fixed.
struct wrapper {
  wrapper() {}
};

struct wrapped_object {
  inline static auto y = wrapper();  // Previously elicited a "must have a
                                     // constant value" error, now okay
};
