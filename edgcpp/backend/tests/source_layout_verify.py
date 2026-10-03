"""Check source splitting boundaries, portable names and navigation accuracy."""
from pathlib import Path
import sys
sys.path.insert(0, str(Path(__file__).resolve().parents[2]))
from backend.source_layout import SourceMember, is_layout_filename, split_members


def main():
    wrapper = 'namespace Demo;\npublic static partial class Engine {\n'
    members = [
        SourceMember('public static int sum() { return 1; }\n', '/repo/a/Module.c', 10, 2, 'sum', 'sum'),
        SourceMember('public static int other() { return 2; }\n', '/repo/b/module.c', 20, 4, 'other', 'other'),
        SourceMember('public static int more() { return 3; }\n', '/repo/a/Module.c', 30, 6, 'more', 'more'),
        SourceMember('private static int helper() { return 4; }\n'),
    ]
    source = split_members(members, wrapper, 'source')
    assert len(source) == 3
    assert 'sum()' in source[0].text and 'more()' in source[0].text
    assert source[0].text.index('sum()') < source[0].text.index('more()')
    assert 'other()' in source[1].text and source[2].filename == 'Translated.generated.cs'
    assert len({part.filename.casefold() for part in source}) == len(source)
    assert source == split_members(members, wrapper, 'source')
    for part in source:
        for location in part.locations:
            assert location['managedName'] + '()' in part.text.splitlines()[location['generatedLine'] - 1]
    assert source[0].locations[1]['sourceLine'] == 30
    functions = split_members(members, wrapper, 'function')
    assert functions[0].filename == 'Translated.function.sum.cs'
    assert len(functions) == len(members)
    pieces = split_members(members, wrapper, 'size', 100)
    assert ''.join(part.text[len(wrapper):-2] for part in pieces) == ''.join(member.text for member in members)
    # UTF-8 byte targets, not character counts; one oversized function stays whole.
    wide = SourceMember('private static string text() => "' + 'λ' * 40 + '";\n')
    pieces = split_members([wide, members[0]], wrapper, 'size', 140)
    assert len(pieces) == 2 and wide.text in pieces[0].text
    odd = [SourceMember('void first() {}\n', '/repo/../CON.c', 1, 1, '../../escape', 'first'),
           SourceMember('void second() {}\n', '/repo/odd.c', 2, 1, '../../escape', 'second'),
           SourceMember('void third() {}\n', '/repo/odd.c', 3, 1, '../../ESCAPE', 'third')]
    parts = split_members(odd, wrapper, 'function')
    assert len({part.filename.casefold() for part in parts}) == 3
    assert all('/' not in part.filename and '\\' not in part.filename and is_layout_filename(part.filename)
               for part in parts)
    assert not is_layout_filename('../Translated.source.a.cs')
    assert not is_layout_filename('Authored.cs')
    assert not is_layout_filename('Translated.source.a.cs/../../Authored.cs')
    # Relocating an identical input tree must not alter collision suffixes.
    relocated = [SourceMember(member.text, member.source_file.replace('/repo/', '/different/root/'),
                              member.source_line, member.source_column, member.source_name, member.managed_name)
                 for member in members]
    assert [part.filename for part in source] == [part.filename for part in split_members(relocated, wrapper, 'source')]
    assert split_members([], wrapper, 'source') == []
    try:
        split_members(members, wrapper, 'size', 0)
    except ValueError:
        pass
    else:
        raise AssertionError('Nonpositive split size accepted')
    print('Source layout: grouping, order, collisions, relocation, UTF-8 size, whole members and navigation passed')


if __name__ == '__main__':
    main()
