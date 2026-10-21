// SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
// Authored mixed native-only proposal. NOT a positive acceptance gate:
// supervisor split controls DISPROVED the ordinary Unicode string-value
// assertions below, even with Unicode source input. Retain those assumptions
// visibly; string spelling and ASCII literal-token data witnesses are separate.
// Native strict C++20 LP64 plus --unicode_source_kind UTF-8.
#define STR(x) #x
#define CAT(a,b) a##b
#define é_tail 7
#define φ(α) \u03b1
#define \u03c6(\u03b1) α
constexpr const char* escaped=STR(\u03b1);
constexpr const char* direct=STR(α);
constexpr const char* literal=STR("\u03b1");
static_assert((unsigned char)escaped[0]==206 && (unsigned char)escaped[1]==177 && escaped[2]==0);
static_assert((unsigned char)direct[0]==206 && (unsigned char)direct[1]==177 && direct[2]==0);
static_assert(literal[0]=='"' && literal[1]=='\\' && literal[2]=='u' && literal[7]=='"' && literal[8]==0);
static_assert(CAT(\u00e9,_tail)==7);
static_assert(\u03c6(8)==8);
#if !defined(\u00e9_tail) || !defined(φ)
#error canonical macro name identity mismatch
#endif
