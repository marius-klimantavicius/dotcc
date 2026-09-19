ENTRY(_start)
PHDRS
{
    headers PT_PHDR PHDRS;
    text PT_LOAD FILEHDR PHDRS FLAGS(5);
    data PT_LOAD FLAGS(6);
    tls PT_TLS FLAGS(4);
    stack PT_GNU_STACK FLAGS(6);
}
SECTIONS
{
    . = 0x400000 + SIZEOF_HEADERS;
    .text : { *(.text*) } :text
    . = ALIGN(0x1000);
    .tdata : { __tls_template = .; *(.tdata*) } :data :tls
    .tbss (NOLOAD) : { *(.tbss*) } :data :tls
    . = ADDR(.tbss) + SIZEOF(.tbss);
    . = ALIGN(16);
    .bss (NOLOAD) : { *(.bss*) } :data
    /DISCARD/ : { *(.note.GNU-stack) }
}
