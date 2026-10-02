//type:fp
//remark:[4.14] Memory corruption on interpreting some derived-to-base conversions
// 3/21/17  [EDGcpfe/18135]
//
// Memory corruption on interpreting some derived-to-base conversions
//
// The constexpr interpreter sometimes corrupted its own stack when evaluating a
// derived-to-base conversion applied to a run-time address.
//
// This is now fixed.
struct B1 {};
struct B2 {};
struct D: B2, B1 {
   constexpr D(D const &src): B2(src) {}  // This corrupted memory, likely
   constexpr explicit D(int) {}           // leading to an abort.
};
int main() {
  D d(42);
  D e = d;  // Address of d is passed as a "run-time" address.
}
