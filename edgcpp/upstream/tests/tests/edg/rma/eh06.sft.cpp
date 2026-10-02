//options_all:-r -x -tused
//options: --strict;cn

// Interaction between "throw anything" and "throw nothing"
void a();
void a() throw ();                        // Error
void b() throw ();
void b();                                 // Error

// Interaction between "throw anything" and "throw one thing"
void c();
void c() throw (int);                     // Error
void d() throw (int);
void d();                                 // Error

// Interaction between "throw nothing" and "throw one thing"
void e() throw ();
void e() throw (int);                     // Error
void f() throw (int);
void f() throw ();                        // Error

// Mismatched lists
void g() throw (int,char);
void g() throw (int);                     // Error
void g() throw (int,char,float);          // Error
void g() throw (float,double);            // Error

// Redundancies
void h() throw (int,char,char,int);
void h() throw (char,int);
void h() throw (char,char,int);
void h() throw (char,char,char);          // Error
typedef int I;
typedef I II;
void i() throw (int,I,II);
void i() throw (I);
void i() throw (char);                    // Error

