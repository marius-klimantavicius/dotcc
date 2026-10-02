//type:fp
//options:--c23

const _Atomic int purr = 0;
const int meow = 1;
const char* const animals[] = {
  "aardvark",
  "bluejay",
  "catte",
};

typeof_unqual(meow) main (int argc, char* argv[]) {
  typeof_unqual(purr)          plain_purr;
  typeof(_Atomic typeof(meow)) atomic_meow;
  typeof(animals)              animals_array;
  typeof_unqual(animals)       animals2_array;
  return 0;
}
