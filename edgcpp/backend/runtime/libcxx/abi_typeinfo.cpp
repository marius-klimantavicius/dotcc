/*
Part of the EDG Compiler Project, under the Apache License v2.0 with LLVM
Exceptions. SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception

Itanium RTTI implementation classes and their key functions, from
upstream/include_c++/cxxabi.h and upstream/lib_src/typeinfo.c. The empty
destructors below are the actual EDG implementations; their definitions make
the compiler emit real ABI vtables, rather than synthetic placeholder storage.

Compile with the pinned LLVM typeinfo/new declarations. These destructor-only
providers use an explicit no-exceptions profile; general C++ EH is supplied
separately. LLVM src/typeinfo.cpp supplies std::type_info's key destructor.
*/
#include <new>
#include <typeinfo>
#ifndef __EDG_RUNTIME_USES_NAMESPACES
#define __EDG_RUNTIME_USES_NAMESPACES
#endif
#include "../../../upstream/include_c++/cxxabi.h"

namespace __cxxabiv1 {
__fundamental_type_info::~__fundamental_type_info() {}
__array_type_info::~__array_type_info() {}
__function_type_info::~__function_type_info() {}
__enum_type_info::~__enum_type_info() {}
__class_type_info::~__class_type_info() {}
__si_class_type_info::~__si_class_type_info() {}
__vmi_class_type_info::~__vmi_class_type_info() {}
__pbase_type_info::~__pbase_type_info() {}
__pointer_type_info::~__pointer_type_info() {}
__pointer_to_member_type_info::~__pointer_to_member_type_info() {}
}
