constexpr const int& identity(const int& value){return value;}
static_assert(identity(3)==3);
static_assert(identity(identity(4))==4);
