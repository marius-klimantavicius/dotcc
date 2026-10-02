//options_all:-r -x -tused
//options: --strict;cp

struct VA1 { int va; };
struct VA2 { int va1; };
struct VA3 { int va2; };
struct AA : virtual VA1, virtual VA2, virtual VA3 { int aa; };
struct A : AA { int a; };
struct VB1 { int vb; };
struct VB2 { int vb2; };
struct VB3 { int vb3; };
struct VB : virtual VB1, virtual VB2, virtual VB3 { int bb; };
struct B : virtual VB { int b; };
struct VC1 { int vc1; };
struct VC2 { int vc2; };
struct VC3 { int vc3; };
struct C : virtual VC1, virtual VC2, virtual VC3 { int c; };
struct D : virtual A, virtual B, virtual C { int d; };
struct DD : virtual D { int dd; };
struct E : virtual VA1, virtual VB1, virtual VC1 { int e; };
struct EE : virtual E { int ee; };
struct DE : virtual D, virtual E { int de; };
struct F : virtual A, virtual VB1, virtual VC1 { int f; };
struct FF : virtual F { int ff; };
struct G : virtual A, virtual B, virtual VC1 { int g; };
struct GG : virtual G { int gg; };
struct H : virtual VA1, virtual B, virtual C { int h; };
struct HH : virtual H { int hh; };
struct HF : virtual H, virtual F { int hf; };
struct I : virtual VA1, virtual B, virtual VC1 { int i; };
struct II : virtual I { int ii; };

