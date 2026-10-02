//type: fp
//options: --c++11
// { dg-do compile { target c++11 } }

void
foo ()
{
  [[ompx::directive(some_vendor_extension)]];  /* { dg-warning "attributes at the beginning of statement are ignored" } */
}
