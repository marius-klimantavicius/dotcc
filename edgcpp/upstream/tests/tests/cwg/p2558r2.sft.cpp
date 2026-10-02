//options_all:--c++23 -A
extern "C" int printf(const char *, ...);

#define STR(x) #x

int main()
{
  printf("%s", STR(\u0060)); // U+0060 is ` GRAVE ACCENT
}
