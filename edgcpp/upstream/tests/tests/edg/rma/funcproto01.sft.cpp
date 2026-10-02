//options_all:-r -x -tused
//options: --strict;rp

int f(int i);
int f(int i) { return i; }
int f(int i);
int g(int i) { return i; }
int g(int i);
int main() {
  extern int f(int i);
  extern int g(int i);
  extern int h(int i);
}
int h(int i) { return i; }

