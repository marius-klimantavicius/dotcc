// SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
#include "m1-unicode-storage-gate.h"
void \u03b7χώ(γράμμα&);
γράμμα κόσμος=u'\u00e9';
constexpr long τρέξε(){γράμμα τιμή=u'\u00e9'; const γράμμα& δεσμός=τιμή; int ηχώ=0; return \u03b1ντίγραφο(δεσμός)+αντίγραφο(2L);}
static_assert(τρέξε()==235);
static_assert(αντίγραφο(u'é')==233);
static_assert(αντίγραφο(3L)==3);
static_assert(true || κόσμος==233);
static_assert(false || αγνόησε(κόσμος)==1);
