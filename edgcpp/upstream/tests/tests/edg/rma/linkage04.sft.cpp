//options_all:-r -x -tused
//options: --strict;cn:;cn

// linkage respecification errors
extern "C" int f();
extern "C++" int f();  // error
extern int f();
extern "C" int i;
extern "C++" int i;    // not a function ==> not an error
extern int i;
extern int g();
extern "C" int g();    // error
extern int g();
extern int j;
extern "C" int j;      // not a function ==> not an error
extern int j;

extern "C" {
  int ff();
  int ii;
}
extern "C++" {
  int ff();            // error
  int ii;              // not a function ==> not an error
}
extern int ff();
extern int ii;
extern int gg();
extern int jj;
extern "C" {
  int gg();            // error
  int jj;              // not a function ==> not an error
}
extern int gg();
extern int jj;

