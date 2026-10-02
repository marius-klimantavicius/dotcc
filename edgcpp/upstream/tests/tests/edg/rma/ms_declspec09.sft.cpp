//options_all:-r -x -tused
//options: --microsoft -n;cn

struct __declspec(uuid("xxx")) A;
struct __declspec(uuid) B;
struct __declspec(uuid()) C;
struct D {
  int __declspec(property(get=f)) i;
  int __declspec(property(get)) j;
  int __declspec(property()) k;
  int __declspec(property) l;
};

