using System;
using System.Collections.Generic;
using System.Text;

namespace HiveNetworking;

public sealed class LineMessageFramer
{
    private const int MaxMessageLength = 64 * 1024;
    private readonly Decoder _decoder = Encoding.UTF8.GetDecoder();
    private readonly StringBuilder _pending = new StringBuilder();

    public void Reset()
    {
        _decoder.Reset();
        _pending.Clear();
    }

    public IEnumerable<string> Append(byte[] bytes)
    {
        if (bytes == null)
            throw new ArgumentNullException(nameof(bytes));

        var chars = new char[Encoding.UTF8.GetMaxCharCount(bytes.Length)];
        var count = _decoder.GetChars(bytes, 0, bytes.Length, chars, 0, false);
        _pending.Append(chars, 0, count);

        if (_pending.Length > MaxMessageLength)
            throw new InvalidOperationException("The incoming message exceeds the maximum length.");

        var messages = new List<string>();
        var start = 0;
        for (var index = 0; index < _pending.Length; index++)
        {
            if (_pending[index] != '\n')
                continue;

            var message = _pending.ToString(start, index - start).TrimEnd('\r');
            if (!string.IsNullOrWhiteSpace(message))
                messages.Add(message);
            start = index + 1;
        }

        if (start > 0)
            _pending.Remove(0, start);

        return messages;
    }
}
