//type: fp
//options: 
# 0 "./compat/scalar-return-4_x.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./compat/scalar-return-4_x.c"
# 1 "./compat/compat-common.h" 1
# 53 "./compat/compat-common.h"
extern void abort (void);

extern int fails;
# 2 "./compat/scalar-return-4_x.c" 2




const int test_va = 1;
# 88 "./compat/scalar-return-4_x.c"
_Complex char g01cc, g02cc, g03cc, g04cc; _Complex char g05cc, g06cc, g07cc, g08cc; _Complex char g09cc, g10cc, g11cc, g12cc; _Complex char g13cc, g14cc, g15cc, g16cc; extern void initcc (_Complex char *p, _Complex char v); extern void checkgcc (void); extern _Complex char test0cc (void); extern _Complex char test1cc (_Complex char); extern _Complex char testvacc (int n, ...); void checkcc (_Complex char x, _Complex char v) { if (x != v) abort (); } void testitcc (void) { _Complex char rslt; ; ; initcc (&g01cc, 1); initcc (&g02cc, 2); initcc (&g03cc, 3); initcc (&g04cc, 4); initcc (&g05cc, 5); initcc (&g06cc, 6); initcc (&g07cc, 7); initcc (&g08cc, 8); initcc (&g09cc, 9); initcc (&g10cc, 10); initcc (&g11cc, 11); initcc (&g12cc, 12); initcc (&g13cc, 13); initcc (&g14cc, 14); initcc (&g15cc, 15); initcc (&g16cc, 16); checkgcc (); ; ; ; rslt = test0cc (); checkcc (rslt, g01cc); ; ; ; rslt = test1cc (g01cc); checkcc (rslt, g01cc); if (test_va) { ; ; ; rslt = testvacc (1, g01cc); checkcc (rslt, g01cc); rslt = testvacc (5, g01cc, g02cc, g03cc, g04cc, g05cc); checkcc (rslt, g05cc); rslt = testvacc (9, g01cc, g02cc, g03cc, g04cc, g05cc, g06cc, g07cc, g08cc, g09cc); checkcc (rslt, g09cc); rslt = testvacc (16, g01cc, g02cc, g03cc, g04cc, g05cc, g06cc, g07cc, g08cc, g09cc, g10cc, g11cc, g12cc, g13cc, g14cc, g15cc, g16cc); checkcc (rslt, g16cc); } ; }
_Complex short g01cs, g02cs, g03cs, g04cs; _Complex short g05cs, g06cs, g07cs, g08cs; _Complex short g09cs, g10cs, g11cs, g12cs; _Complex short g13cs, g14cs, g15cs, g16cs; extern void initcs (_Complex short *p, _Complex short v); extern void checkgcs (void); extern _Complex short test0cs (void); extern _Complex short test1cs (_Complex short); extern _Complex short testvacs (int n, ...); void checkcs (_Complex short x, _Complex short v) { if (x != v) abort (); } void testitcs (void) { _Complex short rslt; ; ; initcs (&g01cs, 1); initcs (&g02cs, 2); initcs (&g03cs, 3); initcs (&g04cs, 4); initcs (&g05cs, 5); initcs (&g06cs, 6); initcs (&g07cs, 7); initcs (&g08cs, 8); initcs (&g09cs, 9); initcs (&g10cs, 10); initcs (&g11cs, 11); initcs (&g12cs, 12); initcs (&g13cs, 13); initcs (&g14cs, 14); initcs (&g15cs, 15); initcs (&g16cs, 16); checkgcs (); ; ; ; rslt = test0cs (); checkcs (rslt, g01cs); ; ; ; rslt = test1cs (g01cs); checkcs (rslt, g01cs); if (test_va) { ; ; ; rslt = testvacs (1, g01cs); checkcs (rslt, g01cs); rslt = testvacs (5, g01cs, g02cs, g03cs, g04cs, g05cs); checkcs (rslt, g05cs); rslt = testvacs (9, g01cs, g02cs, g03cs, g04cs, g05cs, g06cs, g07cs, g08cs, g09cs); checkcs (rslt, g09cs); rslt = testvacs (16, g01cs, g02cs, g03cs, g04cs, g05cs, g06cs, g07cs, g08cs, g09cs, g10cs, g11cs, g12cs, g13cs, g14cs, g15cs, g16cs); checkcs (rslt, g16cs); } ; }

_Complex float g01cf, g02cf, g03cf, g04cf; _Complex float g05cf, g06cf, g07cf, g08cf; _Complex float g09cf, g10cf, g11cf, g12cf; _Complex float g13cf, g14cf, g15cf, g16cf; extern void initcf (_Complex float *p, _Complex float v); extern void checkgcf (void); extern _Complex float test0cf (void); extern _Complex float test1cf (_Complex float); extern _Complex float testvacf (int n, ...); void checkcf (_Complex float x, _Complex float v) { if (x != v) abort (); } void testitcf (void) { _Complex float rslt; ; ; initcf (&g01cf, 1); initcf (&g02cf, 2); initcf (&g03cf, 3); initcf (&g04cf, 4); initcf (&g05cf, 5); initcf (&g06cf, 6); initcf (&g07cf, 7); initcf (&g08cf, 8); initcf (&g09cf, 9); initcf (&g10cf, 10); initcf (&g11cf, 11); initcf (&g12cf, 12); initcf (&g13cf, 13); initcf (&g14cf, 14); initcf (&g15cf, 15); initcf (&g16cf, 16); checkgcf (); ; ; ; rslt = test0cf (); checkcf (rslt, g01cf); ; ; ; rslt = test1cf (g01cf); checkcf (rslt, g01cf); if (test_va) { ; ; ; rslt = testvacf (1, g01cf); checkcf (rslt, g01cf); rslt = testvacf (5, g01cf, g02cf, g03cf, g04cf, g05cf); checkcf (rslt, g05cf); rslt = testvacf (9, g01cf, g02cf, g03cf, g04cf, g05cf, g06cf, g07cf, g08cf, g09cf); checkcf (rslt, g09cf); rslt = testvacf (16, g01cf, g02cf, g03cf, g04cf, g05cf, g06cf, g07cf, g08cf, g09cf, g10cf, g11cf, g12cf, g13cf, g14cf, g15cf, g16cf); checkcf (rslt, g16cf); } ; }




void
scalar_return_4_x ()
{






testitcc ();
testitcs ();

testitcf ();




if (fails != 0)
  abort ();


}
