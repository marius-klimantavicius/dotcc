// SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
// Authored likely-positive timing control, pending native observation.
// Native strict C++20 LP64 plus --unicode_source_kind UTF-8.
#if 0
\uD800 \u00G0 \u0041
#if \UFFFFFFFF
#endif
#endif
#define UNUSED U'\U00110000'
#define DROP(x) 1
#define STR(x) #x
int value=DROP(U'\U00110000');
int argument=DROP(\uD800);
const char* text=STR(\uD800);
#if 1
int selected=1;
#elif \uD800
#error wrong branch
#endif
