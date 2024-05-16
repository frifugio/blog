---
title: Leggere un Blob Storage privato via API Management
author: Francesco Rifugio
headImagePath: assets/posts/images/apim-blob-header.png
summary: Una breve guida in cui capire come si puó ottenere un file da uno storage account privato su Azure, unicamente via API Management.
publishDate: 2024-05-16
categories: [dev, azure, api-management, blob, storage, authorization]
---

# Leggere un Blob Storage privato via API Management

In questi giorni sto rimettendo un po' mano ad alcuni servizi Azure, in particolar modo al mio preferito: Azure API Management; mentre al tempo stesso riflettevo su quanto puó essere facile, ma anche pericoloso ottenere un file caricato su un blob storage pubblico.  
Da qui l'idea: "e se usassi le potenzialitá delle policy di API Management per accedere un blob storage _privato_, cosí che lo stesso sia protetto da qualsiasi altro tipo di accesso esterno?".

E cosí mi sono messo all'opera, scoprendo che lo scenario in questione é abbastanza semplice da implementare, al netto di avere tutti i dettagli a disposizione, e visto che é stato proprio quest'ultimo punto il piú complicato, ho deciso di raccogliere in questa breve guida tutte le informazioni necessarie.

## Prerequisiti

Partiamo dando per scontato che si abbiano giá creati e pronti i due servizi fondamentali per procedere:

- [Azure API Management](https://learn.microsoft.com/en-us/azure/api-management/api-management-key-concepts) (o, in breve, APIM)
- [Azure Blob Storage account](https://learn.microsoft.com/en-us/azure/storage/blobs/storage-blobs-overview)
  - Allo scopo di questa guida, l'accesso anonimo ai blob deve essere disabilitato. Questa impostazione puó essere verificata via _Settings -> Configuration -> Allow Blob anonymous access -> Disabled_
    - In questo modo, é possibile verificare che se si prova a leggere il file tramite link diretto via browser, riceveremo un 409, a differenza dell'accesso via APIM al termine della guida. 
  - Al cui interno é giá presente un container, con il file che vogliamo leggere via APIM
  
indifferentemente da come essi siano creati: Azure CLI, Portale o Terraform etc.  
In questa guida mostreremo principalmente screen dal portale, semplicemente per facilitá di comprensione, ma i concetti possono essere facilmente riapplicati a qualsiasi strumento di deploy preferiate.

## APIM - Managed identities

L'obiettivo é utilizzare Azure RBAC, e Microsoft Entra ID per l'accesso autorizzato al Blob; questo é il metodo consigliato anche da Microsoft come piú sicuro, rispetto ad esempio all'utilizzo della chiave di accesso del Blob Storage.

Per fare ció, il primo step é quello di definire una "system assigned managed identity" per il nostro API Management.  
Ci basta quindi accedere alla schermata _Security -> Managed identities_ e cambiare il toggle su "On".  
A questo punto dopo aver atteso qualche secondo che l'impostazione si aggiorni, possiamo cliccare sul pulsante "Azure role assignments", come mostrato in figura:

![managed-identities](assets/posts/images/apim-blob-1.png)

Dopodiché, nella schermata che si aprirá, ci basta cliccare su "Add role assignment" in alto, e compilare il form come in figura: 

![azure-role-assignments](assets/posts/images/apim-blob-2.png)

> - Scope: Storage  
> - Subscription: < inserire la propria subscription >  
> - Resource: < inserire il nome dello storage account creato come prerequisito >  
> - Role: Storage Blob Data Reader 

Bene, in questo modo abbiamo fornito al nostro API Management la possibilitá di farsi riconoscere dallo Storage Account, e poter leggere i contenuti di un container e dei relativi blob.  
Questo peró non é sufficiente, perché adesso dobbiamo configurare la nostra API (in particolare la singola "operation" che si occuperá di leggere il nostro blob), in modo che fornisca l'autorizzazione tramite il permesso appena definito.

Spostiamoci quindi nella sezione **APIs**.

## API Management - Configurazione API

Diamo per scontato di aver giá creato una nuova API, oppure possiamo semplicemente aggiungere una nuova "operation" all'interno della "Echo API" che viene creata di default.  
Qui dobbiamo concentrarci sulla sezione **"Inbound processing"**, é qui che avverrá tutta la magia, e in cui definiremo tutto ció che é necessario per ottenere il blob con una richiesta HTTP autorizzata. 

Partiamo innanzitutto dal codice completo, e andiamo ad analizzare ogni singola sezione:

```xml
<policies>
    <inbound>
        <base />
        <set-backend-service base-url="https://<nome storage account>.blob.core.windows.net/<nome container>" />
        <rewrite-uri template="/<nome blob>" />
        <set-header name="x-ms-date" exists-action="override">
            <value>@(DateTime.UtcNow.ToString("R"))</value>
        </set-header>
        <set-header name="x-ms-version" exists-action="override">
            <value>2017-11-09</value>
        </set-header>
        <authentication-managed-identity resource="https://storage.azure.com/" />
    </inbound>
    <backend>
        <base />
    </backend>
    <outbound>
        <base />
    </outbound>
    <on-error>
        <base />
    </on-error>
</policies>
```
- `set-backend-service` - Qui impostiamo l'indirizzo del nostro container
- `rewrite-uri` - E utilizziamo questo comando per puntare direttamente al blob di nostro interesse. Nell'esempio questo é un nome fisso, ma é abbastanza facile poterlo rendere dinamico sulla base di un parametro della richiesta.
- `set-header` - Questo é utilizzato due volte, ed é necessario per impostare due header **obbligatori**, cosí come richiesti dalla [Storage API di Azure - Get Blob](https://learn.microsoft.com/en-us/rest/api/storageservices/get-blob?tabs=microsoft-entra-id#request-headers).
  - `x-ms-date` - Dove viene utilizzato direttamente del codice C# per ottenere la data attuale della richiesta
  - `x-ms-version` - Per il nostro metodo di autenticazione, la versione minima che é possibile utilizzare é la `2017-11-09`
  - **NOTA:** Questi due parametri sono FONDAMENTALI, altrimenti riceveremo una risposta fallimentare dallo storage account, con un errore difficile da comprendere.
- `authentication-managed-identity` - Questa é la policy che permette ad APIM di autenticarsi automaticamente all'identitá Microsoft Entra ID creata al passo precedente, ed aggiugnere alla richiesta verso lo Storage account il relativo token di Autorizzazione.

A questo punto possiamo testare la richiesta direttamente dal portale, e se tutto é stato configurato correttamente, avremo in risposta il blob richiesto!
> In alcuni casi ci puó essere qualche ritardo nell'aggiornamento dei permessi o delle policy di APIM. In caso riprovare piú volte.

>Spero che questo post vi sia piaciuto e soprattutto vi sia stato utile, se volete commentarlo o avete bisogno di ulteriore supporto, potete liberamente contattarmi su [X/Twitter](https://twitter.com/Dragonriffi92) o [LinkedIn](https://www.linkedin.com/in/francesco-rifugio/)
