// This is an ordinary unresolved source declaration, not a selected cleanup
// action. Its spelling alone must never select the exception-owner runtime.
extern "C" void __destroy_exception_object();

extern "C" int fixture()
{
    __destroy_exception_object();
    return 1;
}
