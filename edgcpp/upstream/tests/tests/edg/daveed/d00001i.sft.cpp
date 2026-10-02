/*
//remark:Extended designators
//type:fn
//name:
//options:
//options_all:--c --extended_designators
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:
*/

/* GNUisms */

/* egcs generates almost random code when dealing with nested designated
   initializer of object that have static lifetime! gcc 2.8.1 is a bit
   better. */

typedef struct X {
  int a, b, c[10];
} X;

typedef struct Y {
	X p, q, r;
} Y;

/* gcc accepts both .id and id: as designators.
   It also does not require an '=' after a [ <expr> ] designator; however,
   it does require the '=' after a [ <expr> ... <expr> ] designator.
 */
#if !defined NODOTS
Y p0 = { q: { b:2, c: { [4] 3} },
         .r = { .a = 21, .c = { [0 ... 9] = 7 }  } };
Y p1 = { .p = { .b = 99 },
         q: { b:2, c: { [4] 3} },
         .r = { .a = 21, .c = { [0 ... 9] = 7 }  } };
#endif

/* gcc only keeps the last of multiple consecutive id: designators.
   The last one can also be a .id designator */
X p2 = { a:b: 4, a:.a = 5 };

/* A similar situation occurs with [] designators: */
#if !defined NODOTS
X p3 = { .c = { [1][1][1][1][1] 13, [2][2][2][2][2] = 27,
                [3 ... 4] = [5] 7, [6][7] = 71, [8] = [9] = 73 } };
#endif

#if !defined(RUNTEST) /* Negative tests */
/* gcc does not accept multiple designator designations. */

X n1 = { .c[5] = 42 };
Y n2 = { .q.b = 42 };
Y n3 = { p:a: 42 };
Y n4 = { p:.a = 42 };

#endif

/* Test run-time: */

extern int printf(char const*, ...);


void printX(X *p) {
	int k;
	printf("{ a: %d, b: %d, c:{ ", p->a, p->b);
	for (k = 0; k<10; ++k) { printf("%d ", p->c[k]); }
	printf("}}\n");
}


void printY(Y *p) {
	printf("{ p:");
	printX(&p->p);
	printf("  q:");
	printX(&p->q);
	printf("  r:");
	printX(&p->r);
	printf("}\n");
}


int main() {
	X r2 = { a:b: 4, a:.a = 5 };
#if !defined NODOTS
	Y r1 = { .p = { .a = 99 },
            q: { b:2, c: { [4] 3} },
	         .r = { .a = 21, .c = { [0 ... 9] = 7 }  } };
	X r3 = { .c = { [1][1][1][1][1] 13, [2][2][2][2][2] = 27,
	                [3 ... 4] = [5] 7, [6][7] = 71, [8] = [9] = 73 } };
	printf("p0 = \n");
	printY(&p0);
	printf("p1 = \n");
	printY(&p1);
	printf("r1 = \n");
	printY(&r1);
#endif

	printf("p2 = \n");
	printX(&p2);
	printf("r2 = \n");
	printX(&r2);

#if !defined NODOTS
	printf("p3 = \n");
	printX(&p3);
	printf("r3 = \n");
	printX(&r3);
#endif
}

