//type: fp
//options: 
# 1 "SemaCXX/warn-using-namespace-in-header.cpp"
# 1 "<built-in>" 1
# 1 "<built-in>" 3
# 482 "<built-in>" 3
# 1 "<command line>" 1
# 1 "<built-in>" 2
# 1 "SemaCXX/warn-using-namespace-in-header.cpp" 2
# 51 "SemaCXX/warn-using-namespace-in-header.cpp"
# 1 "SemaCXX/warn-using-namespace-in-header.cpp" 1



namespace warn_in_header_in_global_context {}
using namespace warn_in_header_in_global_context;



namespace dont_warn_here {
using namespace warn_in_header_in_global_context;
}


namespace warn_inside_linkage {}
extern "C++" {
using namespace warn_inside_linkage;
}


extern "C++" {
extern "C" {
extern "C++" {
using namespace warn_inside_linkage;
}
}
}


namespace dont_warn_here {
extern "C++" {
using namespace warn_in_header_in_global_context;
}
}


inline void foo() {
  using namespace warn_in_header_in_global_context;
}


namespace macronamespace {}




using namespace macronamespace;
# 52 "SemaCXX/warn-using-namespace-in-header.cpp" 2

namespace dont_warn {}
using namespace dont_warn;



using namespace macronamespace;


namespace warn_header_with_line_marker {}
# 1 "XXX.h" 1
using namespace warn_header_with_line_marker;
# 70 "warn-using-namespace-in-header.cpp" 2

namespace nowarn_after_line_marker {}
using namespace nowarn_after_line_marker;
