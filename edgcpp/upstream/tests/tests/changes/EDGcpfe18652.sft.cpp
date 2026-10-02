//type:fp
//options_all:--c++14 --g++
//remark:[4.14] Temporaries and constexpr subobjects
// 8/9/17   [EDGcpfe/18652]
//
// Temporaries and constexpr subobjects
//
// The constexpr interpreter previously did not always correctly record a
// subobject constructed from a temporary value as actually being initialized.
// This could result in spurious constant-expression evaluation errors.
//
// This is now fixed.
struct X { int x; };
constexpr auto makeX(int) { return X{}; }

struct Y { X m; };
constexpr auto makeY(int p) { return Y{makeX(p)}; }

constexpr auto y = makeY(0);  // Previously an error because member x
                              // was not seen as initialized.
