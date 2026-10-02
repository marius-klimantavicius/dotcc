//type:cp
//options:--c++17:--c++17 -r:--c++20
//options_all:--diag_suppress 174,177 -tused --set_flag=no_checking_pragmas

typedef decltype(sizeof(0)) size_t;
namespace std {
#pragma define_type_info
  class type_info {
  public:
    virtual ~type_info();
    bool operator==(const type_info& rhs) const noexcept;
    bool operator!=(const type_info& rhs) const noexcept;
    bool before(const type_info& rhs) const noexcept;
    size_t hash_code() const noexcept;
    const char* name() const noexcept;
    type_info(const type_info& rhs) = delete; // cannot be copied
    type_info& operator=(const type_info& rhs) = delete; // cannot be copied
  };
}

void incdec() {
  int i = 0;
  volatile int vi = 0;

  i++;  // Should not warn
  vi++; // Deprecated
  i--;  // Should not warn
  vi--; // Deprecated

  ++i;  // Should not warn
  ++vi; // Deprecated
  --i;  // Should not warn
  --vi; // Deprecated
}

struct A {
  A();
  A(A&);
  A(volatile A&);
  volatile A& operator=(volatile A&) volatile { return *this; }
  A& operator=(volatile A&) { return *this; }
  operator bool();
  operator bool() volatile;
};

template<typename T, typename U>
void templass(T t, U u) {
  t = u; // Should not warn
  u = t; // Should not warn
  t = t = u; // Should not warn
  u = u = t; // Deprecated if u is volatile
  (T)(t = u); // Should not warn
  (T)(u = t); // Deprecated if u is volatile
  (void)(t = u); // Should not warn
  (void)(u = t); // Should not warn
  if (t = u, u); // Should not warn
  if (u = t, t); // Should not warn
  if (u, t = u); // Should not warn
  if (t, u = t); // Deprecated if u is volatile
  if (t = u); // Should not warn
  if (u = t); // Deprecated if u is volatile
}

void ass() {
  int i = 0;
  volatile int vi = 0;
  A a;
  volatile A va;

  i = vi; // Should not warn
  vi = i; // Should not warn
  a = va; // Should not warn
  va = a; // Should not warn

  i = i = vi;  // Should not warn
  vi = vi = i; // Deprecated
  a = a = va;  // Should not warn
  va = va = a; // Should not warn

  templass<int, int>(i, i);           // Should not warn
  templass<int, volatile int>(i, vi); // Should trigger deprecated warning on instantiation
  templass<A, A>(a, a);           // Should not warn
  templass<A, volatile A>(a, va); // Should not warn

  (int)(i = vi); // Should not warn
  (int)(vi = i); // Deprecated
  (void)(i = vi); // Should not warn
  (void)(vi = i); // Should not warn
  if (i = vi, vi); // Should not warn
  if (vi = i, vi); // Should not warn
  if (vi, i = vi); // Should not warn
  if (vi, vi = i); // Deprecated
  if (i = vi); // Should not warn
  if (vi = i); // Deprecated

  typeid(i = vi); // Should not warn
  typeid(vi = i); // Should not warn
  sizeof(i = vi); // Should not warn
  sizeof(vi = i); // Should not warn
  noexcept(i = vi); // Should not warn
  noexcept(vi = i); // Should not warn
  decltype(i = vi) foo = i; // Should not warn
  decltype(vi = i) bar = i; // Should not warn
}

void opass() {
  int i = 0;
  volatile int vi = 0;

  i += vi; // Should not warn
  vi += i; // Should not warn
  i -= vi; // Should not warn
  vi -= i; // Should not warn
  i *= vi; // Should not warn
  vi *= i; // Should not warn
  i /= vi; // Should not warn
  vi /= i; // Should not warn
  i %= vi; // Should not warn
  vi %= i; // Should not warn

  i >>= vi; // Should not warn
  vi >>= i; // Should not warn
  i <<= vi; // Should not warn
  vi <<= i; // Should not warn
  i &= vi;  // Should not warn
  vi &= i;  // Should not warn
  i ^= vi;  // Should not warn
  vi ^= i;  // Should not warn
  i |= vi;  // Should not warn
  vi |= i;  // Should not warn
}

int func1(                // Should not warn on return type
          int i,          // Should not warn
          volatile int vi // Deprecated
          );

typedef volatile int volint;

volint func2(       // Return type deprecated
          int i,    // Should not warn
          volint vi // Deprecated
          );

auto func3() -> volint; // Return type deprecated

struct B {
  int func1(              // Should not warn on return type
          int i,          // Should not warn
          volatile int vi // Deprecated
          );

  volint func2(     // Return type deprecated
          int i,    // Should not warn
          volint vi // Deprecated
          );

  auto func3() -> volint; // Return type deprecated
};

void strbind() {
  volatile int arr[3] = {1,2,3};

  auto [a,b,c] = arr;             // Should not warn
  volatile auto [va,vb,vc] = arr; // Deprecated
}

