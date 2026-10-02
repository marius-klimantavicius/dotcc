//type:fp
//options::-DNEG;fc
//options_all:--modules

module X:Part;

#if NEG
import foo;
export import bar;
#endif

void
import_function() {}

void
import1() {}
