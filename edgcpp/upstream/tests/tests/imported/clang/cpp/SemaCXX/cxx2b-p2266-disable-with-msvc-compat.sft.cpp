//type: fn
//options:  --c++23: --c++23 --ms_compatibility: --c++20
# 1 "SemaCXX/cxx2b-p2266-disable-with-msvc-compat.cpp"
# 1 "<built-in>" 1
# 1 "<built-in>" 3
# 499 "<built-in>" 3
# 1 "<command line>" 1
# 1 "<built-in>" 2
# 1 "SemaCXX/cxx2b-p2266-disable-with-msvc-compat.cpp" 2
# 16 "SemaCXX/cxx2b-p2266-disable-with-msvc-compat.cpp"
struct nocopy {
  nocopy(nocopy &&);
};

int &&mt1(int &&x) { return x; }
int &mt2(int &&x) { return x; }
nocopy mt3(nocopy x) { return x; }

namespace {
int &&mt1(int &&x) { return x; }
int &mt2(int &&x) { return x; }
nocopy mt3(nocopy x) { return x; }
}

namespace foo {
int &&mt1(int &&x) { return x; }
int &mt2(int &&x) { return x; }
namespace std {
int &&mt1(int &&x) { return x; }
int &mt2(int &&x) { return x; }
nocopy mt3(nocopy x) { return x; }
}
}

namespace std {

int &&mt1(int &&x) { return x; }
int &mt2(int &&x) { return x; }
nocopy mt3(nocopy x) { return x; }

namespace {
int &&mt1(int &&x) { return x; }
int &mt2(int &&x) { return x; }
nocopy mt3(nocopy x) { return x; }
}

namespace foo {
int &&mt1(int &&x) { return x; }
int &mt2(int &&x) { return x; }
nocopy mt3(nocopy x) { return x; }
}

}

# 1 "SemaCXX/cxx2b-p2266-disable-with-msvc-compat.cpp" 1
# 67 "SemaCXX/cxx2b-p2266-disable-with-msvc-compat.cpp"
int &&ut1(int &&x) { return x; }
int &ut2(int &&x) { return x; }
nocopy ut3(nocopy x) { return x; }

namespace {
int &&ut1(int &&x) { return x; }
int &ut2(int &&x) { return x; }
nocopy ut3(nocopy x) { return x; }
}

namespace foo {
int &&ut1(int &&x) { return x; }
int &ut2(int &&x) { return x; }
nocopy ut3(nocopy x) { return x; }
namespace std {
int &&ut1(int &&x) { return x; }
int &ut2(int &&x) { return x; }
nocopy ut3(nocopy x) { return x; }
}
}

namespace std {

int &&ut1(int &&x) { return x; }
int &ut2(int &&x) { return x; }
nocopy ut3(nocopy x) { return x; }

namespace {
int &&ut1(int &&x) { return x; }
int &ut2(int &&x) { return x; }
nocopy ut3(nocopy x) { return x; }
}

namespace foo {
int &&ut1(int &&x) { return x; }
int &ut2(int &&x) { return x; }
nocopy ut3(nocopy x) { return x; }
}

}
# 61 "SemaCXX/cxx2b-p2266-disable-with-msvc-compat.cpp" 2


# 1 "SemaCXX/cxx2b-p2266-disable-with-msvc-compat.cpp" 1
# 111 "SemaCXX/cxx2b-p2266-disable-with-msvc-compat.cpp" 3

int &&st1(int &&x) { return x; }
int &st2(int &&x) { return x; }
nocopy st3(nocopy x) { return x; }

namespace {
int &&st1(int &&x) { return x; }
int &st2(int &&x) { return x; }
nocopy st3(nocopy x) { return x; }
}

namespace foo {
int &&st1(int &&x) { return x; }
int &st2(int &&x) { return x; }
nocopy st3(nocopy x) { return x; }
namespace std {
int &&st1(int &&x) { return x; }
int &st2(int &&x) { return x; }
nocopy st3(nocopy x) { return x; }
}
}

namespace std {

int &&st1(int &&x) { return x; }
int &st2(int &&x) { return x; }
nocopy st3(nocopy x) { return x; }

namespace {
int &&st1(int &&x) { return x; }
int &st2(int &&x) { return x; }
nocopy st3(nocopy x) { return x; }
}

namespace foo {
int &&st1(int &&x) { return x; }
int &st2(int &&x) { return x; }
nocopy st3(nocopy x) { return x; }
}

}
# 64 "SemaCXX/cxx2b-p2266-disable-with-msvc-compat.cpp" 2

