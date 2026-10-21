int global=3;
constexpr int& alias=global;
constexpr int& identity(int& value){return value;}
constexpr int discard(){identity(global);return 1;}
static_assert(discard()==1);
