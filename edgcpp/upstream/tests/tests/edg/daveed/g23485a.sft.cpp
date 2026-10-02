//remark:Virtual destructor overriding
//options:--c++11 -A;fn

  struct ND { ~ND() noexcept(false); };
  struct VED { virtual ~VED() noexcept(true); };
  struct S: VED {
    union { ND v; };
  };  // Previously silently accepted.  Now a warning or error because
      // the generated destructor is noexcept(false), but the overridden
      // destructor is noexcept(true).
