//options_all:-r -x -tused
//options: --strict;cn:;cp

//int gbl_callee1 (int, int, int);
//int gbl_sink1 = gbl_callee1 (77, 88, 99);
//int gbl_callee1 (int, int, int = 3);			/* OK */
//int gbl_sink2 = gbl_callee1 (77, 88);			/* OK */
//int gbl_callee1 (int, int = 2, int);			/* OK */
//int gbl_sink3 = gbl_callee1 (77);			/* OK */
//int gbl_callee1 (int = 1, int, int);			/* OK */
//int gbl_sink4 = gbl_callee1 ();				/* OK */

struct S1 { friend int gbl_callee2 (int, int, int); };
int gbl_sink5 = gbl_callee2 (77, 88, 99);
struct S2 { friend int gbl_callee2 (int, int, int = 3); };	/* OK */
int gbl_sink6 = gbl_callee2 (77, 88);				/* OK */
struct S3 { friend int gbl_callee2 (int, int = 2, int); };	/* OK */


