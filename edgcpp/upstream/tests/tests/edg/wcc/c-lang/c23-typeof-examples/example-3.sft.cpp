//type:fp
//options:--c23

int main (int argc, char* argv[]) {
  static_assert(sizeof(typeof('p')) == sizeof(int));
  static_assert(sizeof(typeof('p')) == sizeof('p'));
  static_assert(sizeof(typeof((char)'p')) == sizeof(char));
  static_assert(sizeof(typeof((char)'p')) == sizeof((char)'p'));
  static_assert(sizeof(typeof("meow")) == sizeof(char[5]));
  static_assert(sizeof(typeof("meow")) == sizeof("meow"));
  static_assert(sizeof(typeof(argc)) == sizeof(int));
  static_assert(sizeof(typeof(argc)) == sizeof(argc));
  static_assert(sizeof(typeof(argv)) == sizeof(char**));
  static_assert(sizeof(typeof(argv)) == sizeof(argv));

  static_assert(sizeof(typeof_unqual('p')) == sizeof(int));
  static_assert(sizeof(typeof_unqual('p')) == sizeof('p'));
  static_assert(sizeof(typeof_unqual((char)'p')) == sizeof(char));
  static_assert(sizeof(typeof_unqual((char)'p')) == sizeof((char)'p'));
  static_assert(sizeof(typeof_unqual("meow")) == sizeof(char[5]));
  static_assert(sizeof(typeof_unqual("meow")) == sizeof("meow"));
  static_assert(sizeof(typeof_unqual(argc)) == sizeof(int));
  static_assert(sizeof(typeof_unqual(argc)) == sizeof(argc));
  static_assert(sizeof(typeof_unqual(argv)) == sizeof(char**));
  static_assert(sizeof(typeof_unqual(argv)) == sizeof(argv));
  return 0;
}
