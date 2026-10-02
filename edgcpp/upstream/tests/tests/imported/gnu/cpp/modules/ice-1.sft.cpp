//type: fn
//options:  --c++20 --modules
// { dg-additional-options "-fmodules-ts" }
// we ICED on malformed preambles ending at EOF.
import bob // { dg-error "expected" }
