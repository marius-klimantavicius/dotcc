// SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
// Authored PP, character, reference storage and frozen template replay gate.
#define RET(value) return value
constexpr const char16_t& echo(const char16_t& value){RET(value);}
constexpr const long& echo(const long& value){RET(value);}
template<class T> constexpr T copy(T value){RET(echo(value));}
void echo(char16_t&);
constexpr long run(){char16_t value=u'A'; const char16_t& alias=value; int echo=0; return copy(alias)+copy(2L);}
static_assert(run()==67);
static_assert(copy(u'B')==66);
static_assert(copy(3L)==3);
