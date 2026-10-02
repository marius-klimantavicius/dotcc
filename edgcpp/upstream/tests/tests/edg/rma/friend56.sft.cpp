//options_all:-r -x -tused
//options: --strict;cn:;cp

// EDGma00120 -- n1140901
void func()
{
     class Y {
          friend int f();  // error : no prior function f()
     };
}

