//type: fp
//options: --c++11 -W
// EDG - changed to pick up smaller cases
// C++26 P1854R4 - Making non-encodable string literals ill-formed
// { dg-do compile { target c++11 } }
// { dg-require-effective-target int32 }
// { dg-options "-pedantic-errors -finput-charset=UTF-8 -fexec-charset=UTF-8" }

wchar_t g = L'abcd';					// { dg-error "multi-character literal cannot have an encoding prefix" "" { target c++23 } }
							// { dg-warning "multi-character literal cannot have an encoding prefix" "" { target c++20_down } .-1 }
wchar_t h = L'\x61\x62\x63\x64';			// { dg-error "multi-character literal cannot have an encoding prefix" "" { target c++23 } }
							// { dg-warning "multi-character literal cannot have an encoding prefix" "" { target c++20_down } .-1 }
