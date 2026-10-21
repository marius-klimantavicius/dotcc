// SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
#ifndef UNICODE_STORAGE_GATE_H
#define UNICODE_STORAGE_GATE_H
#define ΕΠΙΣΤΡΟΦΗ(τιμή) return τιμή
#define ΗΧΩ ηχώ
using γράμμα=char16_t;
constexpr const γράμμα& ηχώ(const γράμμα& τιμή){ΕΠΙΣΤΡΟΦΗ(τιμή);}
constexpr const long& ηχώ(const long& τιμή){ΕΠΙΣΤΡΟΦΗ(τιμή);}
template<class Τ> constexpr Τ αντίγραφο(Τ τιμή){ΕΠΙΣΤΡΟΦΗ(ΗΧΩ(τιμή));}
constexpr int αγνόησε(const γράμμα&){return 1;}
#endif
