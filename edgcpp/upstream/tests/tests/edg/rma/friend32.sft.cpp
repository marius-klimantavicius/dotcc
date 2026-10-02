//options_all:-r -x -tused
//options: --strict;cn:--diag_warn=260;cp

class A {
        int foo();
      int foo(int);	// Enabling this causes a segmentation fault
        friend foo(A a);	// This declaration is ignored
};

