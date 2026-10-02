//type:fp
//options_all:--c++11
//remark:[4.11] Call via reference cast of constexpr function
// 2/1/16   [EDGcpfe/16818]
//
// Call via reference cast of constexpr function
//
// The front end previously incorrectly indicated that a call to a constexpr
// function was not a constant expression if the function is designated via a
// cast to a reference type.  This is now fixed.
constexpr int f() {
  return 3;
}
int arr[static_cast<decltype(f)&>(f)()];  // Previously an error, now okay
