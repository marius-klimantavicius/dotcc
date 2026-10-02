//remark:Inline const static data members
//options:--c++17;fn

struct A
{
  static inline const int var1;     //clang and g++ give error, EDG accepts
  static inline int var2 = 5;
  static inline const int var3 = 6;
};

template <int N>
struct B
{
  static inline int var4;
  static inline const int var5;    //clang and g++ give error, EDG accepts
  static inline int var6 = 5;
  static inline const int var7 = 6;
};

const int &ref5 = B<0>::var5;
