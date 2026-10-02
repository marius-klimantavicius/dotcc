//type:fp
//remark:[4.14] Unbounded loop on address of array element subobject
// 4/25/17  [EDGcpfe/18285]
//
// Unbounded loop on address of array element subobject
//
// The front end sometimes entered an unbounded loop in the constexpr interpreter
// (in function translate_interpreter_offset) when attempting to fold an address
// of a subobject of an array element.
//
// This is now fixed.
struct X { int x = 1; };
constexpr X ax[2] = {{}};
int const *p = &ax[1].x;
                // Previously triggered an unbounded loop.  Now fixed.
