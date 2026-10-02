//remark:Postfix ++/-- Microsoft behavior
//options:--microsoft_v=1800;fp:--microsoft_v=1900;fn

  struct S { S operator++(); };
  int main() {
    S s;
    s++;  // Ordinarily an error but previously accepted in all Microsoft C++
  }       // modes.  Now an error when microsoft_version >= 1900.

