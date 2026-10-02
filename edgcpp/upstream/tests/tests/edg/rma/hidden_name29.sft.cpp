//options_all:-r -x -tused
//options: --strict;cp

struct outer {
  struct inner {
    inner();
  private:
    static int x;
  };
};
outer::inner::inner() { x = 0; }

