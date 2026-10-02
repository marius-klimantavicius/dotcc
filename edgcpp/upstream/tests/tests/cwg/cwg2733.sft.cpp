//options_all:--c++23 -A

  [[maybe_unused]] void f([[maybe_unused]] bool thing1,
                          [[maybe_unused]] bool thing2) {
    [[maybe_unused]] bool b = thing1 && thing2;
#ifdef NDEBUG
    goto x;
#endif
    [[maybe_unused]] x:
  }

//cwg: 2733
//title: Applying [[maybe_unused]] to a label
//meeting: Kona 11/23
//edg_status: EDGcpfe/26801
