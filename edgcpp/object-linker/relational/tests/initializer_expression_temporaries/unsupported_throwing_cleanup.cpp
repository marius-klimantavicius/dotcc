struct Input {
    Input();
    ~Input() noexcept(false);
};
struct Output {
    Output();
    Output(const Output&) = delete;
    ~Output() noexcept;
};
Input::Input() {}
Input::~Input() noexcept(false) {}
Output::Output() {}
Output::~Output() noexcept {}
Input make_input() { return Input(); }
Output consume(const Input&) { return Output(); }
Output unsupported_return() { return consume(make_input()); }
