"""Small, recorded calling-convention adapters for existing authored tests."""


def adapt(provider, source, filename, emitted, picotls):
    source, changes = provider.adapt(source, filename, emitted, picotls)
    replacements = {
        '(byte*)layout.EncryptionTag': '(byte*)&layout.EncryptionTag[0]',
        '(byte*)layout.Encrypted.OrigConnId': '(byte*)&layout.Encrypted.OrigConnId[0]',
    }
    for old, new in replacements.items():
        count = source.count(old)
        if count:
            source = source.replace(old, new)
            changes.append({'old': old, 'new': new, 'count': count})
    return source, changes
