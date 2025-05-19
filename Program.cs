using HtmlAgilityPack;
using System.Linq;
using System.Runtime.InteropServices;
using System.Xml.Linq;

public class WebPageLinkExtractor
{
    // Anzahl der Staffeln ermitteln
    static int AnzStaffeln(string basislinkderserie)
    {
        // Unterseite öffnen
        string urlinner = basislinkderserie;
        var htmlDocinner = new HtmlDocument();

        try
        {
            HtmlWeb webinner = new();
            htmlDocinner = webinner.Load(urlinner);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fehler beim Laden der Webseite: {ex.Message}");
        }
        // Anzahl der Staffeln des Anime zählen
        var anz = htmlDocinner.DocumentNode.SelectNodes("//a")?
            .Where(node => node.Attributes.Contains("title") && node.Attributes["title"].Value.Contains("Staffel") && !node.Attributes["title"].Value.Contains("Episode"))
            .Select(node => node.Attributes["title"].Value)
            .ToList();

        int count = 0;
        if (anz is not null) { count = anz.Count; }
        return count;

    }

    static (int, int) AnzFolgen(string basislinkderserie)
    {
        // Unterseite öffnen
        string urlinner = basislinkderserie;
        var htmlDocinner = new HtmlDocument();

        try
        {
            HtmlWeb webinner = new();
            htmlDocinner = webinner.Load(urlinner);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fehler beim Laden der Webseite: {ex.Message}");
        }
        // Anzahl der nur deutschen Folgen zählen
        var srcs = htmlDocinner.DocumentNode.SelectNodes("//img")?
            .Where(img => img.Attributes.Contains("src") && img.Attributes["src"].Value.StartsWith("/public/img/german.svg"))
            .Select(img => img.Attributes["src"].Value)
            .ToList();

        // Anzahl der gesamten Folgen zählen
        var td = htmlDocinner.DocumentNode.SelectNodes("//td")?
            .Where(td => td.Attributes.Contains("class") && td.Attributes["class"].Value.StartsWith("editFunctions"))
            .Select(td => td.Attributes["class"].Value)
            .ToList();

        // Rückgabewerte definieren und setzten (countall: alle Folgen / countger: nur deutsche Folgen
        int countall = 0;
        int countger = 0; 
        if (td is not null) { countall = td.Count; }
        if (srcs is not null) { countger = srcs.Count; }
        return (countger, countall);
    }

    public static void Main(string[] args)
    {
        // Übersichtsseite von Aniworld mit allen Serien und Filmen
        string url = "https://aniworld.to/animes";

        // Erstelle ein HtmlAgilityPack-Objekt
        var htmlDoc = new HtmlDocument();

        try
        {
            // Lade die Webseite und lese den HTML-Code ein
            HtmlWeb web = new();
            htmlDoc = web.Load(url);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fehler beim Laden der Webseite: {ex.Message}");
            return;
        }

        // Äußere Schleife
        // Alle Links die mit /anime/stream/ anfangen aus dem HTML-Code extrahieren
        var links = htmlDoc.DocumentNode.SelectNodes("//a")?
            .Where(node => node.Attributes.Contains("href") && node.Attributes["href"].Value.StartsWith("/anime/stream/"))
            .Select(node => node.Attributes["href"].Value)
            .ToList();

        // Alle Titel zu den zugehörigen Links auslesen
        var titles = htmlDoc.DocumentNode.SelectNodes("//a")?
            .Where(node => node.Attributes.Contains("href") && node.Attributes["href"].Value.StartsWith("/anime/stream/"))
            .Select(node => node.Attributes["title"].Value)
            .ToList();


        using (var writer = File.CreateText("Linksammlung.html"))
        {
            // Kopf der Linksammler.html Datei schreiben
            writer.WriteLine("" +
                "<html lang=\"de\">\r\n " +
                    "<head>\r\n    " +
                        "<meta charset=\"utf-8\">\r\n " +
                        "<meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">\r\n " +
                        "<style>\r\n " +
                        "</style>\r\n " +
                        "<title>Aniworld.to nur deutsche Serien</title>\r\n " +
                "</head>\r\n  " +

                // Beim aufrufen der .html Seite sortieren automatisch starten
                "<body onload=\"sortList('myList', true)\">\r\n" +

                // die Ergebnisse <li> sollen nach Alphabet sortiert in <ul> ... <ul> ausgegeben werden
                "<script>\r\n" +
                "function sortList(listId, ascending) {\r\n" +
                    "var list = document.getElementById(listId); \r\n" +
                    "var lis = list.getElementsByTagName('li'); \r\n" +
                    "var items = Array.prototype.map.call(lis, function(li) { \r\n" +
                    "    return { \r\n" +
                    "        text: li.textContent.trim(),\r\n" +
                    "        element: li\r\n" +
                    "    }; \r\n" +
                    "}); \r\n" +

                    "items.sort(function(a, b) { \r\n" +
                    "    var nameA = a.text.toLowerCase(); \r\n" +
                    "    var nameB = b.text.toLowerCase(); \r\n" +
                    "    if (nameA < nameB) { return ascending ? -1 : 1; }\r\n" +
                    "    if (nameA > nameB) { return ascending ? 1 : -1; } \r\n" +
                    "    return 0; \r\n" +
                    "}); \r\n" +

                    "var fragment = document.createDocumentFragment(); \r\n" +
                    "items.forEach(function(item) { \r\n" +
                    "    fragment.appendChild(item.element); \r\n" +
                    "}); \r\n" +
                    "list.innerHTML = ''; \r\n" +
                    "list.appendChild(fragment); \r\n" +
                "} \r\n" +
                "</script >\r\n" +
                "<div style=\"columns:50px 4;\"><ul id=\"myList\" style=\"list-style-type:none; margin: 0;\">");

            // i als Variable für die Title in Titles / progress, totalSteps und j für den Ladebalken
            int i = 0;
            int progress = 0;
            int totalSteps = 100;
            double j = 0;

            // Innere Schleife
            // Pro Link, diesen öffnen und nach einem Deutsch sprachigem Link suchen
            if (links != null)
            {
                foreach (string link in links)
                {
                    // Anzeige bei welchem Link er gerade ist (gekürzt auf max. 16 Stellen
                    //if (link.Length < 30) { Console.WriteLine(link.Substring(14)); } else { Console.WriteLine(link.Substring(14, 16)); }

                    // Wenn im Titel "Stream anschauen" enthalten ist, diesen entfernen
                    if (titles != null) { if (titles[i].Contains(" Stream anschauen")) { titles[i] = titles[i].Replace(" Stream anschauen", ""); } }

                    // Ladebalken anzeigen
                    Console.SetCursorPosition(0, 0);
                    Console.WriteLine("┌────────┬─────────────────────────────────────────────────────────────────────────────────────────────────────┐");
                    Console.WriteLine("│        │                                                                                                     │");
                    Console.WriteLine("└────────┴─────────────────────────────────────────────────────────────────────────────────────────────────────┘");
                    Console.SetCursorPosition(10, 1);
                    for (int p = 0; p < progress; p++)
                    {
                        Console.Write("▓");
                    }

                    if (progress > 0 && progress < totalSteps) { Console.Write("▒"); }
                    else if (progress == 0) { Console.Write("░"); }
                    else if (progress == totalSteps) { Console.Write("▓"); }
                    for (int p = progress; p < totalSteps; p++)

                    {
                        Console.Write("░");
                    }
                    Console.SetCursorPosition(1, 1);
                    if (progress < 10)
                    {
                        Console.Write("[   " + progress + "% ]");
                    }
                    else if (progress > 9 && progress < 100)
                    {
                        Console.Write("[  " + progress + "% ]");
                    }
                    else if (progress > 99)
                    {
                        Console.Write("[ " + progress + "% ]");
                    }
                    Console.SetCursorPosition(0, 3);
                    // Fortschritt erhöhen
                    if (titles != null) { progress = (int)(Math.Round(100 * j / titles.Count)); }

                    // Unterseite öffnen
                    string urlinner = "https://aniworld.to" + link;

                    // Anzahl der Staffeln bestimmen
                    int staffeln = AnzStaffeln(urlinner);

                    // Anzahl der Folgen in der Staffel bestimmen
                    int s = 1;
                    int summe = 0;
                    int isteinfilm = 0;
                    //int test1 = 0;
                    //int test2 = 0;

                    while (s <= staffeln)
                    {
                        string urlfolgen = urlinner + "/staffel-" + s;
                        var folgen = AnzFolgen(urlfolgen);
                        // folgen.Item2 sind deutsche Folgen / folgen.Item1 sind alle Folgen
                        if (folgen.Item2 == folgen.Item1 && folgen.Item1 == 1 && s == 1) { isteinfilm = 1; }
                        summe = summe + folgen.Item1;
                        s++;
                        //test1 = folgen.Item1;
                        //test2 = folgen.Item2;
                    }

                    if (titles != null)
                    {
                        if (summe > 0)
                        {
                            // Linkname : Anzahl der deutschen Folgen in Linksammlung.html abspeichern
                            // Unterschiedung von "Folgen" und "Film"
                            if (summe == 1 && isteinfilm == 1)
                            {
                                writer.WriteLine($"<li><a style=\"text-decoration: none; \" href=\"https://aniworld.to{link}\">{titles[i]}</a> : {summe}&nbsp;Film</li>");
                            }
                            else if (summe == 1 && isteinfilm == 0)
                            {
                                writer.WriteLine($"<li><a style=\"text-decoration: none; \" href=\"https://aniworld.to{link}\">{titles[i]}</a> : {summe}&nbsp;Folge</li>");
                            }
                            else
                            {
                                writer.WriteLine($"<li><a style=\"text-decoration: none; \" href=\"https://aniworld.to{link}\">{titles[i]}</a> : {summe}&nbsp;Folgen</li>");
                            }
                        }
                    }
                    // i als Variable für die Title in Titles / j als Variable (double) zum Berechnen des Fortschrittes
                    i++;
                    j++;

                }
            }
            // Fuß der Linksammler.html Datei schreiben
            writer.WriteLine("  </ul></div></body>\r\n</html>");

        }
    }
}
