#define A é
#define S(x) #x
#define E(x) S(x)
#define EE(x) E(x)
constexpr const char* text=EE(E(A));
static_assert(sizeof(EE(E(A)))==9);
static_assert(text[0]=='"' && text[1]=='\\' && text[2]=='u' && text[5]=='e' && text[6]=='9' && text[7]=='"');
