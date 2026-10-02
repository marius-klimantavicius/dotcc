//type:fp
//options:--c++20 --no_il_lower --il_display
//filter:awk '/^file-scope using-decl@/{f=1; next}/^$/{if (f) print $0; f=0}f' | grep -E -e '^(entity|access|qualifier.[^:]*|is_[^:]*):' -e '^file-scope ' -e '^$' | sed -e 's/@[0-9a-f]*:/:/'

enum E
{
  E1, E2, E3
};

struct B
{
  using E::E1;
  static constexpr int v1 = 1;
  static constexpr int v2 = 1;
  static constexpr int v3 = 1;

protected:
  using E::E2;

private:
  using E::E3;
};

struct D : B
{
  using B::E1;
  using B::v1;

protected:
  using B::E2;
  using B::v2;

private:
  using B::v3;
};
