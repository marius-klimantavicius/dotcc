//type: fp
//options:  --c++20 --modules
// { dg-additional-options -fmodules-ts }
export  module  hello;
// { dg-module-cmi hello }
import  format;

export namespace NS
{
using NS::Format;
}
