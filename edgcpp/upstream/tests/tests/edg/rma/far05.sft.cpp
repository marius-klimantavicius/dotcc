//options_all:-r -x -tused
//options: --strict;cn

// Error checking on declarator scanning of near/far
typedef int I;
typedef int near NI;
typedef int far FI;
I near ni;
I far fi;
I i;
NI near *nip;
NI far *fip;
NI *ip;
NI near ni2;
NI far fi2;
NI i2;
FI near ni3;
FI far fi3;
FI i3;
FI (near x);
FI (near *xx);
FI (near xxx());
FI (near *a1)[5];
FI (near *a1b)[5][3];
FI ((near *a1c)[5])[3];
FI (near a2)[5];
FI (far *a3)[5];
FI (far a4)[5];
FI (far *a5)();
FI (far a6)();

