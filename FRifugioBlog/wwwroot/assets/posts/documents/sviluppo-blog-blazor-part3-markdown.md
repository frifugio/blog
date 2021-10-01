---
title: Come ho sviluppato il mio blog - Parte 3
author: Francesco Rifugio
headImagePath: assets/posts/images/sviluppo-blog-3-header.png
summary: Il motore del blog spiegato - come renderizzare un file Markdown in HTML.
publishDate: 2021-10-01
categories: [dev, blazor, markdown, yaml]
---

# Come ho sviluppato il mio blog - Parte 3

## Markdown

Benvenuti alla terza parte di questo percorso che vuole mostrare come è stato creato questo blog. Se vi siete persi i post precedenti, potete recuperare qui [la prima parte](https://frifugio.github.io/blog/post/sviluppo-blog-blazor-part1.md), mentre qui [la seconda parte](https://frifugio.github.io/blog/post/sviluppo-blog-blazor-part2-github-actions.md).

Quindi, dopo aver fatto tutto il setup del nostro progetto, con tanto di deploy automatico su GitHub Pages, è arrivato il momento di far girare il motore del blog. Per scrivere gli articoli ho infatti deciso di procedere con il formato **Markdown**, così da avere il massimo della compatibilità ovunque poi voglia pubblicare i post.

Se non conoscete [Markdown](https://www.markdownguide.org/getting-started/) è un linguaggio di markup molto leggero che permette la formattazione di un testo in modo semplice e veloce, ma soprattutto senza l'utilizzo di particolari editor WYSIWYG: i vari stili e/o elementi testuali infatti vengono tutti gestiti da particolari shortcut.

> Consiglio personale: Visual Studio Code è un editor Markdown fenomenale, magari prossimamente farò un post per mostrarvi come l'ho impostato e quali estensioni ho installato per lavorare il più comodamente possibile con i file .md

Un altro vantaggio di Markdown è che è molto facile renderizzarlo in HTML, ed essendo universalmente utilizzato, è anche molto facile trovare componenti che ci permettono di fare ciò in praticamente qualsiasi linguaggio, anche nel C# che ci serve per la nostra applicazione Blazor.  
La mia scelta è ricaduta su [**Markdig**](https://github.com/xoofx/markdig): un processore veloce, compatibile con le specifiche [CommonMark](https://commonmark.org/) e soprattutto completo di tutte le funzionalità di cui ho bisogno.

Prima di vedere però il codice C#, partiamo dal primo articolo che ho pubblicato nel blog, così da capire le particolarità del file, e come possiamo recuperarli dalla nostra applicazione Blazor.

Questo è uno snippet dal primo articolo così come pubblicato:

```markdown
---
title: Benvenuti sul mio blog!
author: Francesco Rifugio
headImagePath: assets/posts/images/cover-benvenuti.png
summary: L'inizio di una nuova avventura, e con esso di un nuovo blog in cui poter condividere conoscenza.
publishDate: 2021-07-19
categories: [blog, misc]
---

# Benvenuti sul mio blog!

Ciao a tutti!
Questa settimana per me inizierà una nuova ed emozionante avventura lavorativa, e quindi quale migliore occasione di lanciare con esso anche il mio nuovo blog!

...
```

## YAML Front Matter

Se analizzate la prima parte del documento appena sopra, e avete già avuto dimestichezza con Markdown, noterete come in realtà quel blocco iniziale non sembra proprio del formato; infatti, quello è un piccolo blocco YAML, compreso fra tre "**-**", denominato **_YAML Front Matter_**.

Questo viene utilizzato per aggiungere cosiddetti _metadati_, quindi informazioni aggiuntive al contenuto vero e proprio del documento: ci trovate infatti il titolo, l'autore, un array di categorie e così via (tutte info che se notate sono utilizzate poi in vari punti del blog su cui state leggendo). Non esistono proprietà prefissate, potete decidere liberamente il nome di ognuna, e, dopo i **":"**, il suo valore fra i tipi supportati: numerico, stringa o data (in formato YYYY-MM-DD).

Questa sezione è molto comoda da sfruttare anche grazie al fatto che **MarkDig** è in grado di riconoscere lo **YAML Front Matter** dal documento e di estrarne i dati. Con questi poi non ci resterà altro che utilizzare un'altra libreria utile proprio alla deserializzazione dello YAML: **[YAMLDotNet](https://github.com/aaubry/YamlDotNet)**; il tutto con una configurazione pressochè minima che vedremo fra poco.

## Utilizzo dei file statici

Quindi, una volta scritto il nostro articolo con tanto di metadati, ora bisogna fare in modo che la nostra applicazione Blazor sia in grado di vederlo; e per quanto sembri una cosa banale, in realtà non è propriò così lineare il processo.

Questo perchè il nostro blog è sviluppato in Blazor WASM, che dobbiamo pensare in tutto e per tutto come ad una SPA (Single-Page Application); quindi tutto ciò che serve alla nostra applicazione per farla _girare_ viene scaricato sul client, mentre tutti i file statici, come appunto il nostro documento Markdown, eventuali immagini e altro, che non sono strettamente necessari al funzionamento dell'applicazione devono essere richiesti sul momento.

E come possiamo fare ciò? In realtà è molto semplice, ci basterà fare una richiesta HTTP GET con il percorso relativo partendo dalla cartella wwwroot, che è la base di tutti i nostri file statici. E la maniera più facile è sfruttare l'oggetto **HttpClient** che possiamo recuperare tramite Dependency Injection.

```csharp
var fileString = await _client.GetStringAsync("assets/posts/benvenuto.md");
```

C'è poi un'altro problema legato sempre al comportamento di Blazor WASM e alla sua sicurezza; non ci è infatti possibile andare ad enumerare un elenco di file statici, che siano essi contenuti in una cartella o direttamente in _wwwroot_.

Quindi come possiamo fare per recuperare l'elenco di tutti gli articoli che pubblicheremo sul nostro blog? Qui vi rispondo con la mia soluzione, che non è detto sia l'unica o la migliore, e anzi, se avete proposte non vedo l'ora di sentirle.

Per ovviare a questa limitazione di sicurezza quindi non ho fatto altro che crearmi un file JSON che richiamerò come visto appena in alto, e che al suo interno contiene la lista dei nomi di tutti i file Markdown dei post pubblicati:

```json
{
  "post": [
    "benvenuti.md",
    "sviluppo-blog-blazor-part1.md",
    "sviluppo-blog-blazor-part2-github-actions.md"
  ]
}
```

In questo modo è molto facile recuperare la lista, l'unica accortezza è quella di ricordarsi di aggiornare l'elenco ogni qual volta si voglia pubblicare un nuovo articolo.

## Modelli e servizi

Benissimo, ora che abbiamo fatto tutte le premesse necessarie, non ci resta altro che scrivere il **codice C#** con cui recuperare i file Markdown, estrarne i relativi YAML Front Matter e renderizzare l'HTML.  
Partiamo prima di tutto dal modello _Post_, che vuole rappresentare il nostro documento e tutti i metadati ad esso associato:

```csharp
public class Post
{
    [YamlMember(Alias = "title", ApplyNamingConventions = false)]
    public string Title { get; set; }
    
    [YamlMember(Alias = "author", ApplyNamingConventions = false)]
    public string Author { get; set; }
    
    [YamlMember(Alias = "publishDate", ApplyNamingConventions = false)]
    public DateTime PublishDate { get; set; }
    
    [YamlMember(Alias = "summary", ApplyNamingConventions = false)]
    public string Summary { get; set; }
    
    [YamlMember(Alias = "headImagePath", ApplyNamingConventions = false)]
    public string HeadImagePath { get; set; }
    
    [YamlMember(Alias = "categories", ApplyNamingConventions = false)]
    public List<string> Categories { get; set; }

    public string Body { get; set; }
}
```

Come vedete è un modello molto semplice, la cui unica particolarità è l'aggiunta di attributi **YAMLMember**; questi derivano dal pacchetto NuGet **YAMLDotNet** citato prima e servono per andare ad associare in maniera corretta e senza ambiguità le proprietà settate nel documento a quelle del modello C#.

>Proprio per evitare qualsiasi tipo di ambiguità ho settato a _false_ anche la proprietà _ApplyNamingConventions_. Questa evita l'applicazione di naming convention che possono essere impostate quando andiamo a inizializzare il deserializzatore YAML.

Ora che abbiamo il modello non ci resta che popolarlo. Per fare ciò ho deciso di sfruttare la Dependency Injection integrata in ASP.NET Core, creando quindi un "servizio", così da non essere direttamente vincolato ad una particolare implementazione, ma poterla cambiare in base alle necessità.
> Potrei infatti decidere in futuro di non conservare i documenti sulla stessa soluzione del blog, ma potrei richiamarli da un servizio web esterno. Se decidessi di procedere così mi basterebbe creare una nuova implementazione della stessa interfaccia, mantenendo il resto del codice dell'applicazione Blazor intatto.

Questo è il codice della mia interfaccia _IPostService_:

```csharp
public interface IPostService
{
    Task<IEnumerable<string>> GetAllPostNamesAsync();
    
    Task<Post> GetPostMetadataAsync(string filename);

    Task<Post> GetPostAsync(string filename);
}
```

Tutto molto semplice: il servizio consiste in 3 metodi (asincroni):

1. **GetAllPostNamesAsync** - Restituisce l'elenco di tutti i post (che andrà a leggere come detto prima dal file JSON appositamente creato).
2. **GetPostMetadataAsync** - Restituisce un oggetto Post in cui tutte le proprietà sono valorizzate, eccetto il Body
3. **GetPostAsync** - Restituisce un oggetto Post, in cui tutte le proprietà sono popolate (Body compreso).

Definita l'interfaccia, non basta altro che creare la classe concreta relativa che ne andrà ad implementare i metodi:

```csharp
public class PostService : IPostService
{
    private readonly HttpClient _client;
    private const string _basePostPath = "assets/posts/documents/";

    public PostService(HttpClient client)
    {
        _client = client;
    }
}
```

Vedete come nel costruttore vado a valorizzare l'HttpClient, il quale sarà passato tramite DI.

Aggiungiamo ora i diversi metodi che implementano l'interfaccia.

#### GetAllPostNamesAsync

```csharp
public async Task<IEnumerable<string>> GetAllPostNamesAsync()
{
    var response = await _client.GetFromJsonAsync<JsonElement>("assets/posts/post-list.json");
    var postList = new List<string>();
    foreach (var post in response.GetProperty("post").EnumerateArray())
    {
        postList.Add(post.GetString());
    }

    return postList;
}
```

Come analizzato precedentemente per ottenere la lista dei post ho deciso di procedere con un file JSON d'appoggio, che andrò a recuperare tramite una classica richiesta GET.  
Il resto del codice serve a leggere l'elenco contenuto nella proprietà _post_ del file (il cui esempio abbiamo visto prima).

#### GetPostMetadataAsync

```csharp
public async Task<Post> GetPostMetadataAsync(string filename)
{
    var byteArray = await _client.GetByteArrayAsync(_basePostPath + filename);
    var fileString = Encoding.Latin1.GetString(byteArray, 0, byteArray.Length);

    var pipeline = new MarkdownPipelineBuilder()
        .UseYamlFrontMatter()
        .Build();

    var document = Markdown.Parse(fileString, pipeline);

    var yamlLines = document.Descendants<YamlFrontMatterBlock>().FirstOrDefault()
        .Lines.ToString();

    var yamlDeserializer = new DeserializerBuilder()
        .IgnoreUnmatchedProperties()
        .Build();

    var post = yamlDeserializer.Deserialize<Post>(yamlLines);
    return post;
}
```

Qui le cose cominciano a farsi più interessanti, se non altro perchè sfruttiamo i due pacchetti NuGet citati in precedenza.  
Le prime due righe fanno parte della solita chiamata GET per prendere il file .md desiderato; la particolarità è che non recupero direttamente la stringa, ma l'array di byte, che poi vado a trasformare in stringa con un encoding particolare; questo perchè ho notato che il metodo GetStringAsync dell'HttpClient in accoppiata con YamlDotNet creava strani artefatti con le lettere accentate italiane, che così vengono risolti.

Dopodichè, mettiamo in piedi **MarkDig**, e la prima operazione da fare è inizializzare la pipeline di parsing/rendering, in cui la nostra unica impostazione è data dal metodo _UseYamlFrontMatter()_ che le permette di riconoscere il relativo blocco che altrimenti verrebbe ignorato.  
Fatto ciò, procediamo con il parsing tramite l'oggetto _Markdown_ a cui daremo in pasto la nostra stringa e la pipeline contenente le configurazioni scelte. Questo creerà un oggetto logico di tipo _MarkdownDocument_, in cui sono rappresentate tutte le diverse sezioni ed elementi nel nostro documento originale

>La struttura è molto simile a quella dell'XmlDocument standard di C#.  

Proprio grazie a questo oggetto possiamo andare ad isolare la parte di YamlFrontMatter, ed è ciò che viene fatto con la variabile _yamlLines_, in particolare viene preso il primo blocco YAML (potrebbero idealmente essercene anche di più), recuperate le sue linee di testo e raggruppate tutte in un'unica stringa.

Ora che abbiamo in mano la stringa YAML, sfruttiamo l'altro pacchetto: **YAMLDotNet**.  
Anche qui il primo passo è definire le impostazioni del relativo deserializzatore; nel nostro caso specifichiamo che deve ignorare eventuali proprietà di cui non riesce a fare il match, così da evitare possibili errori in documenti che hanno più metadati di quelli che ci interessano attualmente.  
Terminiamo quindi il processo con una deserializzazione, la quale ci restituirà il nostro oggetto di tipo _Post_ con tutte le proprietà dei metadati valorizzate, e quindi senza Body.

#### GetPostAsync

```csharp
public async Task<Post> GetPostAsync(string filename)
{
    var fileString = await _client.GetStringAsync(_basePostPath + filename);

    var post = await GetPostMetadataAsync(filename);

    var pipeline = new MarkdownPipelineBuilder()
        .UseYamlFrontMatter()
        .Build();

    post.Body = Markdown.ToHtml(fileString, pipeline);

    return post;
}
```

E qui vediamo l'ultimo metodo del nostro _PostService_, il codice è molto simile al precedente, anzi, potete vedere che proprio il metodo precedente viene nuovamente richiamato per popolare i metadati del nostro post, ma qui adesso dobbiamo andare a valorizzare anche il _Body_. Questo deve contenere direttamente l'HTML con cui poi sarà mostrato il documento all'utente, quindi trasformeremo il Markdown contenuto nel file in HTML.  
Per fare questo è sufficiente chiamare il metodo _ToHtml_ a cui passeremo nuovamente una MarkdownPipeline configurata sempre con l'uso dello YAML Front Matter così che non venga confuso come parte del testo.

#### Program.cs

L'ultimo passaggio restante è quello di configurare il servizio, così che la nostra interfaccia venga implementata con il servizio giusto una volta a runtime. Qui nulla di molto diverso rispetto a quello che facciamo sempre in ASP.NET Core, basterà andare sul file _Program.cs_ e andremo a definire il metodo **Main** in questo modo:

```csharp
public static async Task Main(string[] args)
{
    var builder = WebAssemblyHostBuilder.CreateDefault(args);
    builder.RootComponents.Add<App>("#app");

    builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
    builder.Services.AddScoped<IPostService, PostService>();

    await builder.Build().RunAsync();
}
```

## Conclusione

Così facendo il "motore" del blog è pronto ad essere eseguito per recuperare i nostri documenti **Markdown**, estrarne i metadati dai relativi **YAML Front Matter**, e quindi convertirli in **HTML** per la visualizzazione.

Nel prossimo post andremo a concludere il percorso di creazione del nostro blog, andando ad analizzare assieme le componenti Blazor che si occuperano effettivamente di mostrare all'utente i post, i relativi metadati, ma soprattutto il contenuto.  
Continuate quindi a seguirmi per non perdervi gli aggiornamenti dal blog!

>Spero che questo post vi sia piaciuto e soprattutto vi sia stato utile, se volete commentarlo o avete bisogno di ulteriore supporto, potete liberamente commentare sulla relativa [GitHub Discussion](https://github.com/frifugio/blog/discussions/5), oppure mi trovate su [Twitter](https://twitter.com/Dragonriffi92)
