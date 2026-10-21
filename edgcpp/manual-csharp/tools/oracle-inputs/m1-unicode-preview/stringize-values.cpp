// SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
#define S(x) #x
constexpr const char* escaped=S(\u00E9);
constexpr const char* direct=S(é);
static_assert(sizeof(S(\u00E9))==3 && sizeof(S(é))==3);
static_assert(static_cast<unsigned char>(escaped[0])==0xc3 && static_cast<unsigned char>(escaped[1])==0xa9);
static_assert(static_cast<unsigned char>(direct[0])==0xc3 && static_cast<unsigned char>(direct[1])==0xa9);
constexpr const char* supplementary=S(\U00010400);
static_assert(sizeof(S(\U00010400))==5);
static_assert(static_cast<unsigned char>(supplementary[0])==0xf0 && static_cast<unsigned char>(supplementary[1])==0x90);
static_assert(static_cast<unsigned char>(supplementary[2])==0x90 && static_cast<unsigned char>(supplementary[3])==0x80);
constexpr const char* literal=S("\u00E9");
static_assert(sizeof(S("\u00E9"))==9);
static_assert(literal[0]=='"' && literal[1]=='\\' && literal[2]=='u' && literal[7]=='"');
constexpr const char* character=S(U'\U00010400');
static_assert(sizeof(S(U'\U00010400'))==14);
static_assert(character[0]=='U' && character[1]=='\'' && character[2]=='\\' && character[3]=='U' && character[12]=='\'');
