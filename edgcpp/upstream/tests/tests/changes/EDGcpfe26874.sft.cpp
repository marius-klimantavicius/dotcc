//type:fp
//options_all:--gn 999999
//remark:[6.7] GNU compatibility: Extended asm "g" ("general") constraint
// 12/18/23 [EDGcpfe/26874]
//
// GNU compatibility: Extended asm "g" ("general") constraint
//
// Ordinarily, operands in GNU-style extended asm constructs that are of class
// type are converted to an arithmetic type if possible (which is the case here
// for operand s).  However, that is not done for "memory operands" as determined
// by the operand constraints.  Previously, the "g" constraint was not treated as
// a memory operand, and thus the conversion to int was applied, but that means
// that the operand becomes a prvalue which triggers an error since the "+"
// constraint requires mutability.  That is now fixed: The "g" (for "general")
// constraint is now recognized as indicating a (potential) memory operand,
// thereby inhibiting the implicit conversion to an arithmetic type.
struct S { operator int(); };
void g() {
  S s;
  __asm__("" : "+g,x"(s));
}
