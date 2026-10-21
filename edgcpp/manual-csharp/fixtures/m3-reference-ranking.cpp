// SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
// Authored rank/cv/reference-kind controls; all selection results have distinct types.
int reference_promote(const int&);
long reference_promote(const long&);
int reference_cv(int&&);
long reference_cv(const int&&);
int reference_kind(const int&&);
long reference_kind(const int&);
int reference_converted(int&&);
long reference_converted(const int&);
int reference_exact(const int&);
long reference_exact(const long&);
int reference_short(short value) { return reference_promote(value); }
int reference_rvalue() { return reference_cv(1); }
int reference_kind_first() { return reference_kind(1); }
int reference_from_lvalue(long value) { return reference_converted(value); }
long reference_rank_first(long value) { return reference_exact(value); }
