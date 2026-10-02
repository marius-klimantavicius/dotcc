//options_all:-r -x -tused
//options: --strict;cp

class X {
  friend int main(int, char**);
  typedef int Y;
};
int main(int, char**) {
  X::Y y;
}



