// See https://aka.ms/new-console-template for more information
using System.Text;

Console.WriteLine("Hello, Spider fans.");


var alllines = File.ReadAllLines(args[0]);
string category = alllines[0];
Dictionary<string, List<SpiderRecord>> allSpiders = new();
allSpiders.Add(category, new());

int count = 0;
for (int i = 1; i < alllines.Length; i++)
{
    if (alllines[i].Trim().Equals(""))
    {
        i++;
        if (i < alllines.Length)
        {
            category = alllines[i].Trim();
            allSpiders.Add(category, new());
        }
    }
    else
    {
        var splits = alllines[i].Split(" ", StringSplitOptions.RemoveEmptyEntries);
        var family = splits[0];
        var genus = splits[1];
        var species = splits[2];
        StringBuilder sb = new();
        for(int k = 1;k < splits.Length;k++)
            sb.Append(splits[k] + " ");
        var genusspecies = sb.ToString().Trim().Replace(',',' ');
        var spider = new SpiderRecord(family, genus, species, genusspecies);
        allSpiders[category].Add(spider);
        count++;
    }
}

var outputtext = new string[count];
int l = 0;
foreach (var cat in allSpiders.Keys)
{
    var list = allSpiders[cat];
    foreach (var spid in list)
    {
        outputtext[l] = $"""{cat},{spid.family},{spid.genus},{spid.species},{spid.genusspecies},https://srs.britishspiders.org.uk/portal.php/p/Distribution/s/{spid.genus}+{spid.species}/o/17/u//x/""";
        l++;
    }
}

File.WriteAllLines(args[1], outputtext);


public record SpiderRecord(string family, string genus, string species, string genusspecies);

