// SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
#define S(x) #x
constexpr const char* literal=S("\u00E9");
static_assert(sizeof(S("\u00E9"))==9);
static_assert(literal[0]=='"' && literal[1]=='\\' && literal[2]=='u' && literal[7]=='"');
constexpr const char* character=S(U'\U00010400');
static_assert(sizeof(S(U'\U00010400'))==14);
static_assert(character[0]=='U' && character[1]=='\'' && character[2]=='\\' && character[3]=='U' && character[12]=='\'');
