//options_all:-r -x -tused
//options: --strict;cp

struct S { S(); S(int); ~S(); };
S s1[2][2] = { 1 };                     // Fixed
S s2[2][2] = { 1,1 };                   // Fixed
S s3[2][2] = { 1,1,1 };                 // Fixed
S s4[2][2] = { { 1 }, { 1 } };
S s5[2][2] = { { 1,1 }, { 1 } };
S s6[2][2] = { { 1 }, { 1,1 } };
S s7[2][2] = { { 1 } };                 // Fixed
S s8[2][2] = { { 1,1 } };               // Fixed

