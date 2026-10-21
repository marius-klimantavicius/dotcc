#define A x
#define F() x
#define M x+y
#define S(x) #x
#define E(x) S(x)
object E(a+A)
function E(a+F())
multi E(a+M)
spaced E(a+ A)
#define EMPTY
empty_before E(a EMPTY+b)
empty_after E(a+EMPTY b)
empty_adjacent E(a+EMPTY+b)
