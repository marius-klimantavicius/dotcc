//options_all:-r -x -tused
//options: --strict;cp

struct Foo {
      char * base();
      int bar(char);
} ;

int Foo::bar(char c) {
      enum { OCT, DEC, HEX } base;
      base = DEC;
      if(c == 'o') base = OCT;
      else if(c == 'x') base = HEX;
      return (int)base;
}

