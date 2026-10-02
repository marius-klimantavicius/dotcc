//options_all:-r -x -tused
//options: --strict;cn

struct A { operator int(); };
int f(int);
extern int i;
int main() {
  i = i ? new struct A : f(i);
}

