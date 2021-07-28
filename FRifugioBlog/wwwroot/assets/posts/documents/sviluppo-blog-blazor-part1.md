---
title: Come ho sviluppato il mio blog - Parte 1
author: Francesco Rifugio
headImagePath: assets/posts/images/sviluppo-blog-header.png
summary: Prima parte del percorso che spieghera' passo per passo come ho sviluppato questo blog con Blazor e GitHub Pages.
publishDate: 2021-07-27
categories: [dev, blazor, github, github-pages]
---

# Come ho sviluppato il mio blog - Parte 1

## Introduzione

Benvenuto sul mio blog! Come puoi notare questa Single-Page Application è hostata su GitHub Pages, e dietro alle quinte c'è un piccolo progetto fatto in casa e sviluppato con Blazor, in modalità WebAssembly self-hosted.

Se sei curioso di scoprire come sono riuscito a creare e a portare live tutto ciò, sei nel posto giusto!
Vedremo passo passo tutte le fasi necessarie, in un percorso che si dividerà in queste parti:

1. Setup progetto, repository GitHub

2. Automatizzazione publish su GitHub Pages tramite GitHub Actions

3. Gestione Markdown e YAML Front Matter - Cuore funzionale del blog

4. Layout e componenti Blazor

## Requisiti

* .NET 5 - [Scarica ora](https://dotnet.microsoft.com/download)
* Visual Studio 2019, con il workload _ASP.NET and web development_ installato
  * Potete scaricare in modo gratuito la [Community Edition](https://visualstudio.microsoft.com/it/vs/community/).
  * In alternativa potete utilizzare [Visual Studio Code](https://code.visualstudio.com/download), ma i passaggi potrebbero essere leggermente diversi.
* Account GitHub - [Registrazione](https://github.com/join)

## Setup progetto

Iniziamo con la creazione del progetto su Visual Studio 2019:

* Andiamo a selezionare Blazor WebAssembly App dalla schermata di creazione del nuovo progetto
  > Se nella vostra lista non compaiono i template Blazor, assicuratevi di aver installato il workload ASP.NET and web development tramite il Visual Studio Installer.

* Inseriamo tutte le informazioni su nome del progetto e posizione in locale
* Nell'ultimo passaggio lasciamo tutto come in immagine: ![github-repo](assets/posts/images/sviluppo-blog-projsetup.png)
* Clicchiamo sul bottone _Create_ e terminiamo la creazione iniziale.

Fatto ciò per adesso lasciamo tutto così com'è e passiamo a GitHub.

## Setup repository GitHub

Spostiamoci quindi su [GitHub](https://github.com/), e dopo aver fatto login, procediamo alla creazione del nuovo repository (potete cliccare [qui](https://github.com/new) oppure andare sul pulsante verde _New_).
Compilate i campi come preferite, non preoccupatevi di aggiungere ora un _.gitignore_ - sarà poi aggiunto automaticamente da Visual Studio.

L'unica cosa da considerare è che la parte finale dell'URL della vostra SPA sarà il nome del repository (che comunque potrete cambiare in seguito alla prima creazione).
> L'URL finale infatti sarà così composto:
**[USERNAME].github.io/[NOME_REPO]**

![github-repo](assets/posts/images/sviluppo-blog-github-repo.png)

Fatto ciò si può tornare su Visual Studio per collegare il codice locale al repository online; è tutto molto semplice, basta sfruttare l'interfaccia grafica che ci offre proprio il nostro IDE.
Quindi, mantenendo aperto il progetto creato precedentemente, ci spostiamo sul menù in alto _Git_ e clicchiamo su _Create a Git Repository_, infine, nella finestra che si aprirà, ci spostiamo su _Existing remote_, come visibile nell'immagine:

![github-vsrepo](assets/posts/images/sviluppo-blog-github-vsrepo.png)

> Nota: Potevamo anche unire i due punti precedenti e fare tutto un unico punto sempre tramite questa schermata, ma ho preferito dividere i passaggi per vedere tutto più nel dettaglio.

Il Remote URL del nostro repository si può trovare sulla pagina GitHub dello stesso, cliccando sul pulsante verde evidenziato nell'immagine:

![github-remote](assets/posts/images/sviluppo-blog-github-remote.png)

Bene, ora che tutto è configurato, dobbiamo solo fare il primo push del codice sul branch principale. Per fare ciò possiamo sempre sfruttare la nuova schermata _Git Changes_ di Visual Studio, andando quindi a mettere in stage tutti i file del progetto, inserendo un messaggio di commit, e chiudendo il giro con il push.
> Se non trovate questa vista, che di default si trova sulla destra, in alternativa al _Solution Explorer_ , vi basta andare sul menù in alto _Views_ e quindi _Git Changes_


> Se non avete mai utilizzato Git in Visual Studio, vi verrà chiesto con un messaggio in alto alla vista di configurare i parametri di username e email con cui saranno inviati i commit, fare questo prima di procedere nel caso.

![github-vschanges](assets/posts/images/sviluppo-blog-github-vschanges.png)

Ora che il ramo principale (e al momento unico) è sistemato, c'è solo un'ultimo step: la creazione di un nuovo branch vuoto, su cui sarà caricato il codice compilato che verrà poi reso visibile tramite Github Pages.
Per fare questo ci bastano pochi comandi Git, che potete eseguire su un qualsiasi terminale (eventualmente anche quello presente in Visual Studio, accessibile da _View_ -> _Terminal_).

* ` git checkout --orphan gh-pages `
  * _gh-pages_ è il nome del branch che stiamo creando, non è obbligatorio, è semplicemente una naming convention di GitHub, ma potete cambiarlo a piacere.
* Si rimuovono tutti i file eventualmente in area di staging, così che non ne venga fatto il commit:
  * ` git rm -rf . `
* Abbiamo un branch vuoto in locale, prima di farne il push su GitHub, abbiamo bisogno di almeno un commit, anche senza contenuto (_empty commit_); non può essere infatti fatto il push di un branch completamente vuoto, quindi eseguiamo:
  * ` git commit --allow-empty -m "root commit" `
* Ora possiamo fare il push sul remote:
  * ` git push origin gh-pages `

Ci spostiamo ora sul nostro repo GitHub, per verificare che il branch appena pushato sia presente, e per configurarlo come base per Pages.
Basta semplicemente andare nella scheda delle Impostazioni del nostro repository, e nella scheda _Pages_ selezionare il nostro branch appena creato, come vedete nell'immagine sotto:

![github-ghpages](assets/posts/images/sviluppo-blog-github-ghpages.png)

A questo punto il setup iniziale è fatto, ma la nostra webapp non sarà ancora visibile, per fare questo sfrutteremo le **GitHub Actions**, ma le vedremo approfonditamente nel prossimo post!

>Spero che questo post vi sia piaciuto e soprattutto vi sia stato utile, se volete commentarlo o avete bisogno di ulteriore supporto, potete liberamente commentare sulla relativa [GitHub Discussion](https://github.com/frifugio/blog/discussions/5), oppure mi trovate su [Twitter](https://twitter.com/Dragonriffi92)
