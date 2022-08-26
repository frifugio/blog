---
title: Come ho sviluppato il mio blog - Parte 4
author: Francesco Rifugio
headImagePath: assets/posts/images/sviluppo-blog-4-header.png
summary: Ultima parte del percorso di sviluppo del blog. I componenti Blazor
publishDate: 2022-08-26
categories: [dev, blazor]
---

# Come ho sviluppato il mio blog - Parte 4

Benvenuti alla quarta e ultima parte di questo percorso che vuole mostrarvi tutti i passi che si sono resi necessari per portare live questo blog.  
Questo è ciò che abbiamo visto visto finora:

1. [Setup del repository Github e del progetto](https://frifugio.github.io/blog/post/sviluppo-blog-blazor-part1.md)
2. [GitHub Actions per automatizzare il deploy del blog ad ogni update](https://frifugio.github.io/blog/post/sviluppo-blog-blazor-part2-github-actions.md)
3. [Il "motore" del blog: dal Markdown all'HTML](https://frifugio.github.io/blog/post/sviluppo-blog-blazor-part3-markdown.md)

Quello che manca adesso è vedere come poter integrare tramite i differenti componenti Blazor tutto ciò che abbiamo sviluppato al punto 3 per mostrare l'HTML generato. Cominciamo!

## Componenti Blazor

Partiamo innanzitutto con il capire cos'è un componente Blazor, o, come più propriamente dovremmo chiamarlo, un componente _Razor_. Questo altro non è che una porzione autocontenuta di UI, con una parte di logica inclusa per permettere un contenuto dinamico.  
Il vantaggio principale di questi componenti è che possono essere riutilizzati, annidati e anche condivisi all'interno di diversi progetti.

La sintassi è quella classica Razor ([qui la documentazione ufficiale](https://docs.microsoft.com/en-us/aspnet/core/mvc/views/razor?view=aspnetcore-6.0)), e l'unica vera regola particolare è una naming convention: il nome del componente infatti deve iniziare con una lettera maiuscola, e solitamente si sfrutta il PascalCase.

Dalle definizioni viste sopra si può intuire come in realtà per componente Blazor possiamo intendere anche una vera e propria pagina di quella che sarà la nostra applicazione web, e difatti è così. Non ci basta fare altro che aggiungere

```razor
@page "/hello-world"
```

per avere una nuova pagina raggiungibile all'URL _/hello-world_; questo perchè quando viene compilata una pagina che contiene la direttiva **@page** viene generato automaticamente un routing apposito.  
Se invece vogliamo avere solo una porzione di codice riutilizzabile, ci basta non aggiungere questa direttiva all'inizio del nostro file e richiamare il componente in altri file (vedremo a brevissimo come fare).

Altra caratteristica fondamentale dei componenti Blazor è la capacità di gestire **parametri**, quindi informazioni che andranno a "personalizzare" a runtime la porzione di codice che viene ripetuta.

Questi vengono semplicemente gestiti come proprietà C#, decorate dall'attributo **[Parameter]**, come nell'esempio sotto:

```razor
@code {

    [Parameter]
    public string Title { get; set; }

}
```

Una volta scritto tutto il codice Razor, per richiamare il nostro componente sarà sufficiente andare ad inserire nella pagina padre un "tag HTML" identificato dal nome del componente.
Quindi, se ad esempio abbiamo creato un componente definito nel file CustomFooter.razor, con il parametro definito poc'anzi, per richiamarlo in un'altra pagina ci basterà scrivere:

```html
<CustomFooter Title="Made with <3 by Francesco Rifugio">
```

## PostCard

Ora che abbiamo le basi (e veramente le basi, avremmo molto altro di cui parlare) sui componenti Blazor, possiamo analizzare più nel dettaglio il codice che nel mio blog voglio riutilizzare per essere ripetuto dinamicamente a runtime.
L'idea è quella di creare una piccola "card" che mostri nella nostra _index_ i dettagli dei singoli post (in particolare i metadati che abbiamo aggiunto nello [step precedente](https://frifugio.github.io/blog/post/sviluppo-blog-blazor-part3-markdown.md)); per questo motivo ho chiamato il componente **PostCard**.

Innanzitutto partiamo da dove posizionare il nostro codice. Come spesso accade, anche qui possiamo sfruttare una convenzione che già troviamo nel progetto di default: inseriamo il nostro file _.razor_ nella cartella **Shared** adibita proprio a ospitare quei componenti che potenzialmente possono essere condivisi in più pagine.

Andiamo quindi ad analizzare il codice della nostra pagina _PostCard.razor_:

```razor
@using Models
@using Services
@inject IPostService PostService

<div class="card my-5">
    @if (!string.IsNullOrEmpty(Post.HeadImagePath))
    {
        <img class="card-img-top" src="@Post.HeadImagePath" alt="Post image cap">
    }
    <div class="card-body">
        <h5 class="card-title">@Post.Title</h5>
        <p class="card-text">@Post.Summary</p>
    </div>

    @if (Post.Categories.Any())
    {
        <div class="card-body">
            @foreach (var cat in Post.Categories)
            {
                <span class="badge badge-info mr-1">@cat</span>
            }
        </div>
    }

    <div class="card-footer">
        <small class="text-muted">@Post.PublishDate.ToShortDateString()</small>
    </div>
    <a class="stretched-link" href="post/@Filename" />
</div>

@code {

    [Parameter]
    public string Filename { get; set; }

    public Post Post { get; set; } = new Post();

    protected override async Task OnParametersSetAsync()
    {
        Post = await PostService.GetPostMetadataAsync(Filename);
    }
    
}
```

Partiamo dal fondo del file e analizziamo il blocco _@code_: qui troviamo due proprietà, di cui la prima **"Filename"** definita come parametro che identificherà il nome del file Markdown contenente tutti i dettagli del nostro post. Questo è il parametro che nella pagina _Index.razor_ utilizzerò per richiamare il componente, cosicchè ogni volta l'elemento conterrà i diversi dettagli dei diversi post.

```razor
<div class="container card-columns">
    @foreach (var post in PostList)
    {
        <PostCard Filename="@post" />
    }
</div>
```

La seconda proprietà - **Post** - è invece l'oggetto fondamentale che utilizzeremo all'interno del componente per renderizzare i diversi elementi definiti via CSHTML.
Come potete vedere questo viene inizializzato già in fase di definizione con una _new()_, ma noterete subito dopo che è nel metodo **OnParametersSetAsync** in cui _Post_ viene valorizzato con i dati "veri".
Questa sorta di doppio passaggio si rende necesario a causa del ciclo di vita di una pagina Blazor, che qui cercherò di riassumere in modo semplice e veloce, ma che potete analizzare più nel dettaglio nella [documentazione ufficiale](https://docs.microsoft.com/it-it/aspnet/core/blazor/components/lifecycle?view=aspnetcore-6.0):

- La prima fase è quella in cui vengono settati i parametri del componente, dopo il quale viene subito il rendering della pagina.  
  Qui nasce il primo problema: se non inizializzassi con la new la proprietà Post, quando Blazor cerca di renderizzare gli elementi HTML, prendendo le informazioni da essa, questa sarebbe _null_ portando quindi ad una _NullReferenceException_

- Sfrutto quindi la fase immediatamente successiva, in cui il parametro ora ha un valore che posso sfruttare per darlo in pasto al metodo _GetPostMetadataAsync_ definito in _PostService_, e inizializzare così correttamente la proprietà _Post_  
  A questo punto verrà lanciato da Blazor un secondo rendering e il componente verrà correttamente mostrato.

> Un consiglio per semplificare qualsiasi ragionamento: se nel vostro componente avete parametri, sfruttate il metodo **OnParametersSetAsync** per inizializzare il rendering iniziale; nel caso invece in cui non abbiate alcun parametro potete utilizzare il metodo **OnInitializedAsync**.

Tornando quindi all'inizio del file possiamo trovare le prime tre righe in cui abbiamo semplici importazioni di classi e la definizione della dependency injection per il PostService definito nella [parte precedente](https://frifugio.github.io/blog/post/sviluppo-blog-blazor-part3-markdown.md); dopodiché si comincia con la definizione dell'HTML, o più propriamente del CSHTML, perchè potete notare come viene sfruttata la sintassi Razor per andare ad innestare nell'HTML le proprietà dell'oggetto Post che saranno valorizzate solo a runtime.

## Conclusione

Abbiamo visto uno dei componenti fondamentali del blog: **PostCard**, così facendo abbiamo analizzato alcune parti fondamentali nella sua definizione, come i parametri e il ciclo di vita.  
Ovviamente questo non è l'unico componente Blazor che è stato definito al suo interno, ma è forse il più interessante; se vi interessa analizzare anche gli altri, in particolare **PostView** e **Index**, potete vederli direttamente sul [repo GitHub](https://github.com/frifugio/blog/tree/master/FRifugioBlog)

>Spero che questo post vi sia piaciuto e soprattutto vi sia stato utile, se volete commentarlo o avete bisogno di ulteriore supporto, potete liberamente commentare sulla relativa [GitHub Discussion](https://github.com/frifugio/blog/discussions/5), oppure mi trovate su [Twitter](https://twitter.com/Dragonriffi92)
