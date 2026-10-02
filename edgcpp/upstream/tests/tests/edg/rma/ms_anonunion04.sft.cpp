//options_all:-r --microsoft_version=1400 --set_flag=no_checking_pragmas
//type:rp

class LEWORD {
    unsigned short val;

public:
    operator unsigned short () const { return val; }
};

typedef struct MSOSTT {
    LEWORD w1;
    LEWORD w2;
    LEWORD w3;
    union {
        struct {
            LEWORD wsub11;
            LEWORD wsub12;
        };
        struct {
            LEWORD wsub11;
            LEWORD wsub12;
        };
    };
} MSOSTT;

int foo(void *p) {
    return ((MSOSTT*)p)->wsub11;
}

extern "C" int printf(char *, ...);
main() {
  struct S { short a,b,c,d,e; } xxx = { 1,2,3,4,5 };
  int i = foo(&xxx);
  printf("%d\n", i);
}

