//type:fp
//options::-DNEG
//options_all:--modules -E

module X;

#if NEG
import foo;
export import bar;
#endif

void
import_function() {}

void
import1() {}
