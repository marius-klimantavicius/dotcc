//remark:MSVC typeid of const arrays
//options:--microsoft_bugs;rp

  int main() {
    return &typeid(int const[2]) == &typeid(int[2]);
  }

