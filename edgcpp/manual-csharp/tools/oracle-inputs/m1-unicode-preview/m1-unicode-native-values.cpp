// SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
// Authored native type/value witness proposal; no native observations claimed.
// Native strict C++20 LP64 plus TWO arguments: --unicode_source_kind UTF-8.
constexpr int kind(char){return 1;}
constexpr int kind(char8_t){return 2;}
constexpr int kind(char16_t){return 3;}
constexpr int kind(char32_t){return 4;}
constexpr int kind(wchar_t){return 5;}
static_assert(kind('\u0041')==1 && '\u0041'==65);
static_assert(kind(u8'\xff')==2 && u8'\xff'==255);
static_assert(kind(u'é')==3 && u'é'==233);
static_assert(kind(u'\u00e9')==3 && u'\u00e9'==233);
static_assert(kind(U'𐐀')==4 && U'𐐀'==66560);
static_assert(kind(U'\U00010400')==4 && U'\U00010400'==66560);
static_assert(kind(L'𐐀')==5 && L'𐐀'==66560);
static_assert(u'\xd800'==55296); // Numeric code unit, not a scalar UCN.
static_assert(U'\xffffffff'==4294967295U); // Numeric escape remains a target code unit.
int é=1;
int read(){return \u00E9;}
