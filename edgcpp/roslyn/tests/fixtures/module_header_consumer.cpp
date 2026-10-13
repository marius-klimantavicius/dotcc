import "module_header.hpp" [[roslyn_probe::marker]];
int use_header() { return header_value(HeaderValue{3}); }
