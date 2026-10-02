//type:cn
//options_all:--c++20

int foo() {
  goto foo;
foo:
  goto done;
foo:
  goto done;
done:
  return 0;
}
