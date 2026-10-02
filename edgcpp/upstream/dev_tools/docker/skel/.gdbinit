# Enable pretty printing.
set print pretty on

# Stop gdb from prompt about debug info everytime it starts.
set debuginfod enabled off

# Set gdb to emphasize the thing that's about to be stepped into or over.
set style tui-current-position on

# Use a block of Python to register the EDG pretty printers.
python

from edggpp import register_printers
register_printers()

end
