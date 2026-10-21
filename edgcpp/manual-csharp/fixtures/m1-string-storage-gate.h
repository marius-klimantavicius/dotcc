// SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
#ifndef STRING_STORAGE_GATE_H
#define STRING_STORAGE_GATE_H
#define ΚΕΙΜΕΝΟ u"\u00e9" "x"
#define ΕΠΙΣΤΡΟΦΗ(value) return value
#define ΒΟΗΘΟΣ βοήθησε
constexpr int βοήθησε(const char16_t* text){const char16_t& element=text[0]; ΕΠΙΣΤΡΟΦΗ(element);}
constexpr int βοήθησε(const char* text){const char& element=text[0]; ΕΠΙΣΤΡΟΦΗ(element);}
template<class T> constexpr int πρώτο(T text){ΕΠΙΣΤΡΟΦΗ(ΒΟΗΘΟΣ(text));}
#endif
