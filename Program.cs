using HtmlAgilityPack;
using System.Linq;
using System.Runtime.InteropServices;
using System.Xml.Linq;

public class WebPageLinkExtractor
{
    public static void Main(string[] args)
    {
        // Übersichtsseite von Aniworld mit allen Serien und Filmen
        string url = "https://aniworld.to/animes";

        // Erstelle ein HtmlAgilityPack-Objekt
        var htmlDoc = new HtmlDocument();

        try
        {
            // Lade die Webseite und lese den HTML-Code ein
            HtmlWeb web = new HtmlWeb();
            htmlDoc = web.Load(url);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fehler beim Laden der Webseite: {ex.Message}");
            return;
        }

        // Äußere Schleife 1111
        // Alle Links die mit /anime/stream/ anfangen aus dem HTML-Code extrahieren
        var links = htmlDoc.DocumentNode.SelectNodes("//a")
            .Where(node => node.Attributes.Contains("href") && node.Attributes["href"].Value.StartsWith("/anime/stream/"))
            .Select(node => node.Attributes["href"].Value)
            .ToList();

        // Alle Titel zu den zugehörigen Links auslesen
        var titles = htmlDoc.DocumentNode.SelectNodes("//a")
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
                "<body><div style=\"columns:50px 4;\"><ul style=\"list-style-type:none; margin: 0;\">");

            // i als Variable für die Title in Titles / progress, totalSteps und j für den Ladebalken
            int i = 0;
            int progress = 0;
            int totalSteps = 100;
            double j = 0;

            // Innere Schleife
            // Pro Link, diesen öffnen und nach einem Deutsch sprachigem Link suchen
            foreach (string link in links)
            {
                // Anzeige bei welchem Link er gerade ist (gekürzt auf max. 16 Stellen
                //if (link.Length < 30) { Console.WriteLine(link.Substring(14)); } else { Console.WriteLine(link.Substring(14, 16)); }

                // Wenn im Titel "Stream anschauen" enthalten ist, diesen entfernen
                if (titles[i].Contains(" Stream anschauen")) { titles[i] = titles[i].Replace(" Stream anschauen", ""); }

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

                    if ( progress > 0 && progress < totalSteps ) { Console.Write("▒"); } 
                    else if ( progress == 0 ) { Console.Write("░"); } 
                    else if ( progress == totalSteps ) { Console.Write("▓"); }
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
                    progress = (int)(Math.Round(100 * j / titles.Count));

                // Unterseite öffnen
                string urlinner = "https://aniworld.to" + link;
                var htmlDocinner = new HtmlDocument();

                try
                {
                    HtmlWeb webinner = new HtmlWeb();
                    htmlDocinner = webinner.Load(urlinner);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Fehler beim Laden der Webseite: {ex.Message}");
                    return;
                }

                // Alle img tags auslesen aus einer bestimmten Anime Serie
                var srcs = htmlDocinner.DocumentNode.SelectNodes("//img")
                    .Where(img => img.Attributes.Contains("src") && img.Attributes["src"].Value.StartsWith("/public/img/german.svg"))
                    .Select(img => img.Attributes["src"].Value)
                    .ToList();


                if (srcs.Count > 0)
                {
                    // Linkname : Anzahl der deutschen Folgen in Linksammlung.html abspeichern
                    // Unterschiedung von "Folgen" und "Folge"
                    if (srcs.Count == 1)
                    {
                        writer.WriteLine($"<li><a style=\"text-decoration: none; \" href=\"https://aniworld.to{link}\">{titles[i]}</a>:&nbsp;{srcs.Count}&nbsp;Folge</li>");
                    }
                    else
                    {
                        writer.WriteLine($"<li><a style=\"text-decoration: none; \" href=\"https://aniworld.to{link}\">{titles[i]}</a>:&nbsp;{srcs.Count} &nbsp;Folgen</li>");
                    }
                }
                // i als Variable für die Title in Titles / j als Variable (double) zum Berechnen des Fortschrittes
                i++;
                j++;
                
            }
            // Fuß der Linksammler.html Datei schreiben
            writer.WriteLine("  </ul></div></body>\r\n</html>");

        }
    }
}
