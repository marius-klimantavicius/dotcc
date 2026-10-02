These tests were created as part of follow up work on the
`std::source_location` feature.  They are inspired by the C++
standard's source location spec requirements.

These tests run in several front end modes to allow output to be compared
between compiler emulation modes with contextually relevant headers
(e.g. Microsoft mode uses the Microsoft style source_location header).
