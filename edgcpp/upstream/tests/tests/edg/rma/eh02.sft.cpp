//options_all:-r -x -tused
//options: --strict;cn:--diag_warn=260;cn

class A { };
class B1 : private A { };
class C1 : public B1 { };
class B2 : public A { };
class C2 : public B2 { };
void f() { };
void g() {
  try {
    f();
  }
  catch (int) { }
  catch (int) { }    // Error
  catch (A) { }
  catch (C1) { }     // Error? (not masked by A, which is private)
  catch (B1) { }     // Error? (not masked by A, which is private)
  catch (C2) { }     // Error (masked by A)
  catch (B2) { }     // Error (masked by A)
  catch (...) { }
  catch (...) { }    // Error
  catch (int*) { }   // Error
  try {
    f();
  }
  catch (A*) { }
  catch (C1*) { }     // Error? (not masked by A, which is private)
  catch (B1*) { }     // Error? (not masked by A, which is private)
  catch (C2*) { }     // Error (masked by A)
  catch (B2*) { }     // Error (masked by A)
  try {
    f();
  }
  catch (A&) { }
  catch (C1&) { }     // Error? (not masked by A, which is private)
  catch (B1&) { }     // Error? (not masked by A, which is private)
  catch (C2&) { }     // Error (masked by A)
  catch (B2&) { }     // Error (masked by A)
  typedef const int I;
  typedef const unsigned int UI;
  typedef const long L;
  try {
    f();
  }
  catch (const int&) { }
  catch (int&) { }            // Error
  catch (int) { }             // Error
  catch (const) { }           // Error
  catch (I) { }               // Error
  catch (volatile I) { }      // Error
  catch (volatile UI) { }
  catch (unsigned int&) { }   // Error
  catch (unsigned) { }        // Error
  catch (L) { }
}

