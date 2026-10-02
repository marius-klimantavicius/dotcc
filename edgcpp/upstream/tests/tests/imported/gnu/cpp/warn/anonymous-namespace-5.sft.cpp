//type: fp
//options: 
namespace {
  void f();			// { dg-message "never defined" }
}

int main()
{
  f();
}
