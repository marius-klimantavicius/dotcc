//options_all:-r -x -tused
//options: --cfront_3.0;cp

// cfront layout compatibility test
//
//        V
//         \
//          A   B
//         / \ /
//        C   D
//         \ /
//          Y
//
//  Here's how cfront lays it out:
//
//    struct A {    /* sizeof A == 12 */
//      int a__1A ;
//      struct V *PV;
//      struct V OV;
//    };
//    struct B {    /* sizeof B == 4 */
//      int b__1B ;
//    };
//    struct C {    /* sizeof C == 24 */
//      int c__1C ;
//      struct A *PA;
//      struct V *PV;
//      struct A OA;
//    };
//    struct D {    /* sizeof D == 32 */
//      int d__1D ;
//      struct A *PA;
//      struct V *PV;
//      struct B *PB;
//      struct A OA;
//      struct B OB;
//    };
//    struct Y {    /* sizeof Y == 56 */
//      int c__1C ;
//      struct A *PA;
//      struct V *PV;
//      int y__1Y ;
//      struct D *PD;
//      struct B *PB;
//      struct D OD;
//    };

struct V { int v; };
struct A : virtual V { int a; };
struct B { int b; };
struct C : virtual A { int c; };
struct D : virtual A, virtual B { int d; };
struct Y : C, virtual D { int y; };

