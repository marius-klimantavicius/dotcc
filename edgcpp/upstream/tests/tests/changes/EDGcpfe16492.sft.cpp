//type:fp
//remark:[4.11] __is_constructible and access SFINAE
// 9/1/15   [EDGcpfe/16492]
//
// __is_constructible and access SFINAE
//
// The type traits helper __is_constructible sometimes produced an incorrect
// result if a candidate conversion constructor was eliminated because it is
// inaccessible (under access SFINAE rules).
//
// This is now fixed.
struct S {
  explicit S(int);
private:
  S(char);
};
static_assert(__is_constructible(S, int) , "Unexpected!");
  // Previously failed because of the elimination of the candidate
  // S::S(char) because it is not accessible.
