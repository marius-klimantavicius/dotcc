//type: fp
//options: 
# 0 "./warn/anonymous-namespace-1.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./warn/anonymous-namespace-1.C"



# 1 "./warn/anonymous-namespace-1.h" 1
class foo {
 class bar;
};

namespace {
  class bad { };
}
# 5 "./warn/anonymous-namespace-1.C" 2

namespace {
 class good { };
}

class foo::bar : public good { };
class foobar1
{
  good g;
};
# 17 "foo.C"
class foobar : public bad { };
class foobar2 { bad b; };
