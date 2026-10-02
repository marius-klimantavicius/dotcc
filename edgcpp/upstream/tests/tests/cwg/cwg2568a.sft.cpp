//options_all:--c++23 -A
  struct Base {
  protected:
    bool operator==(const Base& other) const = default;
  };

  struct Child : Base {
    int i;
    bool operator==(const Child& other) const = default;
  };

//cwg: 2568
//title: Access checking during synthesis of defaulted comparison operator
//meeting: Tokyo 3/24
//edg_status: Passes
