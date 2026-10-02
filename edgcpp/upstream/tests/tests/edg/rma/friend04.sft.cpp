//options_all:-r -x -tused
//options: --strict;cp:;cp

class X {
  int a;
  friend void friend_set(X*, int);
public:
  void member_set(int);
  void static static_member_set(X*, int);
};
void friend_set(X* p, int i) { p->a = i; }
void X::member_set(int i) { a = i; }
void X::static_member_set(X* p, int i) { p->a = i; }
void f() {
  X x, y, z;
  friend_set(&x, 1);
  y.member_set(2);
  z.static_member_set(&z, 3);
}

