//type:fp
//options_all:--c++17 --multi_trans_unit
//source_files:gnats20953-p2.C gnats20953.h
//require:COMPILE_MULTIPLE_TRANSLATION_UNITS 1

#include "gnats20953.h"

C c;

int main( void )
{
  return ifct() + c.cfoo;
}
