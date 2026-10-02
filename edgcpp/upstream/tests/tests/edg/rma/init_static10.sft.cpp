//options_all:-r -x -tused
//options: --strict;cn:;cn

struct { static int i; } x;
typedef struct { static int i; } S;
S y;
int S::i = 0;
main() {
  x.i = 0;
  y.i = 0;
}

