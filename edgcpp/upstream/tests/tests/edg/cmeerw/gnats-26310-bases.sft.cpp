//type:fp
//options:--c++11
//options_all:--no_il_lower --il_display
//filter:awk '/^file-scope (base-class|class-type-supplement)@/{f=1}/^$/{if (f) print $0; f=0}f' | grep -E '^($|file-scope |next:|next_direct:|next_preorder:|type:|derived_class:|direct:|direct_base_number:|is_virtual:|base_classes:|direct_base_classes:|preorder_base_classes:)' | sed -e 's/@[0-9a-f]*//'

struct VB { };
struct V : VB { };
struct B : virtual V { };
struct C1 { };
struct C2 { };
struct D : virtual B, C1, virtual V, C2 { };
