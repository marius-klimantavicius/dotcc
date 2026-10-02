//options_all:-r -x -tused
//options: --strict;cp

namespace __SNI {
        int foo();
}

namespace __SNI_OVERLOAD {
        extern "C" int foo();
}

namespace std {

using namespace __SNI;

int bar() {
        return __SNI_OVERLOAD::foo();
}
}

