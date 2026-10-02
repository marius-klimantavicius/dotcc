//type:rp
//options:
//options_all:--c++11

typedef struct dataElement
{
  char name[10] = "";
} dataElement;

int main() {
  dataElement d;
  for (int i = 0; i < 10; ++i) {
    if (d.name[i] != 0) return 1;
  }
}

