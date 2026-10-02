//options_all:-r -x -tused
//options: --strict;cp

/*ARGSUSED*/
void a(int i);

void b(int i) { }

void a(int i) { }

/*ARGSUSED*/
void c(int i) { }

/*ARGSUSED*/
class A {
  void a(int i) { }
  /*ARGSUSED*/
  void b(int i) { }
  /*ARGSUSED*/
  void c(int i);
};
/*ARGSUSED*/
void A::c(int i) { }

