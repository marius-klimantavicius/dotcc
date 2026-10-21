// SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
// Authored native/managed proposal. Native requires --unicode_source_kind UTF-8.
#if __has_include("m1-unicode-gate.h") && u'\u00e9'==233 && U'𐐀'==66560
#include "m1-unicode-gate.h"
constexpr int διάλεξε(int χ){return χ+1;}
constexpr int \u03b4ιάλεξε(long χ){return χ+2;}
constexpr γράμμα αριθμός=u'\u00e9';
constexpr int τρέξε(){return διάλεξε(\u03afδιο(u'é'));}
static_assert(\u03c4ρέξε()==234);
static_assert(ίδιο(U'𐐀')==66560);
static_assert(αριθμός==233);
static_assert(\u03afδιο(u'\u00e9')==233);
#else
#error Unicode source and character profile mismatch
#endif
