//type: fp
//options: 
# 0 "./compat/scalar-by-value-4_x.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./compat/scalar-by-value-4_x.c"
# 1 "./compat/compat-common.h" 1
# 53 "./compat/compat-common.h"
extern void abort (void);

extern int fails;
# 2 "./compat/scalar-by-value-4_x.c" 2
# 149 "./compat/scalar-by-value-4_x.c"
_Complex char g01cc, g02cc, g03cc, g04cc; _Complex char g05cc, g06cc, g07cc, g08cc; _Complex char g09cc, g10cc, g11cc, g12cc; _Complex char g13cc, g14cc, g15cc, g16cc; extern void initcc (_Complex char *p, _Complex char v); extern void checkgcc (void); extern void testcc (_Complex char x01, _Complex char x02, _Complex char x03, _Complex char x04, _Complex char x05, _Complex char x06, _Complex char x07, _Complex char x08, _Complex char x09, _Complex char x10, _Complex char x11, _Complex char x12, _Complex char x13, _Complex char x14, _Complex char x15, _Complex char x16); extern void testvacc (int n, ...); void checkcc (_Complex char x, _Complex char v) { if (x != v + (0 + 1 * __extension__ 1i)) abort (); } void test2_cc (_Complex char x01, _Complex char x02, _Complex char x03, _Complex char x04, _Complex char x05, _Complex char x06, _Complex char x07, _Complex char x08) { testcc (x01, g02cc, x02, g04cc, x03, g06cc, x04, g08cc, x05, g10cc, x06, g12cc, x07, g14cc, x08, g16cc); } void testitcc (void) { ; ; initcc (&g01cc, 1); initcc (&g02cc, 2); initcc (&g03cc, 3); initcc (&g04cc, 4); initcc (&g05cc, 5); initcc (&g06cc, 6); initcc (&g07cc, 7); initcc (&g08cc, 8); initcc (&g09cc, 9); initcc (&g10cc, 10); initcc (&g11cc, 11); initcc (&g12cc, 12); initcc (&g13cc, 13); initcc (&g14cc, 14); initcc (&g15cc, 15); initcc (&g16cc, 16); checkgcc (); ; ; ; testcc (g01cc, g02cc, g03cc, g04cc, g05cc, g06cc, g07cc, g08cc, g09cc, g10cc, g11cc, g12cc, g13cc, g14cc, g15cc, g16cc); ; ; ; ; testvacc (1, g01cc); ; testvacc (2, g01cc, g02cc); ; testvacc (3, g01cc, g02cc, g03cc); ; testvacc (4, g01cc, g02cc, g03cc, g04cc); ; testvacc (5, g01cc, g02cc, g03cc, g04cc, g05cc); ; testvacc (6, g01cc, g02cc, g03cc, g04cc, g05cc, g06cc); ; testvacc (7, g01cc, g02cc, g03cc, g04cc, g05cc, g06cc, g07cc); ; testvacc (8, g01cc, g02cc, g03cc, g04cc, g05cc, g06cc, g07cc, g08cc); ; testvacc (9, g01cc, g02cc, g03cc, g04cc, g05cc, g06cc, g07cc, g08cc, g09cc); ; testvacc (10, g01cc, g02cc, g03cc, g04cc, g05cc, g06cc, g07cc, g08cc, g09cc, g10cc); ; testvacc (11, g01cc, g02cc, g03cc, g04cc, g05cc, g06cc, g07cc, g08cc, g09cc, g10cc, g11cc); ; testvacc (12, g01cc, g02cc, g03cc, g04cc, g05cc, g06cc, g07cc, g08cc, g09cc, g10cc, g11cc, g12cc); ; testvacc (13, g01cc, g02cc, g03cc, g04cc, g05cc, g06cc, g07cc, g08cc, g09cc, g10cc, g11cc, g12cc, g13cc); ; testvacc (14, g01cc, g02cc, g03cc, g04cc, g05cc, g06cc, g07cc, g08cc, g09cc, g10cc, g11cc, g12cc, g13cc, g14cc); ; testvacc (15, g01cc, g02cc, g03cc, g04cc, g05cc, g06cc, g07cc, g08cc, g09cc, g10cc, g11cc, g12cc, g13cc, g14cc, g15cc); ; testvacc (16, g01cc, g02cc, g03cc, g04cc, g05cc, g06cc, g07cc, g08cc, g09cc, g10cc, g11cc, g12cc, g13cc, g14cc, g15cc, g16cc); ; ; ; test2_cc (g01cc, g03cc, g05cc, g07cc, g09cc, g11cc, g13cc, g15cc); ; }
_Complex short g01cs, g02cs, g03cs, g04cs; _Complex short g05cs, g06cs, g07cs, g08cs; _Complex short g09cs, g10cs, g11cs, g12cs; _Complex short g13cs, g14cs, g15cs, g16cs; extern void initcs (_Complex short *p, _Complex short v); extern void checkgcs (void); extern void testcs (_Complex short x01, _Complex short x02, _Complex short x03, _Complex short x04, _Complex short x05, _Complex short x06, _Complex short x07, _Complex short x08, _Complex short x09, _Complex short x10, _Complex short x11, _Complex short x12, _Complex short x13, _Complex short x14, _Complex short x15, _Complex short x16); extern void testvacs (int n, ...); void checkcs (_Complex short x, _Complex short v) { if (x != v + (1 + 2 * __extension__ 1i)) abort (); } void test2_cs (_Complex short x01, _Complex short x02, _Complex short x03, _Complex short x04, _Complex short x05, _Complex short x06, _Complex short x07, _Complex short x08) { testcs (x01, g02cs, x02, g04cs, x03, g06cs, x04, g08cs, x05, g10cs, x06, g12cs, x07, g14cs, x08, g16cs); } void testitcs (void) { ; ; initcs (&g01cs, 1); initcs (&g02cs, 2); initcs (&g03cs, 3); initcs (&g04cs, 4); initcs (&g05cs, 5); initcs (&g06cs, 6); initcs (&g07cs, 7); initcs (&g08cs, 8); initcs (&g09cs, 9); initcs (&g10cs, 10); initcs (&g11cs, 11); initcs (&g12cs, 12); initcs (&g13cs, 13); initcs (&g14cs, 14); initcs (&g15cs, 15); initcs (&g16cs, 16); checkgcs (); ; ; ; testcs (g01cs, g02cs, g03cs, g04cs, g05cs, g06cs, g07cs, g08cs, g09cs, g10cs, g11cs, g12cs, g13cs, g14cs, g15cs, g16cs); ; ; ; ; testvacs (1, g01cs); ; testvacs (2, g01cs, g02cs); ; testvacs (3, g01cs, g02cs, g03cs); ; testvacs (4, g01cs, g02cs, g03cs, g04cs); ; testvacs (5, g01cs, g02cs, g03cs, g04cs, g05cs); ; testvacs (6, g01cs, g02cs, g03cs, g04cs, g05cs, g06cs); ; testvacs (7, g01cs, g02cs, g03cs, g04cs, g05cs, g06cs, g07cs); ; testvacs (8, g01cs, g02cs, g03cs, g04cs, g05cs, g06cs, g07cs, g08cs); ; testvacs (9, g01cs, g02cs, g03cs, g04cs, g05cs, g06cs, g07cs, g08cs, g09cs); ; testvacs (10, g01cs, g02cs, g03cs, g04cs, g05cs, g06cs, g07cs, g08cs, g09cs, g10cs); ; testvacs (11, g01cs, g02cs, g03cs, g04cs, g05cs, g06cs, g07cs, g08cs, g09cs, g10cs, g11cs); ; testvacs (12, g01cs, g02cs, g03cs, g04cs, g05cs, g06cs, g07cs, g08cs, g09cs, g10cs, g11cs, g12cs); ; testvacs (13, g01cs, g02cs, g03cs, g04cs, g05cs, g06cs, g07cs, g08cs, g09cs, g10cs, g11cs, g12cs, g13cs); ; testvacs (14, g01cs, g02cs, g03cs, g04cs, g05cs, g06cs, g07cs, g08cs, g09cs, g10cs, g11cs, g12cs, g13cs, g14cs); ; testvacs (15, g01cs, g02cs, g03cs, g04cs, g05cs, g06cs, g07cs, g08cs, g09cs, g10cs, g11cs, g12cs, g13cs, g14cs, g15cs); ; testvacs (16, g01cs, g02cs, g03cs, g04cs, g05cs, g06cs, g07cs, g08cs, g09cs, g10cs, g11cs, g12cs, g13cs, g14cs, g15cs, g16cs); ; ; ; test2_cs (g01cs, g03cs, g05cs, g07cs, g09cs, g11cs, g13cs, g15cs); ; }

_Complex float g01cf, g02cf, g03cf, g04cf; _Complex float g05cf, g06cf, g07cf, g08cf; _Complex float g09cf, g10cf, g11cf, g12cf; _Complex float g13cf, g14cf, g15cf, g16cf; extern void initcf (_Complex float *p, _Complex float v); extern void checkgcf (void); extern void testcf (_Complex float x01, _Complex float x02, _Complex float x03, _Complex float x04, _Complex float x05, _Complex float x06, _Complex float x07, _Complex float x08, _Complex float x09, _Complex float x10, _Complex float x11, _Complex float x12, _Complex float x13, _Complex float x14, _Complex float x15, _Complex float x16); extern void testvacf (int n, ...); void checkcf (_Complex float x, _Complex float v) { if (x != v + (6.0 + 7.0 * __extension__ 1.0i)) abort (); } void test2_cf (_Complex float x01, _Complex float x02, _Complex float x03, _Complex float x04, _Complex float x05, _Complex float x06, _Complex float x07, _Complex float x08) { testcf (x01, g02cf, x02, g04cf, x03, g06cf, x04, g08cf, x05, g10cf, x06, g12cf, x07, g14cf, x08, g16cf); } void testitcf (void) { ; ; initcf (&g01cf, 1); initcf (&g02cf, 2); initcf (&g03cf, 3); initcf (&g04cf, 4); initcf (&g05cf, 5); initcf (&g06cf, 6); initcf (&g07cf, 7); initcf (&g08cf, 8); initcf (&g09cf, 9); initcf (&g10cf, 10); initcf (&g11cf, 11); initcf (&g12cf, 12); initcf (&g13cf, 13); initcf (&g14cf, 14); initcf (&g15cf, 15); initcf (&g16cf, 16); checkgcf (); ; ; ; testcf (g01cf, g02cf, g03cf, g04cf, g05cf, g06cf, g07cf, g08cf, g09cf, g10cf, g11cf, g12cf, g13cf, g14cf, g15cf, g16cf); ; ; ; ; testvacf (1, g01cf); ; testvacf (2, g01cf, g02cf); ; testvacf (3, g01cf, g02cf, g03cf); ; testvacf (4, g01cf, g02cf, g03cf, g04cf); ; testvacf (5, g01cf, g02cf, g03cf, g04cf, g05cf); ; testvacf (6, g01cf, g02cf, g03cf, g04cf, g05cf, g06cf); ; testvacf (7, g01cf, g02cf, g03cf, g04cf, g05cf, g06cf, g07cf); ; testvacf (8, g01cf, g02cf, g03cf, g04cf, g05cf, g06cf, g07cf, g08cf); ; testvacf (9, g01cf, g02cf, g03cf, g04cf, g05cf, g06cf, g07cf, g08cf, g09cf); ; testvacf (10, g01cf, g02cf, g03cf, g04cf, g05cf, g06cf, g07cf, g08cf, g09cf, g10cf); ; testvacf (11, g01cf, g02cf, g03cf, g04cf, g05cf, g06cf, g07cf, g08cf, g09cf, g10cf, g11cf); ; testvacf (12, g01cf, g02cf, g03cf, g04cf, g05cf, g06cf, g07cf, g08cf, g09cf, g10cf, g11cf, g12cf); ; testvacf (13, g01cf, g02cf, g03cf, g04cf, g05cf, g06cf, g07cf, g08cf, g09cf, g10cf, g11cf, g12cf, g13cf); ; testvacf (14, g01cf, g02cf, g03cf, g04cf, g05cf, g06cf, g07cf, g08cf, g09cf, g10cf, g11cf, g12cf, g13cf, g14cf); ; testvacf (15, g01cf, g02cf, g03cf, g04cf, g05cf, g06cf, g07cf, g08cf, g09cf, g10cf, g11cf, g12cf, g13cf, g14cf, g15cf); ; testvacf (16, g01cf, g02cf, g03cf, g04cf, g05cf, g06cf, g07cf, g08cf, g09cf, g10cf, g11cf, g12cf, g13cf, g14cf, g15cf, g16cf); ; ; ; test2_cf (g01cf, g03cf, g05cf, g07cf, g09cf, g11cf, g13cf, g15cf); ; }




void
scalar_by_value_4_x ()
{






testitcc ();
testitcs ();

testitcf ();




if (fails != 0)
  abort ();


}
