//options_all:-r -x -tused
//options: --strict;cn

class Foo {
      int a;
    public:
      void dummy() ;
      int ::global(Foo * a) ;
} ;
int global(Foo *a);
int main()
{
  Foo foo;
  foo.dummy();
  global(&foo);
  foo.global(&foo);
}

