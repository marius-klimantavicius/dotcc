//type:fp
//options_all:-w --gn 140200 --c++17
//remark:auto(...) and auto{...} in non-C++23 modes
// 12/23/25 [EDGcpfe/28600]
//
// auto(...) and auto{...} in non-C++23 modes
//
// The changes for EDGcpfe/26452 and EDGcpfe/28379 enabled the C++23 "auto cast"
// feature in modes that aren't C++23 modes from the front end's perspective.
// However, those changes overlooked some ambiguity resolution issues, causing,
// e.g., the following example to mis-parsed:
//
// This is now fixed.
template<typename T> void f() noexcept(noexcept((auto(T{}))));
