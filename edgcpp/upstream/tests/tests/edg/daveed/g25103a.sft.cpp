//remark:Overload ambiguity for comparison operators
//options:--c++20;fn

struct B1 {
  bool operator==(B1 const&) const;
};
struct B2 {
  bool operator==(B2 const&) const;
};
struct D: B1, B2 {} d;
bool operator==(D const&, D const&);

auto r = d == d;  // Might change to valid some day.
