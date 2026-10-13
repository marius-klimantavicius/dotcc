int header_side_effect();
int file_stmt_size = sizeof(({ int value = header_side_effect(); value; }));
int file_stmt_direct_size = sizeof(({ header_side_effect(); 1; }));
int actual_file_stmt_control() { return header_side_effect(); }
