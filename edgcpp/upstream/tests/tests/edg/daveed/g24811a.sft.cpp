//remark: Exception specifications
//options:--c++17;fn:--c++14;fp

  void (*p)() noexcept;
  void (**pp)() = &p;  // Previously accepted.  Now an error.
