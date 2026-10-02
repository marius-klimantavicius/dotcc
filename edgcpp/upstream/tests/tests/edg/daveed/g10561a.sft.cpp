//remark: Multiple extern "C" definitions
//options:--microsoft_bugs;rp

namespace NS {
 extern "C" {
   void __forceinline foo() {}  // inline or __forceinline would work
 }
}

namespace NS1 {
 extern "C" {
   void __forceinline foo() {} // inline or __forceinline would work
 }
}
int main()
{}

