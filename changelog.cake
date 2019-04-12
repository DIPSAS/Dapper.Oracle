using System.Text.RegularExpressions;

private class ChangeLog
{
    // patterns
    private static Regex semver = new Regex(@"## ?\[?v?([\w\d.-]+\.[\w\d.-]+[a-zA-Z0-9])?");
    private static Regex date = new Regex(@".*[ ](\d\d?\d?\d[-/.]\d\d?[-/.]\d\d?\d?\d?).*");

    private static string EOL = Environment.NewLine;

    public string Title {get;set;}
    public string Description {get;set;}
    public List<VersionInfo> Versions {get;} = new List<VersionInfo>();

    public VersionInfo GetVersion(string version)
    {
        return Versions.FirstOrDefault(ver => version.Equals(ver.Version));
    }

    public override string ToString()
    {
        var str = $"Title: {Title}\nDescription: {Description}\nVersions:\n";
        foreach (var ver in Versions)
        {
            str += ver.ToString() + "\n";
        }
        return str;
    }

    public static ChangeLog Parse(string filename)
    {
        var input = System.IO.File.ReadAllLines(filename);
        return Parse(input);
    }

    public static ChangeLog Parse(IEnumerable<string> lines)
    {
        var log = new ChangeLog();

        VersionInfo current = null;
        string activeSubhead = null;
        foreach (var line in lines)
        {
            log.HandleLine(line, ref current, ref activeSubhead);
        }

        // push last version into log
        log.AddVersion(current);

        // clean up description
        log.Description = Clean(log.Description);
        if (log.Description == string.Empty)
        {
            log.Description = null;
        }

        return log;
    }


    private void HandleLine(string line, ref VersionInfo current, ref string activeSubhead)
    {
        // skip line if it's a link label
        if (Regex.IsMatch(line, @"^\[[^[\]]*\] *?:"))
        {
            return;
        }

        // set title if it's there
        if (Title == null && Regex.IsMatch(line, "^# ?[^#]"))
        {
            Title = line.Substring(1).Trim();
            return;
        }

        // new version found!
        if (Regex.IsMatch(line, "^## ?[^#]"))
        {
            if (current?.Title != null)
            {
                AddVersion(current);
            }

            current = new VersionInfo();
            activeSubhead = null;

            var match = semver.Match(line);
            if (match.Success)
            {
                current.Version = match.Groups[1].Value;
            }

            current.Title = line.Substring(2).Trim();

            match = date.Match(current.Title);
            if (current.Title != null && match.Success)
            {
                current.Date = match.Groups[1].Value;
            }

            return;
        }

        // deal with body or description content
        if (current != null)
        {
            current.HandleLine(line, ref activeSubhead);
        }
        else
        {
            Description = (Description ?? string.Empty) + line + EOL;
        }
    }

    private void AddVersion(VersionInfo current)
    {
        if (current != null)
        {
            current.Body = Clean(current.Body);
            Versions.Add(current);
        }
    }

    private static string Clean(string str)
    {
        if (str == null)
        {
            return string.Empty;
        }

        str = str.Trim();
        str = Regex.Replace(str, "^[" + EOL + "]*", string.Empty);
        str = Regex.Replace(str, "[" + EOL + "]*$", string.Empty);

        return str;
    }
}

private class VersionInfo
{
    // patterns
    private static Regex subhead = new Regex("^###");
    private static Regex listitem = new Regex("^[*-]");

    public string Version {get;set;}
    public string Title {get;set;}
    public string Date {get;set;}
    public string Body {get;set;} = string.Empty;
    public Dictionary<string, List<string>> Parsed {get;} = new Dictionary<string, List<string>> {
        { "_", new List<string>() }
    };

    public override string ToString()
    {
        var str = $"  Version: {Version}\n    Title: {Title}\n    Date: {Date}\n    Body: {Body}\n";
        foreach (var key in Parsed.Keys)
        {
            str += $"    {key}:\n";
            foreach (var line in Parsed[key])
            {
                str += $"      {line}\n";
            }
        }
        return str;
    }

    public void HandleLine(string line, ref string activeSubhead)
    {
        Body += line + "\n";

        // handle case where current line is a 'subhead':
        // - 'handleize' subhead.
        // - add subhead to 'parsed' data if not already present.
        var match = subhead.Match(line);
        if (match.Success)
        {
            var key = line.Replace("###", string.Empty).Trim();

            if (!Parsed.ContainsKey(key))
            {
                Parsed[key] = new List<string>();
                activeSubhead = key;
            }
        }

        // handle case where current line is a 'list item':
        match = listitem.Match(line);
        if (match.Success)
        {
            line = RemoveMarkdown(line);

            // add line to 'catch all' array
            Parsed["_"].Add(line);

            // add line to 'active subhead' if applicable (eg. 'Added', 'Changed', etc.)
            if (activeSubhead != null)
            {
                Parsed[activeSubhead].Add(line);
            }
        }
    }
}


private static string RemoveMarkdown(string str, string listUnicodeChar = null, bool stripListLeaders = true, bool gfm = true, bool useImgAltText = true)
{
    var output = str ?? string.Empty;

    // Remove horizontal rules (stripListLeaders conflict with this rule, which is why it has been moved to the top)
    output = Regex.Replace(output, @"(?m)^(-\s*?|\*\s*?|_\s*?){3,}\s*$", string.Empty);

    if (stripListLeaders)
    {
        var replacement = "$1";
        if (listUnicodeChar != null && listUnicodeChar != string.Empty)
        {
            replacement = $"{listUnicodeChar} $1";
        }
        output = Regex.Replace(output, @"(?m)^([\s\t]*)([\*\-\+]|\d+\.)\s+", replacement);
    }

    if (gfm)
    {
        // Header
        output = Regex.Replace(output, @"\n={2,}", "\n");
        // Fenced codeblocks
        output = Regex.Replace(output, @"~{3}.*\n", string.Empty);
        // Strikethrough
        output = Regex.Replace(output, "~~", string.Empty);
        // Fenced codeblocks
        output = Regex.Replace(output, @"`{3}.*\n", string.Empty);
    }

    // Remove HTML tags
    output = Regex.Replace(output, @"<[^>]*>", string.Empty);
    // Remove setext-style headers
    output = Regex.Replace(output, @"^[=\-]{2,}\s*$", string.Empty);
    // Remove footnotes?
    output = Regex.Replace(output, @"\[\^.+?\](\: .*?$)?", string.Empty);
    output = Regex.Replace(output, @"\s{0,2}\[.*?\]: .*?$", string.Empty);
    // Remove images
    output = Regex.Replace(output, @"\!\[(.*?)\][\[\(].*?[\]\)]", useImgAltText ? "$1" : string.Empty);
    // Remove inline links
    output = Regex.Replace(output, @"\[(.*?)\][\[\(].*?[\]\)]", "$1");
    // Remove blockquotes
    output = Regex.Replace(output, @"^\s{0,3}>\s?", string.Empty);
    // Remove reference-style links?
    output = Regex.Replace(output, @"^\s{1,2}\[(.*?)\]: (\S+)( "".*?"")?\s*$", string.Empty);
    // Remove atx-style headers
    output = Regex.Replace(output, @"(?m)^(\n)?\s{0,}#{1,6}\s+| {0,}(\n)?\s{0,}#{0,} {0,}(\n)?\s{0,}$", "$1$2$3");
    // Remove emphasis (repeat the line to remove double emphasis)
    output = Regex.Replace(output, @"([\*_]{1,3})(\S.*?\S{0,1})\1", "$2");
    output = Regex.Replace(output, @"([\*_]{1,3})(\S.*?\S{0,1})\1", "$2");
    // Remove code blocks
    output = Regex.Replace(output, @"(?m)(`{3,})(.*?)\1", "$2");
    // Remove inline code
    output = Regex.Replace(output, @"`(.+?)`", "$1");
    // Replace two or more newlines with exactly two? Not entirely sure this belongs here...
    output = Regex.Replace(output, @"\n{2,}", "\n\n");

    return output;
}
