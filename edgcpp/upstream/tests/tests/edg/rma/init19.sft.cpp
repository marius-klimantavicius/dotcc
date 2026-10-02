//options_all:-r -x -tused
//options: --strict;cn

class X { };
X a[] = {X()};          // error
X b[] = {0};            // error
X c[] = {};             // error
X d[] = {{}};           // okay -- declares d[1]
X dd[] = {{},{}};       // okay -- declares dd[2]
X ddx[] = {{}, 0};      // error
X e = {};               // okay
X f = X();              // error -- no constructor

