// SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
// Authored coupling of target character types, references and cached templates.
#define UTF8_VALUE u8'A'
char ordinary='A';
char& ordinary_alias=ordinary;
char16_t unit=u'A';
wchar_t wide=L'A';
wchar_t& wide_alias=wide;
using UnitReference=char16_t&;
char16_t& left(char16_t& value){return value;}
UnitReference bound=left(unit);
char16_t& relayed(){return left(unit);}
char16_t&& expiring();
const char16_t& expiring_alias=expiring();
const char8_t& materialized=UTF8_VALUE;
int promotion(const unsigned int&);
long promotion(const int&);
int promoted(){return promotion(U'A');}
int exact(const char32_t&);
long exact(const unsigned int&);
int exact_call(){return exact(U'A');}
int choose(const char16_t&);
long choose(const wchar_t&);
template<class T> int relay(T value){return choose(value);}
int choose(char16_t&);
int invoke(){int choose=0; return relay(u'A');}
int repeated(){return relay(u'B');}
long wide_call(){return choose(wide);}
int later_call(){return choose(unit);}
