//type:fp
//options:--c++11
//options_all:--no_il_lower --il_display
//filter:awk '/^file-scope (base-class-derivation)@/{f=1}/^$/{if (f) print $0; f=0}f' | grep -E '^($|file-scope |path:|  base_class:)' | sed -e 's/@[0-9a-f]*//'

template<int N> struct C : C<N-1> {};
template<> struct C<0> { };
C<3> c;
