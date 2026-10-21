#define A é
#define B \u00E9
#define S(x) #x
#define E(x) S(x)
#define ID(x) x
S(é)
S(\u00E9)
E(é)
E(\u00E9)
E(A)
E(B)
S(ID(é))
E(ID(é))
