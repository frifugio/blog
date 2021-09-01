---
title: Come ho sviluppato il mio blog - Parte 2
author: Francesco Rifugio
headImagePath: assets/posts/images/sviluppo-blog-2-header.png
summary: Continua il percorso di costruzione del blog andando ad automatizzare il deploy tramite le Github Actions.
publishDate: 2021-09-01
categories: [dev, blazor, github, github-pages, github-actions]
---

# Come ho sviluppato il mio blog - Parte 2

## GitHub Action

Benvenuti alla seconda parte di questo percorso che vi mostrerà come questo blog è stato pubblicato su GitHub Pages. Se ti sei perso la prima parte, [comincia da qui!](https://frifugio.github.io/blog/post/sviluppo-blog-blazor-part1.md)

Ora è tutto pronto per rendere la pubblicazione della nostra single-page application del tutto automatica.

Vogliamo infatti che ad ogni push del nostro codice sul branch _master_ venga automaticamente fatto il deploy / publish sul branch _gh_pages_ cosicchè il motore automatico di GitHub Pages mostri la nostra webapp aggiornata.

Per fare ciò sfruttiamo le **GitHub Actions**, una funzionalità relativamente recente aggiunta in GitHub che ci permette proprio di automatizzare diversi flussi di CI/CD (Continuous Integration / Continuous Delivery).

> Se volete approfondire l'argomento vi rimando alla [documentazione ufficiale](https://docs.github.com/en/actions)

Per creare la nostra prima Action non dobbiamo far altro che spostarci sulla tab, e cliccare sul link indicato dall'immagine in basso:

![github-action-setup](assets/posts/images/sviluppo-blog-2-gitaction-set.png)

Come potete vedere il workflow che verrà eseguito è tutto gestito da un file YAML, che verrà memorizzato in _.github/workflows/_.
Se navigate nella schermata vedrete come a destra c'è un marketplace, ricco di action già pronte che possiamo riutilizzare, ma per ora iniziamo con lo svuotare il file così da poterlo compilare con solo le parti a noi necessarie.

La prima linea da aggiungere sarà la proprietà _name_. Diamogli un nome descrittivo così che sia sempre chiaro cosa viene eseguito:

```yaml
name: Deploy to GitHub Pages 
```

Dopodichè andremo a specificare il trigger che farà avviare il workflow. Come anticipato, vogliamo che ad ogni push sul master il sito compilato venga mostrato.

> Fra uno step e l'altro lasciare sempre una riga vuota

```yaml
# Lancia il workflow ad ogni push su master
on:
  push:
    branches: [ master ]
```

> (La prima riga identificata dal simbolo # è un commento in YAML)

Benissimo, a questo punto possiamo cominciare con la configurazione del relativo job e dell'immagine su cui girerà.
Anche qui cominciamo dandogli un nome (solitamente si utilizza la _kebab-case convention_ quindi daremo un nome del tipo _deploy-to-github-pages_), quindi lo impostiamo per girare su Ubuntu:

```yaml
jobs:
  deploy-to-github-pages:
    # Utilizzare l'immagine ubuntu-latest su cui far girare gli step
    runs-on: ubuntu-latest
    steps:
```

Andiamo subito a compilare la sezione _steps_ che per adesso è vuota con la prima azione da eseguire: il checkout del branch master
> Questa è una action che troviamo nel marketplace, utilizzata così come vediamo, senza parametri, farà in automatico il checkout del branch master

```yaml
    # git checkout su branch master
    - uses: actions/checkout@v2
```

Fatto ciò, possiamo lavorare sul nostro codice; ma la nostra è un'applicazione Blazor, quindi per prima cosa dobbiamo installare il _.NET Core SDK_ che non è presente sulla macchina Ubuntu scelta per far girare il job.
Anche in questo caso sfruttiamo una action che ci viene già fornita da GitHub e permette l'installazione dell'SDK in maniera molto facile:

```yaml
    # Installazione .NET Core SDK
    - name: Setup .NET Core SDK
      uses: actions/setup-dotnet@v1
```

Ora che l'SDK è installato, possiamo lanciare i suoi comandi per fare il publish del nostro codice come siamo abituati da terminale:

```yaml
    # Publish del progetto Blazor sulla cartella release
    - name: Publish .NET Core Project
      run: dotnet publish -c Release -o release --nologo
```

> L'argomento --nologo ci evita semplicemente linee inutili nell'output della console

Benissimo, a questo punto il grosso delle operazioni, se dovessimo semplicemente fare il deploy di una applicazione Blazor è fatto, però noi vogliamo che il blog sia hostato da GitHub Pages, che di base sfrutta altre tecnologie per la creazione di un blog (Jekyll).
I prossimi step che andremo a configurare nella nostra Action quindi servono proprio ad aggiustare alcuni problemi di "compatibilità" tra Blazor e GitHub Pages. Vediamoli nel dettaglio.

## Tag _base_ in _index.html_

Il problema fondamentale è che, se pubblicassimo (e vedremo dopo lo step necessario per farlo) il codice che ora abbiamo nella cartella _release_ sul branch gh-pages che abbiamo configurato nell'[articolo precedente](https://frifugio.github.io/blog/post/sviluppo-blog-blazor-part1.md), andrebbe tutto in errore per diversi motivi.
Il primo, è che l'applicazione cerca di recuperare le risorse dalla root del sito (_username.github.io/_) e non dalla sottocartella dedicata a GitHub Pages (che è _username.github.io/repo_name_). Questo accade a causa del tag **base** nella **head HTML** del file **index.html**, che vediamo qui sotto nella sua forma standard (così com'è nel nostro progetto):

```html
<!DOCTYPE html>
<html>
<head>
    ...
    <base href="/" />
    <link href="css/bootstrap/bootstrap.min.css" rel="stylesheet" />
    <link href="css/app.css" rel="stylesheet" />
</head>
<body>
    ...
    <script src="_framework/blazor.webassembly.js"></script>
</body>
</html>
```

Per risolvere questo comportamento quindi ci basta cambiare quel tag in: ` <base href="https://username.github.io/repo_name/" /> `  ma se lo facessimo direttamente all'interno del branch master, andremmo a rompere l'applicazione per lo sviluppo locale, per cui, anzichè farlo noi, lo lasciamo eseguire a GitHub Actions, andando ad aggiungere un ulteriore step:

```yaml
    # Cambio tag base in index.html da '/' a 'nome-repo' 
    - name: Change base-tag in index.html from / to my Repository Name
      run: sed -i 's/<base href="\/" \/>/<base href="\/nome-repo\/" \/>/g' release/wwwroot/index.html
```

> NB: Ovviamente anzichè _nome-repo_ dovrete inserire il nome del vostro repository

## Jekyll

Purtroppo non è finita qui, perchè se ci limitassimo al solo step appena compiuto la nostra webapp ci darebbe ancora errore, e questa volta centra **Jekyll**. Come anticipato questa è la tecnologia che di default utilizza GitHub Pages, e come parte di questo standard in automatico qualsiasi file o cartella che inizi con un underscore viene ignorato; e come abbiamo visto poco fa nel file **index.html** tutto il framework che permette a Blazor di funzionare tramite WebAssembly è contenuto dentro la cartella __framework_.

Per fortuna, questo comportamento può essere sovrascritto facilmente, ci basta aggiungere un file **.nojekyll** alla root del branch _gh-pages_. Per fare ciò potremmo aggiungerlo direttamente nel branch master, ma per non sporcarlo di cose non prettamente necessarie in fase di sviluppo, sfruttiamo nuovamente le GitHub Actions così che il workflow faccia questa operazione per noi; ci basta semplicemente aggiungere:

```yaml
    # Aggiunta di un file .nojekyll per bypassare lo standard di GitHub Pages
    - name: Add .nojekyll file
      run: touch release/wwwroot/.nojekyll
```

## BONUS: Pagina 404

Questo passaggio non è strettamente necessario al funzionamento della nostra webapp, ma può essere sicuramente utile a mantenere un look&feel consistente durante la navigazione del blog.
Infatti di default, in caso di pagina inesistente, GitHub Pages mostrerà la sua pagina 404 brandizzata, e, se questo è un problema, può essere risolto in due modi:

* Si crea un file **404.html** nella cartella **wwwroot** e la si committa sul repository, così una volta fatto il deploy GitHub comincerà automaticamente ad utilizzare quella
* Altrimenti, sfruttando sempre il workflow che stiamo costruendo su GitHub Actions, possiamo duplicare il file _index.html_ come _404.html_ e quindi pubblicarlo su _gh-pages_. In questo modo sarà il routing di Blazor a fare ciò che deve, e renderizzerà la sezione _NotFound_ che si trova in _App.razor_

Per effettuare il secondo passaggio vi basterà aggiungere questo step prima dell'aggiunta del file .nojekyll:

```yaml
    # File 404.html con routing di Blazor
    - name: copy index.html to 404.html
      run: cp release/wwwroot/index.html release/wwwroot/404.html
```

## Ultimi step

Perfetto, siamo quasi al termine della configurazione del nostro workflow. Ora che tutti i problemi sono stati preventivamente risolti, non ci resta altro da fare che committare i file compilati negli step precedenti e presenti nella cartella _release_ per portarli sul branch **gh-pages**.

Per fare ciò sfruttiamo un'altra action presente sul marketplace molto utile: [Deploy to GitHub Pages](https://github.com/marketplace/actions/deploy-to-github-pages):

```yaml
    - name: Commit wwwroot to GitHub Pages
      uses: JamesIves/github-pages-deploy-action@3.7.1
      with:
        BRANCH: gh-pages
        FOLDER: release/wwwroot
```

> Con l'argomento **BRANCH** impostiamo il branch su cui verrà fatto il push del codice (nel nostro caso **gh-pages**) mentre con l'argomento **FOLDER** specifichiamo da dove saranno presi tutti i file e le cartelle da pushare.

A questo punto il nostro file è pronto, non ci resta che farne la commit direttamente dalla schermata su cui abbiamo compilato il file finora, cliccando sui pulsanti indicati nell'immagine:

![github-action-commit](assets/posts/images/sviluppo-blog-2-gitaction-commit.png)

Una volta completato il commit, automaticamente verrà lanciato il workflow che potremo vedere sempre nella schermata Actions, con tanto di log step-by-step di tutte le configurazioni che abbiamo inserito:

![github-action-log](assets/posts/images/sviluppo-blog-2-gitaction-log.png)

## Conclusione

Abbiamo quindi visto come poter pubblicare in automatico, ad ogni push sul master, la nostra applicazione web scritta in **Blazor WebAssembly** tramite l'aiuto delle **GitHub Actions**. Questo ci ha permesso anche di risolvere facilmente alcuni problemi di compatibilità che troviamo in **GitHub Pages** (il tag base e il .nojekyll).

Di seguito trovate l'intero file YAML, nell'articolo spiegato passo passo:

```yaml
name: Deploy to GitHub Pages

# Lancia il workflow ad ogni push su master
on:
  push:
    branches: [ master ]

jobs:
  deploy-to-github-pages:
    # Utilizzare l'immagine ubuntu-latest su cui far girare gli step
    runs-on: ubuntu-latest
    steps:
    # git checkout su branch master
    - uses: actions/checkout@v2
    
    # Installazione .NET Core SDK
    - name: Setup .NET Core SDK
      uses: actions/setup-dotnet@v1

    # Publish del progetto Blazor sulla cartella release
    - name: Publish .NET Core Project
      run: dotnet publish -c Release -o release --nologo
    
    # Cambio tag base in index.html da '/' a 'nome-repo' 
    - name: Change base-tag in index.html from / to my Repository Name
      run: sed -i 's/<base href="\/" \/>/<base href="\/blog\/" \/>/g' release/wwwroot/index.html
    
    # File 404.html con routing di Blazor
    - name: copy index.html to 404.html
      run: cp release/wwwroot/index.html release/wwwroot/404.html

    # Aggiunta di un file .nojekyll per bypassare lo standard di GitHub Pages
    - name: Add .nojekyll file
      run: touch release/wwwroot/.nojekyll
      
    - name: Commit wwwroot to GitHub Pages
      uses: JamesIves/github-pages-deploy-action@3.7.1
      with:
        BRANCH: gh-pages
        FOLDER: release/wwwroot
```

>Spero che questo post vi sia piaciuto e soprattutto vi sia stato utile, se volete commentarlo o avete bisogno di ulteriore supporto, potete liberamente commentare sulla relativa [GitHub Discussion](https://github.com/frifugio/blog/discussions/5), oppure mi trovate su [Twitter](https://twitter.com/Dragonriffi92)
